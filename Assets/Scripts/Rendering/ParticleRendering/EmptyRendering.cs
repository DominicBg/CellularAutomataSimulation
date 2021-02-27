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
        hasSurroundingParticle |= HasSurroundingParticle(up, ref map);
        hasSurroundingParticle |= HasSurroundingParticle(right, ref map);
        hasSurroundingParticle |= HasSurroundingParticle(down, ref map);
        hasSurroundingParticle |= HasSurroundingParticle(left, ref map);

        if (hasSurroundingParticle)
            return contrastColor;

        return emptyColor;
    }

    bool HasSurroundingParticle(int2 position, ref Map map)
    {
        ParticleType type = map.GetParticleType(position);
        return type != ParticleType.None && type != ParticleType.Collision && type != ParticleType.Player;
    }
}
