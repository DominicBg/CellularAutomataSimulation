using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;


[BurstCompile]
public struct MonochromeFilterJob : IJobParallelFor
{
    [System.Serializable]
    public struct Settings
    {
        public float threshold;
        public Color32 color1;
        public Color32 color2;
        public float blendWithOriginal;
    }

    public Settings settings;
    public NativeArray<Color32> outputColors;
    public void Execute(int i)
    {
        float t = RenderingUtils.Luminance(outputColors[i]);
        Color32 flashColor = t > settings.threshold ? settings .color1: settings.color2;
        outputColors[i] = Color32.Lerp(outputColors[i], flashColor, settings.blendWithOriginal);
    }
}
