using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct IceRendering : IParticleRenderer
{
    public float thresholdShineReflection;
    public float reflectionShineSpeed;
    public Color reflectionShineColor;
    public Color iceColor;
    public float reflectionXDifference;
    public float reflectionShineAngle;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        float noiseSeed = tickBlock.tick * reflectionShineSpeed + position.x * reflectionXDifference + position.y * reflectionShineAngle;
        float noiseValue = noise.snoise(new float2(0, noiseSeed));
        if (noiseValue > thresholdShineReflection)
        {
            return reflectionShineColor;
        }
        return iceColor;
    }
}