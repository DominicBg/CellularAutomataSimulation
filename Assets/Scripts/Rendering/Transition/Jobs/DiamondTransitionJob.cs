using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct DiamondTransitionJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeArray<Color32> firstImage;
    [ReadOnly] public NativeArray<Color32> secondImage;

    public float t;
    public int diamondSize;
    public Color32 color;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        if(t < 0.5f)
        {
            //remap [0, .5] -> [0, 1]
            float tt = t * 2;
            outputColors[index] = IsDiamond(pos, tt) ? firstImage[index] : color;
        }
        else
        {
            //remap [.5, 1] -> [0, 1]
            float tt = (t - 0.5f) * 2;
            outputColors[index] = IsDiamond(pos, tt) ? color : secondImage[index];
        }
    }

    bool IsDiamond(int2 pos, float t)
    {
        int2 repeatPosition = pos % (2 * diamondSize);
        int currentSize = (int)math.lerp(0, 2 * diamondSize + 1, t);

        int2 centerPos = diamondSize;
        int2 diff = repeatPosition - centerPos;
        int manhattanDist = math.abs(diff.x) + math.abs(diff.y);
        return manhattanDist >= currentSize;    
    }
}
