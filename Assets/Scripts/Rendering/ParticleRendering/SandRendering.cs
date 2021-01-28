using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SandRendering: IParticleRenderer
{
    public float shimmerThreshold;
    public float waveThreshold;
    public float2 waveScale;
    public float2 waveSpeed;
    public float2 waveScrollSpeed;
    public Color sandColor;
    public Color shimmerColor;
    public Color waveColor;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        //add sin ripple
        bool shimmer = tickBlock.random.NextFloat() > shimmerThreshold;
        if (shimmer)
        {
            return shimmerColor;
        }
        else
        {
            float2 noiseOffset = noise.snoise(tickBlock.tick * waveSpeed);
            float2 scrollOffset = tickBlock.tick * waveScrollSpeed;
            float2 offset = noiseOffset + scrollOffset;

            float xOffset = math.sin(position.x * waveScale.x + offset.x);
            float ySin = math.sin(position.y * waveScale.y + xOffset + offset.y);
            float sinNormalized = (ySin + 1) * 0.5f;
            if (sinNormalized > waveThreshold)
            {
                return waveColor;
            }
            else
            {
                return sandColor;
            }
        }
    }

}