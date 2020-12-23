using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct CinderRendering : IParticleRenderer
{
    public float speed;
    public float spread;
    public Color32 color0;
    public Color32 color1;
    public Color32 color2;
    public Color32 color3;
    public float threshold1;
    public float threshold2;
    public float threshold3;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        float noiseValue = MathUtils.unorm(noise.cnoise((float2)position * spread + tickBlock.tick * speed));
        //sexylolol
        if(noiseValue > threshold1)
        {
            if (noiseValue > threshold2)
            {
                if (noiseValue > threshold3)
                {
                    return color3;
                }
                else
                {
                    return color2;
                }
            }
            else
            {
                return color1;
            }
        }
        else
        {
            return color0;
        }
    }
}
