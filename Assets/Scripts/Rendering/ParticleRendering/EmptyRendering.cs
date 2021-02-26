using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct EmptyRendering : IParticleRenderer
{
    public Color32 emptyColor;
    public Color32 contrastColor;
    public BlendingMode blending;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        int2 up = new int2(0, 1);
        int2 right = new int2(1, 0);
        int2 down = -up;
        int2 left = -right;

        bool hasSurroundingParticle = false;
        hasSurroundingParticle |= map.GetParticleType(position + up) != ParticleType.None;
        hasSurroundingParticle |= map.GetParticleType(position + right) != ParticleType.None;
        hasSurroundingParticle |= map.GetParticleType(position + down) != ParticleType.None;
        hasSurroundingParticle |= map.GetParticleType(position + left) != ParticleType.None;

        if (hasSurroundingParticle)
            return contrastColor;

        return emptyColor;
    }
}
