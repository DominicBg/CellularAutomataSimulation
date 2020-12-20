using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ShockwaveJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;

    public PostProcessManager.ShockwaveSettings settings;

    public TickBlock tickBlock;
    public int startTick;

    public void Execute(int index)
    {
        int2 position = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        float t = tickBlock.DurationSinceTick(startTick) / settings.duration;
        
        //rofl add curve system pls
        t = 1 - (1-t) * (1 - t) * (1 - t) * (1 - t) * (1 - t);

        float2 diff = settings.centerPoint - position;

        float waveInnerRadius = t * settings.waveSpeed;
        float waveOuterRadius = t * settings.waveSpeed + settings.radiusThickness;
        float distSq = math.lengthsq(diff);

        if(distSq >= waveInnerRadius * waveInnerRadius && distSq <= waveOuterRadius * waveOuterRadius)
        {
            float dist = math.sqrt(distSq);
            float2 dir = diff / dist;
            int2 sampleGridIndex = position + (int2)(dir * t * settings.waveSpeed);

            float ratioThick = math.remap(waveInnerRadius, waveOuterRadius, 0, 1, dist);
            //extermities gives 0, center give 1
            ratioThick = math.sin(ratioThick * math.PI);

            if (GridHelper.InBound(sampleGridIndex, GameManager.GridSizes))
            {
                int sampleIndex = ArrayHelper.PosToIndex(sampleGridIndex, GameManager.GridSizes);
                float intensity = settings.intensity * ratioThick * (1 - t);
                outputColors[index] = Color.Lerp(outputColors[index], inputColors[sampleIndex], intensity);
            }
        }
    }
}
