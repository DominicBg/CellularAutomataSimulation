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
    public bool useAlphaMask;

    public ApplyTextureJob(NativeArray<Color32> outputColor, NativeArray<Color32> texture, BlendingMode blending, bool useAlphaMask = false)
    {
        this.outputColor = outputColor;
        this.texture = texture;
        this.blending = blending;
        this.useAlphaMask = useAlphaMask;
    }


    public void Execute(int index)
    {
        bool render = !useAlphaMask || (useAlphaMask && outputColor[index].a != 0);

        if (render)
        {
            outputColor[index] = RenderingUtils.Blend(outputColor[index], texture[index], blending);
        }
    }
}


public struct ApplyTextureBehindJob : IJobParallelFor
{
    public NativeArray<Color32> outputColor;
    public NativeArray<Color32> behindTexture;
    public BlendingMode blending;

    public ApplyTextureBehindJob(NativeArray<Color32> outputColor, NativeArray<Color32> texture, BlendingMode blending)
    {
        this.outputColor = outputColor;
        this.behindTexture = texture;
        this.blending = blending;
    }

    public void Execute(int index)
    {
        outputColor[index] = RenderingUtils.Blend(behindTexture[index], outputColor[index], blending);       
    }
}
