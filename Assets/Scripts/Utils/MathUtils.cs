﻿using Unity.Mathematics;

public static class MathUtils
{
    /// <summary>
    /// From [-1, 1] to [0, 1]
    /// </summary>
    public static float unorm(float x) => x * 0.5f + 0.5f;
    public static float2 unorm(float2 x) => x * 0.5f + 0.5f;

    /// <summary>
    /// From [0, 1] to [-1, 1]
    /// </summary>
    public static float snorm(float x) => x * 2 - 1;
    public static float2 snorm(float2 x) => x * 2 - 1;

    public static int2 quantize(int2 v, int2 cellSize)
    {
        return new int2(math.floor(v / (float2)cellSize));
    }

    public static Random CreateRandomAtPosition(int2 position, uint seed = 0)
    {
        uint randomCellSeed = (uint)(position.x + position.y * 100) + seed;
        return Random.CreateFromIndex(randomCellSeed);
    }

    public static void CartesianToPolar(float2 pos, out float r, out float a)
    {
        r = math.length(pos);
        a = math.atan2(pos.y, pos.x);
    }
    public static float2 PolarToCartesian(float r, float a)
    {
        math.sincos(a, out float sin, out float cos);
        return new float2(r * cos, r * sin);
    }

    public static float2 Rotate(float2 v, float a)
    {
        math.sincos(a, out float s, out float c);
        return new float2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    public static float2 Spherize(int2 center, int2 position, float radius)
    {
        float2 uv = (float2)(position - center) / radius;
        return center + Spherize(uv) * radius;
    }

    public static float2 Spherize(float2 center, float2 position, float radius)
    {
        float2 uv = (position - center) / radius;
        return center + Spherize(uv) * radius;
    }

    public static float2 Spherize(float2 uv)
    {
        float r2 = math.length(uv);
        if(r2 > 1)
        {
            return uv;
        }

        float theta = math.atan2(uv.y, uv.x);
        float r1 = math.asin(r2) / (math.PI / 2);
        return new float2(r1 * math.cos(theta), r1 * math.sin(theta));
    }

    public static float3 NormalStrength(float3 normal, float strength)
    {
        return new float3(normal.x * strength, normal.y * strength, math.lerp(1, normal.z, math.saturate(strength)));
    }

    public static float ReduceResolution(float x, float resolution)
    {
        return math.floor(x * resolution) / resolution;
    }

    public static float RemapSaturate(float from1, float to1, float from2, float to2, float x)
    {
        return math.lerp(from2, to2, math.saturate(math.unlerp(from1, to1, x)));
    }
}

