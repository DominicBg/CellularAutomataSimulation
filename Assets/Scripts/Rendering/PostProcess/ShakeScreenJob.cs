using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ShakeScreenJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;
    public float t;
    public PostProcessManager.ShakeSettings settings;
    public TickBlock tickBlock;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        float falloff = settings.useFalloff ? 1 - t * t * t : 1;
        float p = tickBlock.tick * settings.speed;
        float x = noise.cnoise(new float2(p, 100)) * settings.intensity * falloff;
        float y = noise.cnoise(new float2(100, p)) * settings.intensity * falloff;

        int2 offset = new int2((int)x, (int)y);
        if (math.all(offset == 0))
            return;

        int2 posOffset = pos + offset;
        if(GridHelper.InBound(posOffset, GameManager.GridSizes))
        {
            Color32 colorOffset = inputColors[ArrayHelper.PosToIndex(posOffset, GameManager.GridSizes)];
            outputColors[index] = Color.Lerp(outputColors[index], colorOffset, settings.blendWithOriginal * falloff);
        }
        else
        {
            outputColors[index] = Color.clear;
        }
    }
}
