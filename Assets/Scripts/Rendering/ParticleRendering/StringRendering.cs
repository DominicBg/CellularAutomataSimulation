using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct StringRendering : IParticleRenderer
{
    public Color32 color1;
    public Color32 color2;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        return (position.x + position.y) % 2 == 0 ? color1 : color2;
    }
}
