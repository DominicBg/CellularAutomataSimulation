using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class LightRenderer
{
    public static void AddLight(ref NativeArray<Color32> outputColors, ref NativeArray<LightSource> lightSources, int2 offset, BlendingMode blendingMode = BlendingMode.Transparency)
    {
        new AddLightJob()
        {
            outputColors = outputColors,
            lightSources = lightSources,
            blendingMode = blendingMode,
            offset = offset
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    [BurstCompile]
    public struct AddLightJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeArray<LightSource> lightSources;
        public BlendingMode blendingMode;
        public int2 offset;

        public void Execute(int index)
        {
            int2 pixelPos = ArrayHelper.IndexToPos(index, GameManager.GridSizes); 
            Color color = outputColors[index];
            for (int i = 0; i < lightSources.Length; i++)
            {
                color = lightSources[i].Blend(pixelPos + offset, color, blendingMode);
            }
            outputColors[index] = color;
        }
    }
}
