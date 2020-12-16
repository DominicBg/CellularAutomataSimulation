﻿using Unity.Burst;
using Unity.Mathematics;

public static class RayMarchingPrimitive
{
    [BurstCompile]
    public static float sdSphere(float3 pos, float scale)
    {
        return math.length(pos) - scale;
    }

    [BurstCompile]
    public static float sdBox(float3 pos, float3 boxScale)
    {
        float3 q = math.abs(pos) - boxScale;
        return math.length(math.max(q, 0)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0);
    }

    [BurstCompile]
    public static float sdOctahedron(float3 pos, float scale)
    {
        pos = math.abs(pos);
        float m = pos.x + pos.y + pos.z - scale;
        float3 q;
        if (3.0 * pos.x < m) q = pos.xyz;
        else if (3.0 * pos.y < m) q = pos.yzx;
        else if (3.0 * pos.z < m) q = pos.zxy;
        else return m * 0.57735027f;

        float k = math.clamp(0.5f * (q.z - q.y + scale), 0, scale);
        return math.length(new float3(q.x, q.y - scale + k, q.z - k));
    }

    [BurstCompile]
    public static float sdCone(float3 p, float2 c, float h)
    {
        // c is the sin/cos of the angle, h is height
        // Alternatively pass q instead of (c,h),
        // which is the point at the base in 2D
        float2 q = h * new float2(c.x / c.y, -1);

        float2 w = new float2(math.length(p.xz), p.y);
        float2 a = w - q * math.clamp(math.dot(w, q) / math.dot(q, q), 0, 1);
        float2 b = w - q * new float2(math.clamp(w.x / q.x, 0, 1), 1);
        float k = math.sign(q.y);
        float d = math.min(math.dot(a, a), math.dot(b, b));
        float s = math.max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
        return math.sqrt(d) * math.sign(s);
    }
    [BurstCompile]
    public static float sdPyramid(float3 p, float h)
    {
        float m2 = h * h + 0.25f;

        p.xz = math.abs(p.xz);
        p.xz = (p.z > p.x) ? p.zx : p.xz;
        p.xz -= 0.5f;

        float3 q = new float3(p.z, h * p.y - 0.5f * p.x, h * p.x + 0.5f * p.y);

        float s = math.max(-q.x, 0.0f);
        float t = math.clamp((q.y - 0.5f * p.z) / (m2 + 0.25f), 0.0f, 1.0f);

        float a = m2 * (q.x + s) * (q.x + s) + q.y * q.y;
        float b = m2 * (q.x + 0.5f * t) * (q.x + 0.5f * t) + (q.y - m2 * t) * (q.y - m2 * t);

        float d2 = math.min(q.y, -q.x * m2 - q.y * 0.5f) > 0.0f ? 0.0f : math.min(a, b);

        return math.sqrt((d2 + q.z * q.z) / m2) * math.sign(math.max(q.z, -p.y));
    }


    //Helper

    static float dot2(float2 v) { return math.dot(v, v); }
    static float dot2(float3 v) { return math.dot(v, v); }
    static float ndot(float3 a, float2 b) { return a.x * b.x - a.y * b.y; }

    [BurstCompile]
    public static float3 opTwist(float3 pos, float twist = 10)
    {
        math.sincos(twist * pos.y, out float s, out float c);
        float2x2 m = new float2x2(c, -s, s, c);
        float3 q = new float3(math.mul(m, pos.xz), pos.y);
        return q;
    }

    [BurstCompile]
    public static float3 opRepLim(float3 p, float count, float3 length)
    {
        float3 q = p - count * math.clamp(math.round(p / count), -length, length);
        return q;
    }

    [BurstCompile]
    public static float3 opRep(float3 p, float3 c)
    {
        float3 q = math.fmod(p + 0.5f * c, c) - 0.5f * c;
        return q;
    }

    [BurstCompile]
    public static float3 Transform(float3 p, float3x3 t)
    {
        return math.mul(math.inverse(t), p);
    }
    [BurstCompile]
    public static float3 Transform(float3 p, quaternion t)
    {
        return math.mul(math.inverse(t), p);
    }

    [BurstCompile]
    public static float3 RotateX(float3 p, float a)
    {
        return Transform(p, rotationX(a));
    }

    [BurstCompile]
    public static float3 RotateY(float3 p, float a)
    {
        return Transform(p, rotationY(a));
    }


    [BurstCompile]
    public static float3 RotateZ(float3 p, float a)
    {
        return Transform(p, rotationZ(a));
    }

    [BurstCompile]
    public static float3 RotateAroundAxis(float3 p, float3 axis, float angle)
    {
        quaternion q = quaternion.AxisAngle(math.normalize(axis), angle);
        return math.mul(math.inverse(q), p);

    }


    [BurstCompile]
    public static float3x3 rotationX(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            1, 0, 0,
            0, c, s,
            0, -s, c
        );
    }

    [BurstCompile]
    public static float3x3 rotationY(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            c, 0, -s,
            0, 1, 0,
            s, 0, c
        );
    }

    [BurstCompile]
    public static float3x3 rotationZ(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            c, -s, 0,
            s, c, 0,
            0, 0, 1
        );
    }

    [BurstCompile]
    public static float3 Translate(float3 p, float3 tr)
    {
        float4x4 trm = new float4x4(
            1, 0, 0, tr.x,
            0, 1, 0, tr.y,
            0, 0, 1, tr.z,
            0, 0, 0, 1
        );
        return (math.mul(math.inverse(trm), new float4(p.x, p.y, p.z, 1))).xyz;
    }

    [BurstCompile]
    public static float expsmin(float a, float b, float k)
    {
        float res = math.exp2(-k * a) + math.exp2(-k * b);
        return -math.log2(res) / k;
    }

    [BurstCompile]
    public static float polysmin(float a, float b, float k)
    {
        float h = math.clamp(0.5f + 0.5f * (b - a) / k, 0, 1);
        return math.lerp(b, a, h) - k * h * (1 - h);
    }

    [BurstCompile]
    public static float powersmin(float a, float b, float k)
    {
        a = math.pow(a, k); b = math.pow(b, k);
        return math.pow((a * b) / (a + b), 1 / k);
    }

}
