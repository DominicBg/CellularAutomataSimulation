using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PhysiXVIIJob : IJob
{
    Map map;
    PhysiXVIISetings settings;
    float deltaTime;
    NativeReference<PhysicData> physicDataReference;

    public PhysiXVIIJob(Map map, PhysiXVIISetings settings, float deltaTime, NativeReference<PhysicData> physicData)
    {
        this.map = map;
        this.deltaTime = deltaTime;
        this.settings = settings;
        this.physicDataReference = physicData;
    }

    public void Execute()
    {
        PhysicData physicData = physicDataReference.Value;
        bool wasGrounded = physicData.isGrounded;
        physicData.isGrounded = PhysiXVII.IsGrounded(in physicData, map, physicData.gridPosition);
        bool hasFloorCollision = PhysiXVII.HasFloorCollision(in physicData, map, physicData.gridPosition);

        //TEST 
        //physicData.isGrounded = hasFloorCollision;

        if (!wasGrounded && physicData.isGrounded)
        {
            //Apply ground shock
            CalculateObjectParticleCollision(ref physicData, new float2(0, 1), new int2(0, 1));
        }

        if (hasFloorCollision && !(physicData.velocity.y > 0))
        {
            physicData.velocity.y = 0; //set parallel of gravity

            if(physicData.applyFriction)
                physicData.velocity *= settings.friction;
        }
        else
        {
            physicData.velocity += settings.gravity * deltaTime;
        }

        float2 currentPosition = physicData.position;
        float2 nextPosition = currentPosition + physicData.velocity * deltaTime;
        int2 currentGridPosition = physicData.gridPosition;


        int2 nextGridPosition = (int2)(nextPosition);
        if (math.all(currentGridPosition == nextGridPosition))
        {
            physicData.position = nextPosition;
        }
        else
        {
            int inclination = GetTerrainInclination(ref physicData, nextGridPosition);
            float inclinationSlowDown = inclination == 0 ? 1 : settings.slopeSlow;
            nextPosition = currentPosition + physicData.velocity * deltaTime * inclinationSlowDown;
            HandlePhysics(ref physicData, nextPosition, out int2 collisionNormal);

            if(physicData.hasCollision && collisionNormal.x != 0)
            {
                PhysiXVII.MoveUpFromPile(ref physicData, map, settings);
            }
        }

        physicDataReference.Value = physicData;

        map.RemoveSpriteAtPosition(currentGridPosition, ref physicData.physicBound);
        map.SetSpriteAtPosition(physicData.gridPosition, ref physicData.physicBound);
    }


    public void HandlePhysics(ref PhysicData physicData, float2 desiredPosition, out int2 collisionNormal)
    {
        physicData.hasCollision = false;
        physicData.collisionNormal = 0;
        physicData.collisionNormalNormalized = 0;

        int2 desiredGridPosition = (int2)(desiredPosition);

        int2 finalGridPosition = SlideCollider(ref physicData, physicData.gridPosition, desiredGridPosition, out collisionNormal, settings.limitAxis);

        physicData.debugCollisionNormal = collisionNormal;
        physicData.debugSafePosition = finalGridPosition;

        if (math.all(desiredGridPosition == finalGridPosition))
        {
            physicData.position = desiredPosition;
            physicData.gridPosition = finalGridPosition;
        }
        else
        {
            if(!math.all(collisionNormal == 0))
            {
                physicData.hasCollision = true;
                physicData.collisionNormal = collisionNormal;

                //Since values can only be -1, 0 and 1, its easy to normalize fast
                float2 normal = collisionNormal;
                if(math.abs(normal.x) + math.abs(normal.y) == 2)
                {
                    //this simulate normalization
                    // normal / sqrt(1^2 + 1^2)
                    // but normals are 1 or -1, so we can multiply by half sqrt2
                    normal *= math.SQRT2 * 0.5f;
                }
                physicData.collisionNormalNormalized = normal;
                CalculateObjectParticleCollision(ref physicData, normal, collisionNormal);
            }

            //Make sure you don't go faster uphill while going left
            bool2 moved = desiredGridPosition != finalGridPosition;
            float2 finalDesiredPosition;
            if (moved.x && !moved.y)
            {
                finalDesiredPosition = new float2(finalGridPosition.x, desiredPosition.y);

            }
            else if (!moved.x && moved.y)
            {
                finalDesiredPosition = new float2(desiredPosition.x, finalGridPosition.y);
            }
            else
            {
                finalDesiredPosition = finalGridPosition;
            }

            //Kill velocity on vertical collision
            if (collisionNormal.y != 0)
                physicData.velocity.y = 0;

            physicData.position = finalDesiredPosition;
            physicData.inclinaison = finalGridPosition.y - physicData.gridPosition.y;
            physicData.gridPosition = (int2)physicData.position;
        } 
    }

    int GetTerrainInclination(ref PhysicData physicData, int2 to)
    {
        int2 newPosition = SlideCollider(ref physicData, physicData.gridPosition, to, out _, settings.limitAxis);
        return to.y - newPosition.y;
    }

    void CalculateObjectParticleCollision(ref PhysicData physicData, float2 normal, int2 collisionDirection)
    {
        ref PhysicBound physicBound = ref physicData.physicBound;
        int2 position = physicData.gridPosition;

        float horizontalAbsorbtion = 1;
        float verticalAbsorbtion = 1;
        if (collisionDirection.x != 0)
        {
            int2 boundPos = position - new int2(collisionDirection.x, 0);
            Bound horizontalBound = (collisionDirection.x == 1) ? physicBound.GetLeftCollisionBound(boundPos) : physicBound.GetRightCollisionBound(boundPos);
            CalculateObjectParticleCollisionBound(ref physicData, horizontalBound, out horizontalAbsorbtion);
        }
        if (collisionDirection.y != 0)
        {
            int2 boundPos = position - new int2(0, collisionDirection.y);
            Bound verticalBound = (collisionDirection.y == 1) ? physicBound.GetBottomCollisionBound(boundPos) : physicBound.GetTopCollisionBound(boundPos);
            CalculateObjectParticleCollisionBound(ref physicData, verticalBound, out verticalAbsorbtion);
        }


        //make thing smoother?
        if (physicData.applyFriction)
        {
            float absorbtion = (horizontalAbsorbtion + verticalAbsorbtion) * 0.5f;
            physicData.velocity = math.reflect(physicData.velocity, normal) * (1 - absorbtion);
        }
    }

    unsafe void CalculateObjectParticleCollisionBound(ref PhysicData physicData, Bound bound, out float absorbtion)
    {
        int count = 0;
        absorbtion = 1;
        bound.GetPositionsGrid(out NativeArray<int2> pos);
        for (int i = 0; i < pos.Length; i++)
        {
            Particle particle = map.GetParticle(pos[i]);

            if(particle.type != ParticleType.Player && particle.type != ParticleType.None)
            {
                float mass = settings.mass[(int)particle.type];
                float2 centerPos = physicData.physicBound.GetCollisionBound(physicData.gridPosition).center + math.frac(physicData.position);
                PhysiXVII.ComputeElasticCollision(centerPos, pos[i] + particle.fracPosition, physicData.velocity, particle.velocity, physicData.mass, mass, out float2 outv1, out float2 outv2);
            
                //Add player?
                particle.velocity = outv2;
                map.SetParticle(pos[i], particle);

                absorbtion += settings.absorbtion[(int)particle.type];
                count++;
            }
        }
        if (count == 0)
            absorbtion = 1;
        else
            absorbtion /= count;

        pos.Dispose();
    }



    int2 GetCollisionNormal(ref PhysicBound physicBound, int2 safePosition, int2 direction)
    {
        //Move a bit horizontal/vertically to check if there's a collision
        Bound horizontalBound = physicBound.GetCollisionBound(safePosition + new int2(direction.x, 0));
        Bound verticalBound = physicBound.GetCollisionBound(safePosition + new int2(0, direction.y));
        bool hasHorizontalCollision = map.HasCollision(ref horizontalBound, PhysiXVII.GetFlag(ParticleType.Player));
        bool hasVerticalCollision = map.HasCollision(ref verticalBound, PhysiXVII.GetFlag(ParticleType.Player));
        int2 collision = 0;
        if (hasHorizontalCollision)
            collision.x = -direction.x;
        if (hasVerticalCollision)
            collision.y = -direction.y;

        //No horizontal and vertical collision, so its one pixel diagonal collision
        if(math.all(collision == 0))
        {
            return -direction;
        }

        return collision;
    }

    public int2 SlideCollider(ref PhysicData physicData, int2 from, int2 to, out int2 collisionNormal, bool2 limitAxis)
    {
        PhysicBound physicBound = physicData.physicBound;
        int2 diff = to - from;

        int2 safePosition = from;

        //Trace line on a grid
        int maxSteps = math.abs(diff.x) + math.abs(diff.y);
        float steps = 1f / (maxSteps == 0 ? 1 : maxSteps);
        bool2 blockedAxis = false;
        int2 blockedPos = 0;
        physicData.debugAxisBlocked = false;
        collisionNormal = 0;
        Bound currentPosBound;

        for (int i = 0; i <= maxSteps; i++)
        {
            int2 currentPos = (int2)math.lerp(from, to, i * steps);
            if (math.all(currentPos == safePosition))
                continue;

            if (blockedAxis.x && limitAxis.x)
                currentPos.x = safePosition.x;
            if (blockedAxis.y && limitAxis.x)
                currentPos.y = safePosition.y;

            currentPosBound = physicBound.GetCollisionBound(currentPos);

            int2 currentDir = math.clamp(currentPos - safePosition, -1, 1);
            if (map.HasCollision(ref currentPosBound, PhysiXVII.GetFlag(ParticleType.Player)))
            {
                //Match collision height
                if(GetSlopeHeight(ref physicBound, currentPos, settings.maxSlope, out int newYPos))
                {
                    currentPos.y = newYPos;
                }
                else
                {
                    collisionNormal = GetCollisionNormal(ref physicBound, safePosition, currentDir);

                    //This is to make sliding collision
                    //we limit the axis that is blocked but we still slide along the other axis
                    blockedAxis |= math.abs(collisionNormal) != 0;
                    if (blockedAxis.x && limitAxis.x)
                    {
                        blockedPos.x = safePosition.x;
                        currentPos.x = safePosition.x;
                    }
                    if (blockedAxis.y && limitAxis.y)
                    {
                        blockedPos.y = safePosition.y;
                        currentPos.y = safePosition.y;
                    }
                    physicData.debugAxisBlocked = blockedAxis;
                }
            }
            safePosition = currentPos;
        }

        return safePosition;
    }

    private bool GetSlopeHeight(ref PhysicBound physicBound, int2 position, int maxHeight, out int highestPosY)
    {
        bool canClimb = false;
        highestPosY = position.y;
        for (int i = 1; i < maxHeight; i++)
        {
            int2 pos = position + new int2(0, i);
            Bound getBoundAtPosition = physicBound.GetCollisionBound(pos);
            if (!map.HasCollision(ref getBoundAtPosition, PhysiXVII.GetFlag(ParticleType.Player)))
            {
                highestPosY = pos.y;
                return true;
            }
        }
        return canClimb;
    }
}