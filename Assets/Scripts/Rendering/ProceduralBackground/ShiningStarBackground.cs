using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct StarBackgroundRendering : ITextureRenderableAnimated
{
    public int2 density;
    public int radius;
    public uint seed;
    public float speed;
    public float2 sinOffsetScale;
    public float sinOffsetAmplitude;
    [Range(0, 1)] public float maxAlpha;

    public static StarBackgroundRendering Default()
    {
        return new StarBackgroundRendering()
        {
            density = 5,
            radius = 3,
            seed = 0,
            sinOffsetAmplitude = 12643.2f,
            sinOffsetScale = new float2(23.51f, 126.367f),
            speed = 0.02f
        };
    }

    public void Render(ref NativeArray<Color32> colorArray, int tick, int2 offset)
    {
        new ShiningStarBackgroundJob()
        {
            colors = colorArray,
            maxSizes = GameManager.GridSizes,
            settings = this,
            tick = tick,
            offset = offset
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public void Render(ref NativeArray<Color32> colorArray, int tick)
    {
        Render(ref colorArray, tick, 0);
    }
}

[BurstCompile]
public struct ShiningStarBackgroundJob : IJobParallelFor
{
    public NativeArray<Color32> colors;
    public int2 maxSizes;
    public int tick;
    public StarBackgroundRendering settings;
    public int2 offset;

    public void Execute(int index)
    {
        int2 cellSize = maxSizes / settings.density;

        //Find grid position and the corresponding cell
        int2 position = ArrayHelper.IndexToPos(index, maxSizes) + offset;
        int2 cellIndex = MathUtils.quantize(position, cellSize);

        float alpha = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int2 currentCellIndex = cellIndex + new int2(x, y);

                Bound gridBound = new Bound(currentCellIndex * cellSize, cellSize);

                //Generate a random point in this cell
                var random = MathUtils.CreateRandomAtPosition(currentCellIndex, settings.seed);

                //Find closest star
                int2 starPosition = gridBound.RandomPointInBound(ref random);

                if (math.all(position == starPosition))
                {
                    //This pixel is a star
                    colors[index] = CalculateColor(colors[index], settings.maxAlpha);
                    return;
                }
                else
                {
                    float distancesq = math.distancesq(position, starPosition);
                    if (distancesq < settings.radius * settings.radius)
                    {
                        float distance = math.sqrt(distancesq);

                        //Fade out of light
                        float ratioLight = 1 - math.saturate(distance / settings.radius);
                        float noiseSinOffset = settings.sinOffsetAmplitude * noise.cnoise(starPosition * settings.sinOffsetScale);
                        float sinValue = MathUtils.unorm(math.sin(tick * settings.speed + noiseSinOffset));
                        alpha += sinValue * ratioLight;
                    }
                }
            }

        }

        colors[index] = CalculateColor(colors[index], math.saturate(alpha * settings.maxAlpha));
    }

    Color CalculateColor(Color baseColor, float alpha)
    {
        return RenderingUtils.Blend(baseColor, Color.white * alpha, BlendingMode.Transparency);
    }
}
