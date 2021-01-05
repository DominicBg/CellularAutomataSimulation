using Unity.Mathematics;

[System.Serializable]
public struct Glowing
{
    public int min;
    public int max;
    public float offsynch;
    public float speed;

    public float EvaluateGlow(int tick)
    {
        return math.lerp(min, max, MathUtils.unorm(math.sin(tick * speed + offsynch)));
    }
}
[System.Serializable]
public struct Glowing2
{
    public int innerRadiusMin;
    public int innerRadiusMax;
    public int outerRadiusMin;
    public int outerRadiusMax;
    public float offsynch;
    public float offsynch2;
    public float speed;

    public float EvaluateInnerGlowRadius(int tick)
    {
        return math.lerp(innerRadiusMin, innerRadiusMax, MathUtils.unorm(math.sin(tick * speed + offsynch)));
    }

    public float EvaluateOuterGlowRadius(int tick)
    {
        return math.lerp(outerRadiusMin, outerRadiusMax, MathUtils.unorm(math.sin(tick * speed + offsynch2)));
    }
}