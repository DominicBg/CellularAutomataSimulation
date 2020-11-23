using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct RubbleRendering: IParticleRenderer
{
    public Color rockColor;
    public Color crackColor;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        uint seed = (uint)(position.x + position.y * 100);
        if (Unity.Mathematics.Random.CreateFromIndex(seed).NextBool())
        {
            return crackColor;
        }
        else
        {
            return rockColor;
        }
    }
}