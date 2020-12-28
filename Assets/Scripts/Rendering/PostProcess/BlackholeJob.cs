using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct BlackholeSettings
{
    public float duration;
    public float waveSpeed;
    public float intensityMin;
    public float intensityMax;
}

[BurstCompile]
public struct BlackholeJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;

    public int2 position;
    public BlackholeSettings settings;
    public float t;

    public void Execute(int index)
    {
        int2 position = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        //float t = tickBlock.DurationSinceTick(startTick) / settings.duration;

        //t = math.sin(t * math.PI);

        float2 diff = position - position;

        float waveInnerRadius = t * settings.waveSpeed;
        float distSq = math.lengthsq(diff);

        if(distSq >= waveInnerRadius * waveInnerRadius)
        {
            float2 dir = diff / math.sqrt(distSq);
            float intensity = math.lerp(settings.intensityMin, settings.intensityMax, t);
            int2 sampleGridIndex = position + (int2)(dir * t * settings.waveSpeed * intensity);

            if(GridHelper.InBound(sampleGridIndex, GameManager.GridSizes))
            {
                int sampleIndex = ArrayHelper.PosToIndex(sampleGridIndex, GameManager.GridSizes);
                outputColors[index] = inputColors[sampleIndex];
            }
            else
            {
                outputColors[index] = Color.black;
            }
        }
        else
        {
            outputColors[index] = Color.black;
        }
    }
}
