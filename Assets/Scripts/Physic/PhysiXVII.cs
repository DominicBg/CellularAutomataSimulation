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
        new PhysiXVIIJob(map, GameManager.PhysiXVIISetings, GameManager.DeltaTime, physicDataReference).Run();
        physicObject.physicData = physicDataReference.Value;
        physicDataReference.Dispose();

        physicObject.position = physicObject.physicData.gridPosition;
    }

    [BurstCompile]
    public static bool IsGrounded(in PhysicData physicData, Map map, int2 position)
    {
        Bound feetBound = physicData.physicBound.GetBottomCollisionBound(position);
        Bound underFeetBound = physicData.physicBound.GetUnderFeetCollisionBound(position);
        bool hasFeetCollision = map.HasCollision(ref feetBound, GetFlag(ParticleType.Player), Allocator.Temp);
        bool hasUnderFeetCollision = map.HasCollision(ref underFeetBound, GetFlag(ParticleType.Player), Allocator.Temp);
        bool atFloorLevel = position.y <= 0;
        return hasFeetCollision || hasUnderFeetCollision || atFloorLevel;
    }

    [BurstCompile]
    public static void ComputeElasticCollision(float2 p1, float2 p2, float2 v1, float2 v2, float m1, float m2, out float2 outv1, out float2 outv2)
    {
        float massSum = m1 + m2;
        float diffSq = math.lengthsq(p1 - p2);

        outv1 = v1 - (2 * m2 / massSum) * (math.dot(v1 - v2, p1 - p2) / diffSq) * (p1 - p2);
        outv2 = v2 - (2 * m1 / massSum) * (math.dot(v2 - v1, p2 - p1) / diffSq) * (p2 - p1);
    }

    [BurstCompile]
    public unsafe static void CalculateParticleCollisions(ref Particle p1, ref Particle p2, int2 pos1, int2 pos2, in PhysiXVIISetings settings)
    {
        int p1Type = (int)p1.type;
        int p2Type = (int)p2.type;
        float m1 = settings.mass[p1Type];
        float m2 = settings.mass[p2Type];
        ComputeElasticCollision(pos1, pos2, p1.velocity, p2.velocity, m1, m2, out float2 outv1, out float2 outv2);
        float absorbtion = (settings.absorbtion[p1Type] + settings.absorbtion[p2Type]) * 0.5f;
        p1.velocity = outv1 * absorbtion;
        p2.velocity = outv2 * absorbtion;
    }

    [BurstCompile]
    public static void MoveUpFromPile(ref PhysicData physicData, Map map, in PhysiXVIISetings settings)
    {
        int2 position = physicData.gridPosition;
        int2 pushUp = new int2(0, 1);
        Bound bound = physicData.physicBound.GetTopCollisionBound(position + pushUp);
        var boundPos = bound.GetPositionsGrid();
        
        bool isBlocked = false;
        bool canSwap = false;
        for (int i = 0; i < boundPos.Length; i++)
        {
            //Need at least one non-None particle
            if (!canSwap && map.CanPush(boundPos[i], settings) && !map.IsFreePosition(boundPos[i]))
            {
                canSwap = true;
            }
            
            if (!map.IsFreePosition(boundPos[i]) && !map.CanPush(boundPos[i], settings))
            {
                isBlocked = true;
                break;
            }
        }

        if (!isBlocked && canSwap)
        {
            //Set particles at feet position, move up the character
            int yPos = physicData.physicBound.GetBottomCollisionBound(position).min.y;
            for (int i = 0; i < boundPos.Length; i++)
            {
                int2 oldPos = boundPos[i];
                int2 newPos = new int2(boundPos[i].x, yPos);
                ParticleType type = map.GetParticleType(oldPos);
                map.SetParticleType(newPos, type);
            }
            //SetPosition(position + pushUp);
            physicData.position = position + pushUp;
            physicData.gridPosition = position + pushUp;
        }
       
        boundPos.Dispose();
    }


    public static int GetFlag(ParticleType particleType)
    {
        return 1 << (int)particleType;
    }
    public static int GetFlag(params ParticleType[] particles)
    {
        int flag = 0;
        for (int i = 0; i < particles.Length; i++)
        {
            flag |= GetFlag(particles[i]);
        }
        return flag;
    }

    public static bool IsInFlag(int flag, ParticleType particleType)
    {
        return (GetFlag(particleType) & flag) != 0;
    }
}
