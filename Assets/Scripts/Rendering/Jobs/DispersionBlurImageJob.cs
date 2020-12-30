using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct DispersionBlurImageJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;
    public float fadeOff;
    public float dispersionFactor;
    public TickBlock tickBlock;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        Color32 input = inputColors[index];
        input = Color32.Lerp(input, Color.clear, fadeOff);

        int count = 0;
        float4 col = GetColor(pos, new int2(1, 0), ref count);
        col += GetColor(pos, new int2(-1, 0), ref count);
        col += GetColor(pos, new int2(0, 1), ref count);
        col += GetColor(pos, new int2(0, -1), ref count);
        col /= count;

        Color surrounding = new Color(col.x, col.y, col.z, col.w);
        outputColors[index] = Color.Lerp(input, surrounding, dispersionFactor);
    }

    float4 GetColor(int2 pos, int2 offset, ref int count)
    {
        if (!GridHelper.InBound(pos + offset, GameManager.GridSizes))
            return 0;

        count++;
        int index = ArrayHelper.PosToIndex(pos + offset, GameManager.GridSizes);
        Color col = inputColors[index];
        return new float4(col.r, col.g, col.b, col.a);
    }
}
