using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PhysiXVII
{
    public static void HandlePhysics(PhysicObject physicObject, Map map)
    {
        physicObject.physicData.gridPosition = physicObject.position;

        NativeReference<PhysicData> physicDataReference = new NativeReference<PhysicData>(Allocator.TempJob);
        physicDataReference.Value = physicObject.physicData;
        new PhysiXVIIJob(map, GameManager.PhysiXVIISetings, GameManager.deltaTime, physicDataReference).Run();
        physicObject.physicData = physicDataReference.Value;
        physicDataReference.Dispose();

        physicObject.position = physicObject.physicData.gridPosition;
    }


    public static bool IsGrounded(in PhysicData physicData, Map map, int2 position)
    {
        Bound feetBound = physicData.physicBound.GetFeetCollisionBound(position);
        Bound underFeetBound = physicData.physicBound.GetUnderFeetCollisionBound(position);
        bool hasFeetCollision = map.HasCollision(ref feetBound);
        bool hasUnderFeetCollision = map.HasCollision(ref underFeetBound);
        bool atFloorLevel = position.y <= 0;
        return hasFeetCollision || hasUnderFeetCollision || atFloorLevel;
    }
}
