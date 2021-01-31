using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public struct Bound
{
    public int2 position;
    public int2 sizes;

    public int2 min => position;
    public int2 max => position + (sizes - 1);

    public int2 topLeft => new int2(position.x, position.y + sizes.y - 1);
    public int2 topRight => new int2(position.x + sizes.x, position.y + sizes.y - 1);
    public int2 bottomLeft => position;
    public int2 bottomRight => new int2(position.x + sizes.x - 1, position.y);
    public int2 center => new int2(position.x + sizes.x / 2, position.y + sizes.y / 2);


    public Bound(int2 bottomLeft, int2 sizes)
    {
        this.position = bottomLeft;
        this.sizes = sizes;
    }
    public static Bound CenterAligned(int2 center, int2 sizes)
    {
        return new Bound(center - sizes / 2, sizes);
    }


    public NativeArray<int2> GetPositionsGrid(Allocator allocator = Allocator.Temp)
    {
        int size = sizes.x * sizes.y;
        NativeArray<int2> positions = new NativeArray<int2>(size, allocator);
        int i = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                positions[i++] = new int2(x, y);
            }
        }
        return positions;
    }
    public void GetPositionsGrid(out NativeArray<int2> positions, Allocator allocator = Allocator.Temp)
    {
        int size = sizes.x * sizes.y;
        positions = new NativeArray<int2>(size, allocator);
        int i = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                positions[i++] = new int2(x, y);
            }
        }
    }
    public void GetPositionsIndexArrayXY(out NativeArray<int> positions, int2 mapSizes, Allocator allocator = Allocator.Temp)
    {
        int size = sizes.x * sizes.y;
        positions = new NativeArray<int>(size, allocator);
        int i = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                positions[i++] = ArrayHelper.PosToIndex(new int2(x, y), mapSizes);
            }
        }
    }

    public void GetPositionsIndexArrayYX(out NativeArray<int> positions, int2 mapSizes, Allocator allocator = Allocator.TempJob)
    {
        int size = sizes.x * sizes.y;
        positions = new NativeArray<int>(size, allocator);
        int i = 0;
        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                positions[i++] = ArrayHelper.PosToIndex(new int2(x, y), mapSizes);
            }
        }
    }

    public bool IntersectWith(Bound otherBound)
    {
        return !(otherBound.max.x < min.x || otherBound.min.x > max.x || otherBound.max.y < min.y || otherBound.min.y > max.y);
    }
    public bool PointInBound(int2 point)
    {
        return !(point.x < min.x || point.x > max.x || point.y < min.y || point.y > max.y);
    }
    public int2 ProjectPointOnbound(int2 point)
    {
        return math.clamp(point, min, max);
    }

    public int2 RandomPointInBound(ref TickBlock tickblock)
    {
        return tickblock.random.NextInt2(min, max + 1);
    }
    public int2 RandomPointInBound(ref Random random)
    {
        return random.NextInt2(min, max + 1);
    }
}
