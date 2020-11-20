using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// From [-1, 1] to [0, 1]
    /// </summary>
    public static float unorm(float x) => x * 0.5f + 0.5f;

    /// <summary>
    /// From [0, 1] to [-1, 1]
    /// </summary>
    public static float snorm(float x) => x * 2 - 1;

    public static int2 quantize(int2 v, int2 cellSize)
    {
        return new int2(math.floor(v / (float2)cellSize));
    }
}
