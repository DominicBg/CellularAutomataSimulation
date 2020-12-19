﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GridRendererJob : IJobParallelFor
{
    public NativeArray<Color32> colorArray;
    Map map;
    ParticleRendering particleRendering;
    TickBlock tickBlock;

    public GridRendererJob(NativeArray<Color32> colorArray, Map map, ParticleRendering particleRendering, TickBlock tickBlock)
    {
        this.colorArray = colorArray;
        this.map = map;
        this.particleRendering = particleRendering;
        this.tickBlock = tickBlock;
    }

    public void Execute(int i)
    {
        int2 pos = ArrayHelper.IndexToPos(i, map.Sizes);
        if (map.GetParticleType(pos) != ParticleType.None)
        {
            Color32 color = ParticleRenderUtil.GetColorForType(pos, map.GetParticleType(pos), ref particleRendering, ref tickBlock);
            colorArray[i] = color;
        }
    }
}
