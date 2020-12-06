using Unity.Mathematics;

public static class RayMarchingPrimitive
{

    public static float sdSphere(float3 pos, float scale)
    {
        return math.length(pos) - scale;
    }

    public static float sdBox(float3 pos, float3 boxScale)
    {
        float3 q = math.abs(pos) - boxScale;
        return math.length(math.max(q, 0)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0);
    }

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


    //Helper

    static float dot2(float2 v) { return math.dot(v, v); }
    static float dot2(float3 v) { return math.dot(v, v); }
    static float ndot(float3 a, float2 b) { return a.x * b.x - a.y * b.y; }

    public static float3 opTwist(float3 pos, float twist = 10)
    {
        math.sincos(twist * pos.y, out float s, out float c);
        float2x2 m = new float2x2(c, -s, s, c);
        float3 q = new float3(math.mul(m, pos.xz), pos.y);
        return q;
    }


    public static float3 opRepLim(float3 p, float count, float3 length)
    {
        float3 q = p - count * math.clamp(math.round(p / count), -length, length);
        return q;
    }

    public static float3 opRep(float3 p, float3 c)
    {
        float3 q = math.fmod(p + 0.5f * c, c) - 0.5f * c;
        return q;
    }

    public static float3 Transform(float3 p, float3x3 t)
    {
        return math.mul(math.inverse(t), p);
    }

    public static float smin0(float a, float b, float k)
    {
        float h = math.clamp(0.5f + 0.5f * (b - a) / k, 0, 1);
        return math.lerp(b, a, h) - k * h * (1 - h);
    }


    public static float3 RotateX(float3 p, float a)
    {
        return Transform(p, rotationX(a));
    }
    public static float3 RotateY(float3 p, float a)
    {
        return Transform(p, rotationY(a));
    }
    public static float3 RotateZ(float3 p, float a)
    {
        return Transform(p, rotationZ(a));
    }

    public static float3 RotateAroundAxis(float3 p, float3 axis, float angle)
    {
        quaternion q = quaternion.AxisAngle(math.normalize(axis), angle);
        return math.mul(math.inverse(q), p);

    }


    public static float3x3 rotationX(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            1, 0, 0,
            0, c, s,
            0, -s, c
        );
    }

    public static float3x3 rotationY(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            c, 0, -s,
            0, 1, 0,
            s, 0, c
        );
    }

    public static float3x3 rotationZ(float a)
    {
        math.sincos(a, out float s, out float c);
        return new float3x3(
            c, -s, 0,
            s, c, 0,
            0, 0, 1
        );
    }

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
}
