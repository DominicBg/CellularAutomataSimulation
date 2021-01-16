using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class ParticleRenderUtil
{
    public static Color32 GetColorForType(int2 position, ParticleType type, ref ParticleRendering particleRendering, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        switch (type)
        {
            case ParticleType.None:
                return Color.black;
            case ParticleType.Water:
                return particleRendering.waterRendering.GetColor(position, ref tickBlock);
            case ParticleType.Sand:
                return particleRendering.sandRendering.GetColor(position, ref tickBlock);
            case ParticleType.Mud:
                return particleRendering.mudColor;

            case ParticleType.Snow:
                return particleRendering.snowColor;
            case ParticleType.Ice:
                return particleRendering.iceRendering.GetColor(position, ref tickBlock);
            case ParticleType.Rock:
                return particleRendering.rockRendering.GetColor(position, ref tickBlock, ref map, lightSources);
            case ParticleType.Rubble:
                return particleRendering.rubbleColor.GetColor(position, ref tickBlock);
            case ParticleType.TitleDisintegration:
                return particleRendering.titleDisintegration;
            case ParticleType.Fire:
                return particleRendering.fireRendering.GetColor(position, ref tickBlock);
            case ParticleType.Player:
                //Gets overriden when trying the sprite
                return Color.clear;
            case ParticleType.Cinder:
                return particleRendering.cinderRendering.GetColor(position, ref tickBlock);
            case ParticleType.Wood:
                return particleRendering.woodRendering.GetColor(position, ref tickBlock);
            case ParticleType.String:
                return particleRendering.stringRendering.GetColor(position, ref tickBlock);

            default:
                return Color.clear;
        }
    }
}
