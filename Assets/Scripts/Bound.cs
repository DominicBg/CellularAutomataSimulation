using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public struct Bound
{
    public int2 position;
    public int2 sizes;

    public int2 min => position;
    public int2 max => position + (sizes - 1);

    public int2 topLeft => new int2(position.x, position.y + sizes.y);
    public int2 topRight => new int2(position.x + sizes.x, position.y + sizes.y);
    public int2 bottomLeft => position;
    public int2 bottomRight => new int2(position.x + sizes.x, position.y);

    public Bound(int2 position, int2 sizes)
    {
        this.position = position;
        this.sizes = sizes;
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

}
