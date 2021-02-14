using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CreateEllipseJob : IJobParallelFor
{
    public NativeArray<Color32> outputColor;
    public float2 radius;
    public int2 position;
    public Color32 innerColor;
    public Color32 outerColor;
    public BlendingMode blending;
    public bool useAlphaMask;

    public void Execute(int index)
    {
        int2 pos = position - ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
        Color32 drawColor = outerColor;

        float2 inverseRadius = 1f / (radius * radius);
        float currentRadius = pos.x * pos.x * inverseRadius.x + pos.y * pos.y * inverseRadius.y;
        if (math.length(currentRadius) < 1)
        {
            drawColor = innerColor;
        }

        if (RenderingUtils.ShouldRender(outputColor[index], useAlphaMask))
        {
            outputColor[index] = RenderingUtils.Blend(outputColor[index], drawColor, blending);
        }
    }
}
