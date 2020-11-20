using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static GameMainMenuManager;

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
                uint randomCellSeed = settings.seed + (uint)(currentCellIndex.x + currentCellIndex.y * 100);
                Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex(randomCellSeed);

                float starDistance = random.NextFloat();

                //Find closest star
                int2 starPosition = gridBound.RandomPointInBound(ref random);

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


        Color color = Color.white;
        color.a = math.saturate(alpha);
        colors[index] = color;
    }
}
