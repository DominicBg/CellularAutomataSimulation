using Unity.Mathematics;

public struct Bound
{
    public int2 topLeft;
    public int2 topRight;
    public int2 bottomLeft;
    public int2 bottomRight;

    public Bound(int2 position, int2 sizes)
    {
        topLeft = new int2(position.x, position.y + sizes.y);
        topRight = new int2(position.x + sizes.x, position.y + sizes.y);
        bottomLeft = position;
        bottomRight = new int2(position.x + sizes.x, position.y);
    }
}
