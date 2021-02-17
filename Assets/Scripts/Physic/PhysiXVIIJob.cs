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

    //Find real slope
    //displace along the slope with speed modifyer


    public void Execute()
    {
        PhysicData physicData = physicDataReference.Value;
        bool wasGrounded = physicData.isGrounded;
        physicData.isGrounded = PhysiXVII.IsGrounded(in physicData, map, physicData.gridPosition);
        bool hasFloorCollision = PhysiXVII.HasFloorCollision(in physicData, map, physicData.gridPosition);

        if(!wasGrounded && physicData.isGrounded)
        {
            //Apply ground shock
            CalculateObjectParticleCollision(ref physicData, new float2(0, 1), new int2(0, 1), false);
        }

        if (hasFloorCollision && !(physicData.velocity.y > 0))
        {
            physicData.velocity.y = 0; //set parallel of gravity
            //physicData.velocity *= settings.friction;
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
            HandlePhysics(ref physicData, nextPosition, out bool hasCollision, out int2 collisionNormal);

            if(hasCollision && collisionNormal.x != 0)
            {
                PhysiXVII.MoveUpFromPile(ref physicData, map, settings);
            }
        }

        physicDataReference.Value = physicData;

        map.RemoveSpriteAtPosition(currentGridPosition, ref physicData.physicBound);
        map.SetSpriteAtPosition(physicData.gridPosition, ref physicData.physicBound);
    }


    public void HandlePhysics(ref PhysicData physicData, float2 desiredPosition, out bool hasCollision, out int2 collisionNormal)
    {
        hasCollision = false;
        int2 desiredGridPosition = (int2)(desiredPosition);

        int2 finalGridPosition = FindFinalMovePosition(ref physicData, physicData.gridPosition, desiredGridPosition, out collisionNormal);
        if (math.all(desiredGridPosition == finalGridPosition))
        {

            physicData.position = desiredPosition;
            physicData.gridPosition = finalGridPosition;
        }
        else
        {
            if(!math.all(collisionNormal == 0))
            {
                hasCollision = true;

                //Since values can only be -1, 0 and 1, its easy to normalize fast
                float2 normal = collisionNormal;
                if(math.abs(normal.x) + math.abs(normal.y) == 2)
                {
                    //this simulate normalization
                    // normal / sqrt(1^2 + 1^2)
                    // but normals are 1 or -1, so we can multiply by half sqrt2
                    normal *= math.SQRT2 * 0.5f;
                }
                
                CalculateObjectParticleCollision(ref physicData, normal, collisionNormal, false);
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

            physicData.position = finalDesiredPosition;
            physicData.inclinaison = finalGridPosition.y - physicData.gridPosition.y;
            physicData.gridPosition = (int2)physicData.position;
        } 
    }

    //This is slow
    int GetTerrainInclination(ref PhysicData physicData, int2 to)
    {
        int2 newPosition = FindFinalMovePosition(ref physicData, physicData.gridPosition, to, out _);
        return to.y - newPosition.y;
    }

    int2 FindFinalMovePosition(ref PhysicData physicData, int2 from, int2 to, out int2 collisionNormal)
    {
        PhysicBound physicBound = physicData.physicBound;
        int2 desiredPosition = HandleHorizontalDesiredPosition(ref physicBound, from, to, physicData.isGrounded);
        int2 diff = desiredPosition - from;

        //For slopes, works but is sloppy
        Bound currentPosBound = physicBound.GetCollisionBound(desiredPosition);
        if (physicData.isGrounded && math.abs(diff.x) <= 1 && !map.HasCollision(ref currentPosBound, PhysiXVII.GetFlag(ParticleType.Player)))
        {
            collisionNormal = 0;
            return desiredPosition;
        }

        int2 safePosition = from;

        //Trace line on a grid
        int maxSteps = math.abs(diff.x) + math.abs(diff.y);
        float steps = 1f / (maxSteps == 0 ? 1 : maxSteps);
        bool2 blockedAxis = false;
        int2 blockedPos = 0;
        collisionNormal = 0;

        for (int i = 0; i <= maxSteps; i++)
        {
            int2 currentPos = (int2)math.lerp(from, desiredPosition, i * steps);
            if (math.all(currentPos == safePosition))
                continue;

            if (blockedAxis.x)
                currentPos.x = safePosition.x;
            if (blockedAxis.y)
                currentPos.y = safePosition.y;

            currentPosBound = physicBound.GetCollisionBound(currentPos);

            int2 currentDir = math.clamp(currentPos - safePosition, -1, 1);
            if (map.HasCollision(ref currentPosBound, PhysiXVII.GetFlag(ParticleType.Player)))
            {
                 collisionNormal = GetCollisionNormal(ref physicBound, safePosition, currentDir);

                //This is to make sliding collision
                //we limit the axis that is blocked but we still slide along the other axis
                blockedAxis |= math.abs(collisionNormal) != 0;
                if (blockedAxis.x)
                {
                    blockedPos.x = safePosition.x;
                    currentPos.x = safePosition.x;
                }
                if (blockedAxis.y)
                {
                    blockedPos.y = safePosition.y;
                    currentPos.y = safePosition.y;
                }

                if (math.all(blockedAxis))
                { 
                    //collisionNormal = GetCollisionNormal(ref physicBound, safePosition, currentDir);
                    return safePosition;
                }
            }
            safePosition = currentPos;
        }
        collisionNormal = 0;
        return safePosition;
    }

    void CalculateObjectParticleCollision(ref PhysicData physicData, float2 normal, int2 collisionDirection, bool changeObjectVelocity = true)
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

        float absorbtion = (horizontalAbsorbtion + verticalAbsorbtion) * 0.5f;
         
        //make thing smoother?
        if(changeObjectVelocity)
            physicData.velocity = math.reflect(physicData.velocity, normal) * 0.1f; // absorbtion;
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


    private int2 HandleHorizontalDesiredPosition(ref PhysicBound physicBound, int2 from, int2 to, bool isGrounded)
    {
        int2 direction = (int2)math.sign(to - from);
        bool goingLeft = direction.x == -1;

        Bound horizontalBound = goingLeft ? physicBound.GetLeftCollisionBound(to) : physicBound.GetRightCollisionBound(to);

        int minY = horizontalBound.min.y;
        horizontalBound.GetPositionsGrid(out NativeArray<int2> directionPositions);
        int2 desiredPosition = to;

        if (isGrounded && CanClimb(minY, directionPositions, out int highestClimbY))
        {
            desiredPosition.y = highestClimbY;
        }
        return desiredPosition;
    }

    private bool CanClimb(int minY, NativeArray<int2> directionPositions, out int highestClimbY)
    {
        bool canClimb = false;
        highestClimbY = 0;
        for (int i = 0; i < directionPositions.Length; i++)
        {
            int2 pos = directionPositions[i];
            if (map.HasCollision(pos) && map.GetParticleType(pos) != ParticleType.Player)
            {
                if (pos.y >= minY && pos.y <= minY + settings.maxSlope)
                { 
                    canClimb = true;
                    highestClimbY = math.max(highestClimbY, pos.y + 1);
                }
            }
        }
        return canClimb;    
    }
}