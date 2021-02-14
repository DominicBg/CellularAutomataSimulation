using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class LightRenderer
{
    public static void AddLight(ref NativeArray<Color32> outputColors, ref NativeList<LightSource> lightSources, int2 renderingOffset, in LightRenderingSettings settings)
    {
        new AddLightJob()
        {
            outputColors = outputColors,
            lightSources = lightSources,
            settings = settings,
            renderingOffset = renderingOffset
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    [BurstCompile]
    public struct AddLightJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeList<LightSource> lightSources;
        public LightRenderingSettings settings;
        public int2 renderingOffset;
        public void Execute(int index)
        {
            int2 pixelPos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes) - renderingOffset; 
            Color color = outputColors[index];

            for (int i = 0; i < lightSources.Length; i++)
            {
                color = lightSources[i].Blend(pixelPos, color, settings.lightBlending);
            }
            outputColors[index] = RenderingUtils.BlendTransparentAdditive(outputColors[index], color, color.a, settings.additiveRatio);
        }
    }
}
