using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class GridHelper
{
    public static bool InBound(int x, int y, int2 sizes)
    {
        return InBound(new int2(x, y), sizes);
    }

    public static bool InBound(int2 pos, int2 sizes)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < sizes.x && pos.y < sizes.y;
    }

    public static bool InBound(Bound bound, int2 sizes)
    {
        return
            InBound(bound.topLeft, sizes) &&
            InBound(bound.topRight, sizes) &&
            InBound(bound.bottomLeft, sizes) &&
            InBound(bound.bottomRight, sizes);
    }

    public static NativeArray<int2> GetCircle(int radius, Allocator allocator)
    {
        NativeList<int2> positionList = new NativeList<int2>(allocator);
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int2 position = new int2(x, y);
                if (math.length(position) < radius)
                {
                    positionList.Add(position);
                }
            }
        }
        return positionList.AsArray();
    }

    public static NativeArray<int2> GetCircleAtPosition(int2 centerPosition, int radius, int2 mapSizes, Allocator allocator)
    {
        NativeList<int2> positionList = new NativeList<int2>(2 * radius * radius, Allocator.Temp);
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int2 position = centerPosition + new int2(x, y);
                if (InBound(position, mapSizes) && math.length(centerPosition - position) < radius)
                {
                    positionList.Add(position);
                }
            }
        }

        NativeArray<int2> positions = new NativeArray<int2>(positionList.Length, allocator);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = positionList[i];
        }
        positionList.Dispose();
        return positions;
    }
}
