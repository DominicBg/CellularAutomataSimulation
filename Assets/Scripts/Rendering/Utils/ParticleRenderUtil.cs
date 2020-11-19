using Unity.Mathematics;
using UnityEngine;

public static class ParticleRenderUtil
{
    public static Color32 GetColorForType(int2 position, ParticleType type, ref ParticleRendering particleRendering, ref TickBlock tickBlock)
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
            case ParticleType.Player:
                //Gets overriden when trying the sprite
                return Color.clear;
            case ParticleType.Snow:
                return particleRendering.snowColor;
            case ParticleType.Ice:
                return particleRendering.iceRendering.GetColor(position, ref tickBlock);
            case ParticleType.Rock:
                return particleRendering.rockRendering.GetColor(position, ref tickBlock);
            case ParticleType.TitleDisintegration:
                return particleRendering.titleDisintegration;
            default:
                return Color.black;
        }
    }
}
