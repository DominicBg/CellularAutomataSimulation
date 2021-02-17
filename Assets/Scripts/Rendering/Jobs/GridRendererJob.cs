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

    int2 cameraPosition;
    bool debug;

    [ReadOnly] NativeArray<LightSource> lightSources;

    public GridRendererJob(NativeArray<Color32> colorArray, Map map, ParticleRendering particleRendering, TickBlock tickBlock, float2 cameraPosition, NativeArray<LightSource> lightSources, bool debug = false)
    {
        this.colorArray = colorArray;
        this.map = map;
        this.particleRendering = particleRendering;
        this.tickBlock = tickBlock;
        this.cameraPosition = (int2)cameraPosition;
        this.lightSources = lightSources;
        this.debug = debug;
    }

    public void Execute(int i)
    {
        int2 pos = cameraPosition + ArrayHelper.IndexToPos(i, GameManager.RenderSizes) - GameManager.RenderSizes/2;
        if (map.InBound(pos) && map.GetParticleType(pos) != ParticleType.None)
        {
            if(debug)
            {
                colorArray[i] = map.GetParticle(pos).InFreeFall() ? Color.green : Color.red;
                return;
            }

            Color32 color = ParticleRenderUtil.GetColorForType(pos, map.GetParticleType(pos), ref particleRendering, ref tickBlock, ref map, lightSources);
            colorArray[i] = (color == Color.clear) ? colorArray[i]: color;
        }
    }
}
