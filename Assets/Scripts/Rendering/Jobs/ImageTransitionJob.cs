using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ImageTransitionJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeArray<Color32> firstImage;
    [ReadOnly] public NativeArray<Color32> secondImage;

    public float t;
    public bool isHorizontal;

    public void Execute(int index)
    {
        t = math.saturate(t);
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        int size = isHorizontal ? GameManager.GridSizes.x : GameManager.GridSizes.y;

        int offset = (int)(t * size);
        offset = math.clamp(offset, 0, size);

        //int2 samplePos = new int2(pos.x + offset, pos.y);
        //bool useFirstImage = (samplePos.x >= size);
        //samplePos.x %= size;
        int2 samplePos = GetSamplePos(pos, size, offset, out bool useSecondImage);

        int sampleIndex = ArrayHelper.PosToIndex(samplePos, GameManager.GridSizes);
        outputColors[index] = useSecondImage ? secondImage[sampleIndex] : firstImage[sampleIndex];
    }

    int2 GetSamplePos(int2 pos, int size, int offset, out bool useSecondImage)
    {
        int2 samplePos;
        if (isHorizontal)
        {
            samplePos = new int2(pos.x + offset, pos.y);
            useSecondImage = (samplePos.x >= size);
            samplePos.x %= size;
        }
        else
        {

            samplePos = new int2(pos.x, pos.y + offset);
            useSecondImage = (samplePos.y >= size);
            samplePos.y %= size;
        }
        return samplePos;
    }
}
