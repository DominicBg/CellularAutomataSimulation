using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct WaterRendering: IParticleRenderer
{
    [Header("Bubble Sin")]
    public float bubbleSineAmplitude;
    public float bubbleSineSpeed;
    public float bubbleSineNoiseAmplitude;
    public float2 bubbleSineOffSynch;
    public float2 bubbleSineNoiseSpeed;

    [Header("Bubble Color")]
    public Color bubbleOuterColor;
    public Color bubbleInnerColor;
    public float bubbleInnerThreshold;
    public float bubbleOuterThreshold;

    [Header("Other")]
    public float scaling;
    public Color waterColor;
    public float2 speed;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        float2 sineNoiseValue = tickBlock.tick * bubbleSineNoiseSpeed;
        float sinNoise = math.remap(0, 1, bubbleSineNoiseAmplitude, 1, noise.snoise(sineNoiseValue));

        float2 offSynch = position * bubbleSineOffSynch;
        float sin = math.sin(bubbleSineSpeed * tickBlock.tick + offSynch.x + offSynch.y);
        float posX = position.x + (sinNoise * bubbleSineAmplitude * sin);
        float2 pos = new float2(posX, position.y) + speed * tickBlock.tick;
        float value = noise.snoise(pos * scaling);
        float valueNormalized = (value + 1) * 0.5f;

        if (valueNormalized > bubbleInnerThreshold)
        {
            return bubbleInnerColor;
        }
        else if (value > bubbleOuterThreshold)
        {
            return bubbleOuterColor;
        }
        else
        {
            return waterColor;
        }
    }
}