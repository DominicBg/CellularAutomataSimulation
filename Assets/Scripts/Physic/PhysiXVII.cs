using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PhysiXVII
{
    public static bool IsGrounded(in PhysicData physicData, Map map, int2 position)
    {
        Bound feetBound = physicData.physicBound.GetFeetCollisionBound(position);
        Bound underFeetBound = physicData.physicBound.GetUnderFeetCollisionBound(position);
        bool hasFeetCollision = map.HasCollision(ref feetBound);
        bool hasUnderFeetCollision = map.HasCollision(ref underFeetBound);
        bool atFloorLevel = position.y <= 0;
        return hasFeetCollision || hasUnderFeetCollision || atFloorLevel;
    }

    public static void HandlePhysics(ref PhysicData physicData, Map map, float2 desiredPosition, Allocator allocator = Allocator.Temp)
    {
        int2 nextGridPosition = (int2)(desiredPosition / GameManager.GridScale);

        int2 desirGridPosition = FindDesiredMovePosition(ref physicData.physicBound, map, physicData.gridPosition, nextGridPosition, allocator);
        if (TryGoPosition(ref physicData.physicBound, map, physicData.gridPosition, desirGridPosition))
        {
            physicData.position = desirGridPosition * GameManager.GridScale;
            physicData.gridPosition = desirGridPosition;
        }
    }


    static int2 FindDesiredMovePosition(ref PhysicBound physicBound, Map map, int2 from, int2 to, Allocator allocator)
    {
        int direction = (to.x - from.x);
        bool goingLeft = direction == -1;

        Bound directionBound;
        if (goingLeft)
        {
            directionBound = physicBound.GetLeftCollisionBound(to);
        }
        else
        {
            directionBound = physicBound.GetRightCollisionBound(to);
        }

        int minY = directionBound.min.y;
        directionBound.GetPositionsGrid(out NativeArray<int2> directionPositions, allocator);
        int2 desiredPosition = to;

        int slopeLimit = 2;
        bool canClimb = false;
        int highestClimbY = 0;
        for (int i = 0; i < directionPositions.Length; i++)
        {
            int2 pos = directionPositions[i];
            if (map.HasCollision(pos))
            {
                if (pos.y >= minY && pos.y <= minY + slopeLimit)
                {
                    canClimb = true;
                    highestClimbY = math.max(highestClimbY, pos.y + 1);
                }
            }
        }

        if (canClimb)
        {
            desiredPosition.y = highestClimbY;
        }
        return desiredPosition;
    }

    static bool TryGoPosition(ref PhysicBound physicBound, Map map, int2 from, int2 to)
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
