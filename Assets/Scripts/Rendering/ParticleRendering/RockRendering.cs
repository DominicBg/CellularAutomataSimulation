using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct RockRendering: IParticleRenderer
{
    public Color rockColor;
    public Color crackColor;
    public float noiseCrackThreshold;
    public float noiseScale;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        float2 positionScaled = new float2(position.x * noiseScale, position.y * noiseScale);
        float noiseValue = noise.cellular(positionScaled).x;
        float noiseValueNormalized = (noiseValue + 1) * 0.5f;
        if (noiseValueNormalized > noiseCrackThreshold)
        {
            return crackColor;
        }
        else
        {
            return rockColor;
        }
    }
}