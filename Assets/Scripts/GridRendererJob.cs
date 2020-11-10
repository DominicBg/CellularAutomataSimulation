
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static GridRenderer;

[BurstCompile]
public struct GridRendererJob : IJobParallelFor
{
    //[ReadOnly] public NativeArray<Particle> particles;
    public NativeArray<Color32> colorArray;
    public Map map;

    public ParticleRendering particleRendering;

    public Unity.Mathematics.Random random;
    public int tick;

    public void Execute(int i)
    {
        int2 pos = new int2(i % map.sizes.x, i / map.sizes.y);
        int index = map.PosToIndex(pos);

        Color32 color = GetColorForType(pos, map[index].type);
        //Color32 color = GetColorForType(pos, map[pos].type);
        colorArray[i] = color;
    }

    Color32 GetColorForType(int2 position, ParticleType type)
    {
        switch (type)
        {
            case ParticleType.None:
                return particleRendering.noneColor;
            case ParticleType.Water:
                return GetWaterColor(position);
            case ParticleType.Sand:
                return GetSandcolor(position);
            case ParticleType.Mud:
                return particleRendering.mudColor;
            case ParticleType.Player:
                //Gets overriden when trying the sprite
                return Color.clear;
            default:
                return Color.black;
        }
    }

    Color32 GetWaterColor(int2 position)
    {
        WaterRendering waterRendering = particleRendering.waterRendering;
        float2 pos = new float2(position.x, position.y) + waterRendering.speed * tick;
        float value = noise.snoise(pos * waterRendering.scaling);
        float valueNormalized = (value + 1) * 0.5f;
        if (valueNormalized > waterRendering.bubbleInnerThreshold)
        {
            return waterRendering.bubbleInnerColor;
        }
        else if (value > waterRendering.bubbleOuterThreshold)
        {
            return waterRendering.bubbleOuterColor;
        }
        else
        {
            return waterRendering.waterColor;
        }
    }

    Color32 GetSandcolor(int2 position)
    {
        SandRendering sandRendering = particleRendering.sandRendering;

        //add sin ripple
        bool shimmer = random.NextFloat() > sandRendering.shimmerThreshold;
        if (shimmer)
        {
            return sandRendering.shimmerColor;
        }
        else
        {
            float2 noiseOffset = noise.snoise(tick * sandRendering.waveSpeed);
            float2 scrollOffset = tick * sandRendering.waveScrollSpeed;
            float2 offset = noiseOffset + scrollOffset;

            float xOffset = math.sin(position.x * sandRendering.waveScale.x + offset.x);
            float ySin = math.sin(position.y * sandRendering.waveScale.y + xOffset + offset.y);
            float sinNormalized = (ySin + 1) * 0.5f;
            if (sinNormalized > particleRendering.sandRendering.waveThreshold)
            {
                return sandRendering.waveColor;
            }
            else
            {
                return sandRendering.sandColor;
            }
        }
    }

}
