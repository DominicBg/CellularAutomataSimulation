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
        float noiseValue = MathUtils.unorm(noise.cellular(positionScaled).x);
        return GetColorWithNoiseValue(noiseValue);
    }

    public Color32 GetColorWithNoiseValue(float noiseValue)
    {
        if (noiseValue > noiseCrackThreshold)
        {
            return crackColor;
        }
        else
        {
            return rockColor;
        }
    }
}