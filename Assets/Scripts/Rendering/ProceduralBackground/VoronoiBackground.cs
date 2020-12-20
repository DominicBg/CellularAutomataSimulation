using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static GameMainMenuManager;


[System.Serializable]
public struct VoronoiRendering : IRenderableAnimated
{
    public int2 density;
    public uint seed;
    public float speed;
    public float maxDistance;
    public float step;
    public bool inverted;

    public void Render(ref NativeArray<Color32> colorArray, int tick)
    {
        new VoronoiBackgroundJob()
        {
            colors = colorArray,
            maxSizes = GameManager.GridSizes,
            settings = this,
            tick = tick
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
}

[BurstCompile]
public struct VoronoiBackgroundJob : IJobParallelFor
{
    public NativeArray<Color32> colors;
    public int2 maxSizes;
    public int tick;
    public VoronoiRendering settings;

    public void Execute(int index)
    {
        int2 cellSize = maxSizes / settings.density;

        //Find grid position and the corresponding cell
        int2 position = ArrayHelper.IndexToPos(index, maxSizes);
        int2 cellIndex = MathUtils.quantize(position, cellSize);

        float minDistance = int.MaxValue;
        //float maxDistance = int.MinValue;
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

                //Find closest point
                int2 point = gridBound.RandomPointInBound(ref random);
                float distance = math.distance(position, point);

                minDistance = math.min(minDistance, distance);
            }

        }

        float ratio = minDistance / settings.maxDistance;
        float step = math.floor(ratio * settings.step) / settings.step;

        if(settings.inverted)
        {
            colors[index] = Color.white - Color.white * math.saturate(step);
        }
        else
        {
            colors[index] = Color.white * math.saturate(step);
        }
    }
}
