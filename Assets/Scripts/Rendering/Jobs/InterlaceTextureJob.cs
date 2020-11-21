using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct InterlaceTextureSettings
{
    public int2 stride;
    public bool inverted;
}

[BurstCompile]
public struct InterlaceTextureJob : IJobParallelFor
{
    public NativeArray<Color32> outputColor;
    public NativeArray<Color32> colors;
    public InterlaceTextureSettings settings;
    public int2 mapSizes;

    public void Execute(int index)
    {
        int2 pixelPosition = ArrayHelper.IndexToPos(index, mapSizes);
        int2 moduloPos = pixelPosition % settings.stride * 2;

        bool2 smaller = moduloPos < settings.stride;
        bool show = (smaller.x && !smaller.y) || (!smaller.x && smaller.y);
        if (show == settings.inverted)
        {
            outputColor[index] = colors[index];
        }
    }
}
