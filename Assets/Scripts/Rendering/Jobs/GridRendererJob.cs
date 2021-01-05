using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GridRendererJob : IJobParallelFor
{
    public NativeArray<Color32> colorArray;
    Map map;
    ParticleRendering particleRendering;
    TickBlock tickBlock;
    int2 currentLevel;
    [ReadOnly] NativeArray<LightSource> lightSources;

    public GridRendererJob(NativeArray<Color32> colorArray, Map map, ParticleRendering particleRendering, TickBlock tickBlock, int2 currentLevel, NativeArray<LightSource> lightSources)
    {
        this.colorArray = colorArray;
        this.map = map;
        this.particleRendering = particleRendering;
        this.tickBlock = tickBlock;
        this.currentLevel = currentLevel;
        this.lightSources = lightSources;
    }

    public void Execute(int i)
    {
        int2 pos = ArrayHelper.IndexToPos(i, map.Sizes);
        if (map.GetParticleType(pos) != ParticleType.None)
        {
            int2 mapOffset = currentLevel * GameManager.GridSizes;
            Color32 color = ParticleRenderUtil.GetColorForType(pos + mapOffset, map.GetParticleType(pos), ref particleRendering, ref tickBlock, ref map, lightSources);
            colorArray[i] = color;
        }
    }
}
