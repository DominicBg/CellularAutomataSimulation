using Unity.Collections;
using Unity.Mathematics;

public struct RotationBound
{
    public enum Anchor {
        TopLeft, TopCenter, TopRight,
        CenterLeft, Center, CenterRight,
        BottomLeft, BottomCenter, BottomRight    
    }

    public Bound bound;
    public float angle;
    public Anchor anchor;

    public RotationBound(Bound bound, float angle, Anchor anchor = Anchor.Center)
    {
        this.bound = bound;
        this.angle = angle;
        this.anchor = anchor;
    }

    public bool PointInBound(int2 point)
    {
        return bound.PointInBound(InverseTransformPoint(TransformPoint(point)));
    }
    public bool PointInBound(int2 point, float sin, float cos)
    {
        return bound.PointInBound(InverseTransformPoint(TransformPoint(point, sin, cos)));
    }

    public float2 GetUV(int2 point)
    {
        int2 localPoint = TransformPoint(point);
        return bound.GetUV(InverseTransformPoint(localPoint));
    }

    private int2 TransformPoint(int2 point)
    {
        int2 anchorPos = AnchorPosition();
        int2 diffMid = anchorPos - bound.center;
        return (int2)MathUtils.Rotate(anchorPos - diffMid - point, -math.radians(angle));
    }

    private int2 TransformPoint(int2 point, float sin, float cos)
    {
        int2 anchorPos = AnchorPosition();
        int2 diffMid = anchorPos - bound.center;
        return (int2)MathUtils.Rotate(anchorPos - diffMid - point, sin, cos);
    }

    private int2 InverseTransformPoint(int2 localPoint)
    {
        return AnchorPosition() + localPoint;
    }

    public bool IntersectWith(Bound otherBound)
    {
        NativeList<float2> otherPos = otherBound.GetCornersFloat2();
        NativeList<float2> pos = GetCornersFloat2();
        bool hasCollision = PhysiXVII.HasPolygonCollision(otherPos, pos);
        otherPos.Dispose();
        pos.Dispose();
        return hasCollision;
    }
    public bool IntersectWith(RotationBound otherBound)
    {
        NativeList<float2> otherPos = otherBound.GetCornersFloat2();
        NativeList<float2> pos = GetCornersFloat2();
        bool hasCollision = PhysiXVII.HasPolygonCollision(otherPos, pos);
        otherPos.Dispose();
        pos.Dispose();
        return hasCollision;
    }

    public NativeList<int2> GetCorners(Allocator allocator = Allocator.Temp)
    {
        NativeList<int2> positions = new NativeList<int2>(allocator);
        positions.Add(CornerPos(bound.topLeft));
        positions.Add(CornerPos(bound.topRight));
        positions.Add(CornerPos(bound.bottomLeft));
        positions.Add(CornerPos(bound.bottomRight));
        return positions;
    }
    public NativeList<float2> GetCornersFloat2(Allocator allocator = Allocator.Temp)
    {
        NativeList<float2> positions = new NativeList<float2>(allocator);
        positions.Add(CornerPos(bound.topLeft));
        positions.Add(CornerPos(bound.topRight));
        positions.Add(CornerPos(bound.bottomLeft));
        positions.Add(CornerPos(bound.bottomRight));
        return positions;
    }

    private int2 CornerPos(int2 position)
    {
        return (int2)MathUtils.Rotate(AnchorPosition() - position, math.radians(angle)) + bound.center;
    }

    public void GetSinCosAngle(out float sin, out float cos)
    {
        math.sincos(-math.radians(angle), out sin, out cos);
    }

    int2 AnchorPosition()
    {
        switch (anchor)
        {
            case Anchor.TopLeft:
                return bound.topLeft;
            case Anchor.TopCenter:
                return bound.topCenter;
            case Anchor.TopRight:
                return bound.topRight;
            case Anchor.CenterLeft:
                return bound.centerLeft;
            case Anchor.Center:
                return bound.center;
            case Anchor.CenterRight:
                return bound.centerRight;
            case Anchor.BottomLeft:
                return bound.bottomLeft;
            case Anchor.BottomCenter:
                return bound.bottomCenter;
            case Anchor.BottomRight:
                return bound.bottomRight;
        }
        return bound.bottomCenter;
    }
}
