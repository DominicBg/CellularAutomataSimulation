using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct IllusionEffectSettings : IPostEffect
{
    public float minIntensity;
    public float intensity;
    public float speed;
    public float duration;
    public float yoffset;
    //add ease?
}

[BurstCompile]
public struct IllusionEffectJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeArray<Color32> inputColors;
    public float t;
    public IllusionEffectSettings settings;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
        int offset = (int)(math.sin(t * math.PI * settings.speed + pos.y * settings.yoffset) * settings.intensity);
        int2 samplePos = pos + new int2(offset, 0);

        if(GridHelper.InBound(samplePos, GameManager.RenderSizes))
        {
            int sampleIndex = ArrayHelper.PosToIndex(samplePos, GameManager.RenderSizes);
            outputColors[index] = inputColors[sampleIndex];
        }
        else
        {
            outputColors[index] = Color.black;
        }
    }
}
