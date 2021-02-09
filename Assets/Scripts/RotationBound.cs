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

    public bool PointInBound(int2 pos)
    {
        int2 anchorPos = AnchorPosition();
        int2 diffMid = anchorPos - bound.center;
        int2 localPos = (int2)MathUtils.Rotate(anchorPos - diffMid - pos, -math.radians(angle));
        return bound.PointInBound(anchorPos + localPos);
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
