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
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;
    public int2 offset;
    public float blendWithOriginal;
    
    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        int2 posOffset = pos + offset;
        if(GridHelper.InBound(posOffset, GameManager.GridSizes))
        {
            Color32 colorOffset = inputColors[ArrayHelper.PosToIndex(posOffset, GameManager.GridSizes)];
            outputColors[index] = Color.Lerp(outputColors[index], colorOffset, blendWithOriginal);
        }
        else
        {
            outputColors[index] = Color.clear;
        }
    }
}
