using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FogElement : LevelElement
{
    public FogSettings settings;
    ILightSource[] sources;

    public override void Init(Map map)
    {
        base.Init(map);
        sources = GetInterfaces<ILightSource>();
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }


    public override void PostRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        NativeArray<LightSource> lightSources = new NativeArray<LightSource>(sources.Length, Allocator.TempJob);
        for (int i = 0; i < lightSources.Length; i++)
        {
            LightSource source = sources[i].GetLightSource();
            lightSources[i] = source;
        }

        new FogRenderingJob()
        {
            outputColor = outputColor,
            settings = settings,
            lightSources = lightSources,
            tickBlock = tickBlock
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        lightSources.Dispose();
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
    }

    [BurstCompile]
    public struct FogRenderingJob : IJobParallelFor
    {
        public FogSettings settings;
        public NativeArray<Color32> outputColor;
        public TickBlock tickBlock;

        [ReadOnly] public NativeArray<LightSource> lightSources;

        public void Execute(int index)
        {
            const float small = 0.001f;
            int2 position = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
         
            float lightRatio = GetLightRatio(position);
            if (lightRatio == 0)
                return;
            
            float2 colorUV = GetUV(position, settings.colorSpeed * small);
            float3 colorNoisePos = new float3(colorUV.x, colorUV.y, tickBlock.tick * settings.noiseSpeed * small);

            float2 movingUV = GetUV(position, settings.movingSpeed * small);
            float3 movingNoisePos = new float3(movingUV.x, movingUV.y, tickBlock.tick * settings.noiseSpeed * small);

            float colorNoise = MathUtils.unorm(noise.snoise(colorNoisePos));
            float movingNoise = MathUtils.unorm(noise.snoise(movingNoisePos));

            Color color = Color.Lerp(settings.colorA, settings.colorB, colorNoise);

            color.a = math.remap(0, 1, settings.minAlpha, settings.maxAlpha, movingNoise) * lightRatio;

            outputColor[index] = RenderingUtils.Blend(outputColor[index], color, settings.blending).ReduceResolution(settings.resolution);
        }

        //Rescale la position + bouge par rapport au temps
        float2 GetUV(int2 position, float2 speed)
        {
            return position * settings.noiseScale + tickBlock.tick * speed;
        }

        float GetLightRatio(int2 position)
        {
            float ratio = 1;
            if (!lightSources.IsCreated)
                return ratio;

            for (int i = 0; i < lightSources.Length; i++)
            {
                LightSource source = lightSources[i];
                float distSq = math.distancesq(position, source.position);

                if (distSq > source.outerRadiusMax * source.outerRadiusMax)
                {
                    continue;
                }

                float lightInnerSin = math.sin(tickBlock.tick * source.speed);
                float innerRadius = math.lerp(source.innerRadiusMin, source.innerRadiusMin, lightInnerSin);
                float lightOuterSin = math.sin(tickBlock.tick * source.speed + source.offsynch);
                float outerRadius = math.lerp(source.outerRadiusMin, source.outerRadiusMax, lightOuterSin);

                if (distSq < innerRadius * innerRadius)
                {
                    return 0;
                }
                else if (distSq < outerRadius * outerRadius)
                {
                    float unlerp = math.unlerp(innerRadius, outerRadius, math.sqrt(distSq));
                    ratio = math.min(ratio, unlerp);
                }
            }
            return ratio;
        }
    }

    [System.Serializable]
    public struct FogSettings
    {
        public Color32 colorA;
        public Color32 colorB;
        public float minAlpha;
        public float maxAlpha;

        public float noiseSpeed;
        public float2 colorSpeed;
        public float2 movingSpeed;
        public float2 noiseScale;

        public float lightRatio;
        public BlendingMode blending;
        public int resolution;
    }


}

[System.Serializable]
public struct LightSource
{
    public int2 position;
    public int innerRadiusMin;
    public int innerRadiusMax;
    public int outerRadiusMin;
    public int outerRadiusMax;
    public float offsynch;
    public float speed;
}
public interface ILightSource
{
    LightSource GetLightSource();
}