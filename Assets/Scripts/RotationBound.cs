using Unity.Mathematics;

public struct RotationBound
{
    public Bound bound;
    public float angle;

    public RotationBound(Bound bound, float angle)
    {
        this.bound = bound;
        this.angle = angle;
    }

    public bool PointInBound(int2 pos)
    {
        int2 localPos = (int2)MathUtils.Rotate(bound.center - pos, -math.radians(angle));
        return bound.PointInBound(bound.center + localPos);
    }
}
