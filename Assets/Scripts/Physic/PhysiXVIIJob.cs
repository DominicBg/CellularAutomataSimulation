﻿using Unity.Burst;
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

        bool isGrounded = PhysiXVII.IsGrounded(in physicData, map, physicData.gridPosition);
        if (isGrounded && !(physicData.velocity.y > 0))
        {
            physicData.velocity.y = 0; //set parallel of gravity
            physicData.velocity *= settings.friction;
        }
        else
        {
            physicData.velocity += settings.gravity;
        }

        float2 currentPosition = physicData.position;
        float2 nextPosition = currentPosition + (physicData.velocity + physicData.controlledVelocity) * deltaTime;
        int2 currentGridPosition = physicData.gridPosition;


        int2 nextGridPosition = (int2)(nextPosition / GameManager.GridScale);
        if (math.all(currentGridPosition == nextGridPosition))
        {
            physicData.position = nextPosition;
        }
        else
        {
            HandlePhysics(ref physicData, nextPosition, isGrounded);

            if (math.distancesq(currentPosition, nextPosition) > 0.01f)
            {
                map.RemoveSpriteAtPosition(currentGridPosition, ref physicData.physicBound);
                map.SetSpriteAtPosition(physicData.gridPosition, ref physicData.physicBound);
            }
        }

        physicData.position = math.clamp(physicData.position, 0, GameManager.GridSizes * GameManager.GridScale);
        physicData.gridPosition = math.clamp(physicData.gridPosition, 0, GameManager.GridSizes);
        physicDataReference.Value = physicData;
    }


    //public void HandlePhysics(ref PhysicData physicData, float2 desiredPosition)
    //{
    //    int2 nextGridPosition = (int2)(desiredPosition / GameManager.GridScale);

    //    int2 desiredGridPosition = FindDesiredMovePosition(ref physicData.physicBound, physicData.gridPosition, nextGridPosition);
    //    if (TryGoPosition(ref physicData.physicBound, physicData.gridPosition, desiredGridPosition))
    //    {
    //        if (math.all(nextGridPosition == desiredGridPosition))
    //        {
    //            physicData.position = desiredPosition;
    //            physicData.gridPosition = desiredGridPosition;
    //        }
    //        else
    //        {
    //            physicData.inclinaison = desiredGridPosition.y - nextGridPosition.y;

    //            physicData.position = desiredGridPosition * GameManager.GridScale;
    //            physicData.gridPosition = desiredGridPosition;
    //        }
    //    }
    //}

    public void HandlePhysics(ref PhysicData physicData, float2 desiredPosition, bool isGrounded)
    {
        int2 nextGridPosition = (int2)(desiredPosition / GameManager.GridScale);

        int2 desiredGridPosition = FindDesiredMovePosition(ref physicData, physicData.gridPosition, nextGridPosition, isGrounded, out int2 collisionDirection);
        if (math.all(nextGridPosition == desiredGridPosition))
        {
            physicData.position = desiredPosition;
            physicData.gridPosition = desiredGridPosition;
        }
        else
        {
            if(!math.all(collisionDirection == 0))
            {
                //could ez normalize with 0.707..
                float2 normal = math.normalize(collisionDirection);
                physicData.velocity = math.reflect(physicData.velocity, normal) * .5f;
            }

            physicData.inclinaison = desiredGridPosition.y - nextGridPosition.y;

            physicData.position = desiredGridPosition * GameManager.GridScale;
            physicData.gridPosition = desiredGridPosition;
        }
       
    }


    int2 FindDesiredMovePosition(ref PhysicData physicData, int2 from, int2 to, bool isGrounded, out int2 collisionDirection)
    {
        PhysicBound physicBound = physicData.physicBound;
        int2 desiredPosition = HandleHorizontalDesiredPosition(ref physicBound, from, to, isGrounded);
        int2 diff = desiredPosition - from;

        int2 safePosition = from;
        int2 currentPos = from;
        while (diff.x != 0 || diff.y != 0)
        {
            int2 currentDir = (int2)math.sign(diff);
            currentPos += currentDir;
            diff -= currentDir;
            Bound currentPosBound = physicBound.GetCollisionBound(currentPos);
            if(map.HasCollision(ref currentPosBound))
            {
                collisionDirection = GetCollisionDirection(ref physicBound, safePosition, currentDir);
                return safePosition;
            }
            safePosition = currentPos;
        }

        collisionDirection = 0;
        return safePosition;
    }

    int2 GetCollisionDirection(ref PhysicBound physicBound, int2 safePosition, int2 direction)
    {
        Bound horizontalBound = physicBound.GetCollisionBound(safePosition + new int2(direction.x, 0));
        Bound verticalBound = physicBound.GetCollisionBound(safePosition + new int2(0, direction.y));
        bool hasHorizontalCollision = map.HasCollision(ref horizontalBound);
        bool hasVerticalCollision = map.HasCollision(ref verticalBound);
        int2 collision = 0;
        if (hasHorizontalCollision)
            collision.x =- direction.x;
        if (hasVerticalCollision)
            collision.y = -direction.y;

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
            if (map.HasCollision(pos))
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

    bool TryGoPosition(ref PhysicBound physicBound, int2 from, int2 to)
    {
        int2 pushDirection = math.clamp(to - from, -1, 1);
        Bound bound = physicBound.GetCollisionBound(to);
        //Add push particles

        NativeList<int2> pushedParticlePositions = new NativeList<int2>(Allocator.Temp);
        bool isBlocked = false;
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            int2 position = positions[i];
            int2 pusedPosition = position + pushDirection;

            if (map.HasCollision(positions[i]))
            {
                bool canPush = map.CanPush(positions[i]) && map.IsFreePosition(pusedPosition);
                if (canPush)
                {
                    pushedParticlePositions.Add(positions[i]);
                }
                else
                {
                    isBlocked = true;
                    break;
                }
            }
        }

        if (!isBlocked)
        {
            for (int i = 0; i < pushedParticlePositions.Length; i++)
            {
                int2 position = pushedParticlePositions[i];
                int2 pusedPosition = position + pushDirection;
                map.MoveParticle(position, pusedPosition);
            }
        }

        positions.Dispose();
        pushedParticlePositions.Dispose();
        return !isBlocked;
    }
}