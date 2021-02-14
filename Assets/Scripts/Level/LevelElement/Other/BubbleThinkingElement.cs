using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BubbleThinkingElement : LevelElement
{
    public BubbleClusterSetting[] settings;
    public uint seed;

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    public override void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        return;

        NativeArray<Circle> nativeCircles = GenerateBubbleCircle(ref tickBlock);
        new DrawBubblesJob()
        {
            outputColors = outputColors,
            circles = nativeCircles
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        nativeCircles.Dispose();
    }

    NativeArray<Circle> GenerateBubbleCircle(ref TickBlock tickBlock)
    {
        int count = 0;
        for (int i = 0; i < settings.Length; i++)
        {
            count += settings[i].count;
        }
     
        NativeArray<Circle> nativeCircles = new NativeArray<Circle>(count, Allocator.TempJob);


        Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex(seed);

        int circleIndex = 0;
        for (int i = 0; i < settings.Length; i++)
        {
            for (int j = 0; j < settings[i].count; j++)
            {
                int2 staticOffset = random.NextInt2(-settings[i].randomness, +settings[i].randomness+1);
                float shakeSpeed = settings[i].shakeSpeed;
                float2 offset = new float2(
                    noise.cnoise(new float2(staticOffset.x * 50 + tickBlock.tick * shakeSpeed, staticOffset.y * 100 + tickBlock.tick * shakeSpeed)),
                    noise.cnoise(new float2(staticOffset.y * 50 + tickBlock.tick * shakeSpeed, staticOffset.x * 100 + tickBlock.tick * shakeSpeed))
                    );

                int2 dynamicOffset = (int2)(offset * settings[i].shakeAmplitude);

                nativeCircles[circleIndex] = new Circle()
                {
                    position = settings[i].position + staticOffset + dynamicOffset,
                    radius = settings[i].radius
                };
                circleIndex++;
            }
        }
        return nativeCircles;
    }

    [System.Serializable]
    public struct BubbleClusterSetting
    {
        public int radius;
        public int2 randomness;
        public int2 position;
        public int count;
        public float shakeSpeed;
        public float shakeAmplitude;
    }

    [BurstCompile]
    public struct DrawBubblesJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeArray<Circle> circles;
        const int minRadius = 3;

        public void Execute(int index)
        {
            int2 position = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);

            for (int i = 0; i < circles.Length; i++)
            {
                if(circles[i].radius <= minRadius)
                {
                    //Manhattan distance is better for small radius
                    int2 dist = math.abs(position - circles[i].position);
                    if((dist.x + dist.y) < circles[i].radius)
                    {
                        outputColors[index] = Color.white;
                        return;
                    }
                }
                else if(math.distancesq(position, circles[i].position) < circles[i].radius * circles[i].radius)
                {
                    outputColors[index] = Color.white;
                    return;
                }
            }
        }
    }

}
