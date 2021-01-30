using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.noise;
public static class NoiseXVII
{
    public enum NoiseType { Worley, CNoise, SNoise, CNoiseFmb4, CNoiseFbm4r }

    [System.Serializable]
    public struct Noise
    {
        public float amplitude;
        public float scale;
        public float offset;
        public NoiseType type;
        public bool isInverted;

        public float CalculateValue(float2 pos)
        {
            float noiseValue = CalculateNoise(pos * scale + offset);
            //normalize [-1, 1] to [0, 1]
            noiseValue = noiseValue * 0.5f + 0.5f;
            if (isInverted)
                noiseValue = 1 - noiseValue;
            return amplitude * math.saturate(noiseValue);
        }

        float CalculateNoise(float2 pos)
        {
            switch (type)
            {
                case NoiseType.Worley:
                    return cellular(pos).x;
                case NoiseType.CNoise:
                    return cnoise(pos);
                case NoiseType.CNoiseFmb4:
                    return fbm4(pos);
                case NoiseType.CNoiseFbm4r:
                    return fbm4r(pos);
                case NoiseType.SNoise:
                    return snoise(pos);
            }
            return 0;
        }
    }


    // See http://www.iquilezles.org/www/articles/warp/warp.htm for details
    static float2x2 mtx => new float2x2(0.80f, 0.60f, -0.60f, 0.80f);

    public static float fbm4r(float2 p)
    {
        float f = 0f;

        float2x2 mtx = NoiseXVII.mtx;
        f += 0.5000f * cnoise(p); p = math.mul(mtx, p) * 2.02f;
        f += 0.2500f * cnoise(p); p = math.mul(mtx, p) * 2.03f;
        f += 0.1250f * cnoise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.0625f * cnoise(p);

        return f / 0.9375f;
    }

    public static float fbm4r_3x(float2 p, float2 offset)
    {
       return fbm4r(p + fbm4r(offset + p + fbm4r(p)));
    }

    public static float fbm4(float2 p)
    {
        float f = 0f;

        f += 0.5000f * cnoise(p);
        f += 0.2500f * cnoise(p);
        f += 0.1250f * cnoise(p);
        f += 0.0625f * cnoise(p);

        return f / 0.9375f;
    }

    public static float fbm4(float3 p)
    {
        float f = 0f;

        f += 0.5000f * cnoise(p);
        f += 0.2500f * cnoise(p);
        f += 0.1250f * cnoise(p);
        f += 0.0625f * cnoise(p);

        return f / 0.9375f;
    }
    public static float2 fbm4_worly(float3 p)
    {
        float2 f = 0f;

        f += 0.5000f * cellular(p);
        f += 0.2500f * cellular(p);
        f += 0.1250f * cellular(p);
        f += 0.0625f * cellular(p);

        return f / 0.9375f;
    }

    public static float fbm4_3x(float2 p, float2 offset)
    {
        return fbm4r(p + fbm4(offset + p + fbm4r(p)));
    }

    public static float fbm6r(float2 p)
    {
        float f = 0f;

        float2x2 mtx = NoiseXVII.mtx;
        f += 0.500000f * cnoise(p); p = math.mul(mtx, p) * 2.02f;
        f += 0.250000f * cnoise(p); p = math.mul(mtx, p) * 2.03f;
        f += 0.125000f * cnoise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.062500f * cnoise(p); p = math.mul(mtx, p) * 2.04f;
        f += 0.031250f * cnoise(p); p = math.mul(mtx, p) * 2.01f;
        f += 0.015625f * cnoise(p);

        return f / 0.96875f;
    }

    public static float fbm6(float2 p)
    {
        float f = 0f;

        f += 0.500000f * cnoise(p);
        f += 0.250000f * cnoise(p);
        f += 0.125000f * cnoise(p);
        f += 0.062500f * cnoise(p);
        f += 0.031250f * cnoise(p);
        f += 0.015625f * cnoise(p);

        return f / 0.96875f;
    }
    public static float fbm6(float3 p)
    {
        float f = 0f;

        f += 0.500000f * cnoise(p);
        f += 0.250000f * cnoise(p);
        f += 0.125000f * cnoise(p);
        f += 0.062500f * cnoise(p);
        f += 0.031250f * cnoise(p);
        f += 0.015625f * cnoise(p);

        return f / 0.96875f;
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
