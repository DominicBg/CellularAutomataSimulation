using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ShakeScreenJobs : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColor;
    public NativeArray<Color32> outputColor;
    public int2 offset;
    public float blendWithOriginal;
    
    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        int2 posOffset = pos + offset;
        if(GridHelper.InBound(posOffset, GameManager.GridSizes))
        {
            Color32 colorOffset = inputColor[ArrayHelper.PosToIndex(posOffset, GameManager.GridSizes)];
            outputColor[index] = Color.Lerp(outputColor[index], colorOffset, blendWithOriginal);
        }
        else
        {
            outputColor[index] = Color.clear;
        }
    }
}
