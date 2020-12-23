using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct WoodRendering : IParticleRenderer
{
    public Color32 color;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        return color;
    }
}
