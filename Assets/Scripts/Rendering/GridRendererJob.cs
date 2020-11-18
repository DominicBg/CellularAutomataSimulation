﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GridRendererJob : IJobParallelFor
{
    public NativeArray<Color32> colorArray;
    public Map map;

    public ParticleRendering particleRendering;

    //use tickblock
    public Unity.Mathematics.Random random;
    public int tick;

    public void Execute(int i)
    {
        int2 pos = new int2(i % map.Sizes.x, i / map.Sizes.y);
        Color32 color = GetColorForType(pos, map.GetParticleType(pos), i);
        colorArray[i] = color;
    }

    Color32 GetColorForType(int2 position, ParticleType type, int index)
    {
        switch (type)
        {
            case ParticleType.None:
                return colorArray[index];
            case ParticleType.Water:
                return GetWaterColor(position);
            case ParticleType.Sand:
                return GetSandColor(position);
            case ParticleType.Mud:
                return particleRendering.mudColor;
            case ParticleType.Player:
                //Gets overriden when trying the sprite
                return Color.clear;
            case ParticleType.Snow:
                return particleRendering.snowColor;
            case ParticleType.Ice:
                return GetIceColor(position);
            case ParticleType.Rock:
                return GetRockColor(position);
            case ParticleType.TitleDisintegration:
                return particleRendering.titleDisintegration;
            default:
                return Color.black;
        }
    }

    Color32 GetWaterColor(int2 position)
    {
        WaterRendering waterRendering = particleRendering.waterRendering;
        float2 sineNoiseValue = tick * waterRendering.bubbleSineNoiseSpeed;
        float sinNoise = math.remap(0, 1, waterRendering.bubbleSineNoiseAmplitude, 1, noise.snoise(sineNoiseValue));

        float2 offSynch = position * waterRendering.bubbleSineOffSynch;
        float sin = math.sin(waterRendering.bubbleSineSpeed * tick + offSynch.x + offSynch.y);
        float posX = position.x + (sinNoise * waterRendering.bubbleSineAmplitude * sin);
        float2 pos = new float2(posX, position.y) + waterRendering.speed * tick;
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

    Color32 GetSandColor(int2 position)
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

    Color32 GetIceColor(int2 position)
    {
        IceRendering iceRendering = particleRendering.iceRendering;

        float noiseSeed = tick * iceRendering.reflectionShineSpeed + position.x * iceRendering.reflectionXDifference + position.y * iceRendering.reflectionShineAngle;
        float noiseValue = noise.snoise(new float2(0, noiseSeed));
        if(noiseValue > iceRendering.thresholdShineReflection)
        {
            return iceRendering.reflectionShineColor;
        }
        return iceRendering.iceColor;
    }

    Color32 GetRockColor(int2 position)
    {
        RockRendering rockRendering = particleRendering.rockRendering;
        float2 positionScaled = new float2(position.x * rockRendering.noiseScale, position.y * rockRendering.noiseScale);
        float noiseValue = noise.cellular(positionScaled).x;
        float noiseValueNormalized = (noiseValue + 1) * 0.5f;
        if(noiseValueNormalized > rockRendering.noiseCrackThreshold)
        {
            return rockRendering.crackColor;
        }
        else
        {
            return rockRendering.rockColor;
        }
    }
}
