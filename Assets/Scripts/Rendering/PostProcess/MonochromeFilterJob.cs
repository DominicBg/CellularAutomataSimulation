using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

[BurstCompile]
public struct MonochromeFilterJob : IJobParallelFor
{
    public float threshold;
    public Color32 black;
    public Color32 white;
    public float blendWithOriginal;
    public NativeArray<Color32> outputColors;
    public void Execute(int i)
    {
        float t = RenderingUtils.Luminance(outputColors[i]);
        Color32 flashColor = t > threshold ? white : black;
        outputColors[i] = Color32.Lerp(outputColors[i], flashColor, blendWithOriginal);
    }
}
