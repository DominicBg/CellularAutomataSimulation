using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class NoiseXVII
{
    // See http://www.iquilezles.org/www/articles/warp/warp.htm for details
    static float2x2 mtx => new float2x2(0.80f, 0.60f, -0.60f, 0.80f);

    public static float fbm4r(float2 p)
    {
        float f = 0f;

        float2x2 mtx = NoiseXVII.mtx;
        f += 0.5000f * noise(p); p = math.mul(mtx, p) * 2.02f;
        f += 0.2500f * noise(p); p = math.mul(mtx, p) * 2.03f;
        f += 0.1250f * noise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.0625f * noise(p);

        return f / 0.9375f;
    }

    public static float fbm4(float2 p)
    {
        float f = 0f;

        f += 0.5000f * noise(p);
        f += 0.2500f * noise(p);
        f += 0.1250f * noise(p);
        f += 0.0625f * noise(p);

        return f / 0.9375f;
    }


    public static float fbm6(float2 p)
    {
        float f = 0f;

        float2x2 mtx = NoiseXVII.mtx;
        f += 0.500000f * noise(p); p = math.mul(mtx, p) * 2.02f;
        f += 0.250000f * noise(p); p = math.mul(mtx, p) * 2.03f;
        f += 0.125000f * noise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.062500f * noise(p); p = math.mul(mtx, p) * 2.04f;
        f += 0.031250f * noise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.015625f * noise(p);

        return f / 0.96875f;
    }

    public static float noise(float2 p)
    {
        return Unity.Mathematics.noise.cnoise(p);
    }

    public static float2 fbm4r_2(float2 p)
    {
        return new float2(fbm4r(p + 1.0f), fbm4r(p + 6.2f));
    }
    public static float2 fbm4_2(float2 p)
    {
        return new float2(fbm4(p + 1.0f), fbm4(p + 6.2f));
    }

    public static float2 fbm6_2(float2 p)
    {
        return new float2(fbm6(p +  9.2f), fbm6(p + 5.7f));
    }

}
