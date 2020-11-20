using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct FireRendering : IParticleRenderer
{
    public Color32 flameColor1;
    public float2 flameSpeed1;
    public float2 flameScale1;
    public float2 minMaxAlpha1;

    public Color32 flameColor2;
    public float2 flameSpeed2;
    public float2 flameScale2;
    public float2 minMaxAlpha2;

    public Color32 flameColor3;
    public float2 flameSpeed3;
    public float2 flameScale3;
    public float2 minMaxAlpha3;

    public BlendingMode blending;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        Color32 color = Color.clear;
        color = GetColor(color, position, ref tickBlock, flameColor1, flameSpeed1, flameScale1, minMaxAlpha1);
        color = GetColor(color, position, ref tickBlock, flameColor2, flameSpeed2, flameScale2, minMaxAlpha2);
        color = GetColor(color, position, ref tickBlock, flameColor3, flameSpeed3, flameScale3, minMaxAlpha3);
        return color;
    }

    private Color32 GetColor(Color currentColor, int2 position, ref TickBlock tickBlock, Color flameColor, float2 flameSpeed, float2 flameScale, float2 minMaxAlpha)
    {
        float noiseValue = noise.cnoise(position * flameScale + tickBlock.tick * flameSpeed);
        float a = math.remap(-1, 1, minMaxAlpha.x, minMaxAlpha.y, noiseValue);
        flameColor.a = a;
        return RenderingUtils.Blend(currentColor, flameColor, blending);
    }
}
