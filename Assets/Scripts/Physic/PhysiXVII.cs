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
        Bound feetBound = physicData.physicBound.GetBottomCollisionBound(position);
        Bound underFeetBound = physicData.physicBound.GetUnderFeetCollisionBound(position);
        bool hasFeetCollision = map.HasCollision(ref feetBound, (int)ParticleType.Player);
        bool hasUnderFeetCollision = map.HasCollision(ref underFeetBound, (int)ParticleType.Player);
        bool atFloorLevel = position.y <= 0;
        return hasFeetCollision || hasUnderFeetCollision || atFloorLevel;
    }

    public static void ComputeElasticCollision(float2 p1, float2 p2, float2 v1, float2 v2, float m1, float m2, out float2 outv1, out float2 outv2)
    {
        float massSum = m1 + m2;
        float diffSq = math.lengthsq(p1 - p2);

        outv1 = v1 - (2 * m2 / massSum) * (math.dot(v1 - v2, p1 - p2) / diffSq) * (p1 - p2);
        outv2 = v2 - (2 * m1 / massSum) * (math.dot(v2 - v1, p2 - p1) / diffSq) * (p2 - p1);
    }

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
}
