using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ImageTransitionJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeArray<Color32> leftImage;
    [ReadOnly] public NativeArray<Color32> rightImage;

    public float t;

    public void Execute(int index)
    {
        t = math.saturate(t);
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        int offset = (int)(t * GameManager.GridSizes.x);
        offset = math.clamp(offset, 0, GameManager.GridSizes.x);

        int2 samplePos = new int2(pos.x + offset, pos.y);
        bool useRightImage = (samplePos.x >= GameManager.GridSizes.x);
        samplePos.x %= GameManager.GridSizes.x;

        int sampleIndex = ArrayHelper.PosToIndex(samplePos, GameManager.GridSizes);
        outputColors[index] = useRightImage ? rightImage[sampleIndex] : leftImage[sampleIndex];
    }
}
