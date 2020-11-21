﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct StarBackgroundRendering : IRenderableAnimated
{
    public int2 density;
    public int radius;
    public uint seed;
    public float speed;
    public float2 sinOffsetScale;
    public float sinOffsetAmplitude;

    public void Render(ref NativeArray<Color32> colorArray, int tick)
    {
        new ShiningStarBackgroundJob()
        {
            colors = colorArray,
            maxSizes = GameManager.GridSizes,
            settings = this,
            tick = tick
        }.Schedule(GameManager.GridLength, 100).Complete();
    }
}

[BurstCompile]
public struct ShiningStarBackgroundJob : IJobParallelFor
{
    public NativeArray<Color32> colors;
    public int2 maxSizes;
    public int tick;
    public StarBackgroundRendering settings;

    public void Execute(int index)
    {
        int2 cellSize = maxSizes / settings.density;

        //Find grid position and the corresponding cell
        int2 position = ArrayHelper.IndexToPos(index, maxSizes);
        int2 cellIndex = MathUtils.quantize(position, cellSize);

        float alpha = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int2 currentCellIndex = cellIndex + new int2(x, y);

                //out of map cell
                if (!GridHelper.InBound(currentCellIndex, settings.density))
                    continue;

                Bound gridBound = new Bound(currentCellIndex * cellSize, cellSize);

                //Generate a random point in this cell
                var random = MathUtils.CreateRandomAtPosition(currentCellIndex, settings.seed);

                //Find closest star
                int2 starPosition = gridBound.RandomPointInBound(ref random);
                float starDistance = random.NextFloat();

                if (math.all(position == starPosition))
                {
                    //This pixel is a star
                    colors[index] = Color.white * starDistance;
                    return;
                }
                else
                {
                    float distance = math.distance(position, starPosition);
                    if (distance < settings.radius)
                    {
                        //Fade out of light
                        float ratioLight = 1 - math.saturate(distance / settings.radius);
                        float noiseSinOffset = settings.sinOffsetAmplitude * noise.cnoise(starPosition * settings.sinOffsetScale);
                        float sinValue = MathUtils.unorm(math.sin(tick * settings.speed + noiseSinOffset));
                        alpha += sinValue * ratioLight * starDistance;
                    }
                }
            }

        }


        colors[index] = Color.white * math.saturate(alpha);
    }
}
