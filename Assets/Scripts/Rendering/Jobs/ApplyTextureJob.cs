using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ApplyTextureJob : IJobParallelFor
{
    public NativeArray<Color32> outputColor;
    public NativeArray<Color32> texture;
    public BlendingMode blending;

    public ApplyTextureJob(NativeArray<Color32> outputColor, NativeArray<Color32> texture, BlendingMode blending)
    {
        this.outputColor = outputColor;
        this.texture = texture;
        this.blending = blending;
    }

    public void Execute(int index)
    {
        outputColor[index] = RenderingUtils.Blend(outputColor[index], texture[index], blending);
    }
}
