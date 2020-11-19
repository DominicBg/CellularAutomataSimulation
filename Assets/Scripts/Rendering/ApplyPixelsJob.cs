using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ApplyPixelsJob : IJob
{
    public NativeArray<Color32> outputColor;

    public NativeArray<int2> pixelPositions;
    public NativeArray<Color32> pixelcolors;
    public int2 sizes;
    public BlendingMode blending;

    public ApplyPixelsJob(NativeArray<Color32> outputColor, NativeArray<int2> pixelPositions, NativeArray<Color32> pixelcolors, int2 sizes, BlendingMode blending)
    {
        this.outputColor = outputColor;
        this.pixelPositions = pixelPositions;
        this.pixelcolors = pixelcolors;
        this.sizes = sizes;
        this.blending = blending;
    }

    public void Execute()
    {
        for (int i = 0; i < pixelPositions.Length; i++)
        {
            int index = ArrayHelper.PosToIndex(pixelPositions[i], sizes);
            outputColor[index] = RenderingUtils.Blend(outputColor[index], pixelcolors[i], blending);
        }
    }
}
