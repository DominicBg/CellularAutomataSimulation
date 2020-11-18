using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ApplyTextureJob : IJobParallelFor
{
    public enum Blending { Override, OverrideAlpha }
    public NativeArray<Color32> outputColor;
    public NativeArray<Color32> texture;
    public Blending blending;

    public void Execute(int index)
    {
        switch (blending)
        {
            case Blending.Override:
                outputColor[index] = texture[index];
                break;
            case Blending.OverrideAlpha:
                if (texture[index].a != 0)
                    outputColor[index] = texture[index];
                break;
        }
    }
}
