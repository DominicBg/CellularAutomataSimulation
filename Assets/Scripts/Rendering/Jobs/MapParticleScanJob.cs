using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct MapParticleScanJob : IJob
{
    public NativeList<int2> outputPositions;
    public NativeList<int> tickIdle;

    public Bound updateBound;
    public Map map;
    public ParticleType type;

    public void Execute()
    {
        int2 start = updateBound.bottomLeft;
        int2 end = updateBound.topRight;

        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                int2 pos = new int2(x, y);
                if (map.InBound(pos) && map.GetParticleType(pos) == type)
                {
                    outputPositions.Add(pos);
                    tickIdle.Add(map.GetParticle(pos).tickIdle);
                }
            }
        }      
    }
}
