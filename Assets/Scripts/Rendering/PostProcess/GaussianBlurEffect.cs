using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class GaussianBlurEffect
{
    public static void Apply(ref NativeArray<Color32> outputColor)
    {
        NativeArray<float> kernelValues = new NativeArray<float>(5, Allocator.TempJob);

        kernelValues[0] = 0.06136f;
        kernelValues[1] = 0.24477f;
        kernelValues[2] = 0.38774f;
        kernelValues[3] = 0.24477f;
        kernelValues[4] = 0.06136f;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColor, Allocator.TempJob);
        new GaussianBlurJob()
        {
            inputColors = outputColor,
            outputColors = inputColors,
            kernelValues = kernelValues,
            isHorizontal = true
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        new GaussianBlurJob()
        {
            inputColors = inputColors,
            outputColors = outputColor,
            kernelValues = kernelValues,
            isHorizontal = false
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        inputColors.Dispose();
        kernelValues.Dispose();
    }

    [BurstCompile]
    public struct GaussianBlurJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> inputColors;
        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeArray<float> kernelValues;

        public bool isHorizontal;

        public void Execute(int index)
        {
            int2 pixelPos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
            int halfSize = kernelValues.Length / 2;

            Color colorSum = Color.clear;
            for (int i = 0; i < kernelValues.Length; i++)
            {
                int2 offset = isHorizontal ? new int2(1, 0) : new int2(0, 1);

                int2 pos = pixelPos + offset * (i - halfSize);
                if(ArrayHelper.TryPosToIndex(pos, GameManager.RenderSizes, out int sampleIndex))
                {
                    colorSum += (Color)inputColors[sampleIndex] * kernelValues[i];
                }
            }
            outputColors[index] = colorSum;
        }
    }
}
