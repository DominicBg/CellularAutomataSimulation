using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct DirectionLightSettings
{
    public float3 direction;
    public float intensity;
    public Color color;

    public LightSource GetLightSource()
    {
        return LightSource.DirectionalLight(direction, intensity, color);
    }
}

public struct PointLightSettings
{
    public float2 position;
    public float intensity;
    public float radius;
    public Color color;
    public float z;
    public float fadeoff;
    public float resolution;

    public LightSource GetLightSource(float2 position)
    {
        return LightSource.PointLight(new float3(position.x, position.y, z), radius, fadeoff, intensity, color, resolution);
    }
}

