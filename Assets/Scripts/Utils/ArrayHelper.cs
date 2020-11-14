using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class ArrayHelper
{
    public static int PosToIndex(int2 pos, int2 sizes)
    {
        return pos.y * sizes.x + pos.x;
    }
    public static int PosToIndex(int x, int y, int2 sizes)
    {
        return y * sizes.x + x;
    }
    public static bool InBound(int x, int y, int2 sizes)
    {
        return InBound(new int2(x,y), sizes);
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

    public static T[,] GetGridFromArray<T>(T[] array, int2 sizes)
    {
        T[,] grid = new T[sizes.x, sizes.y];
        for (int i = 0; i < grid.Length; i++)
        {
            int x = i % sizes.x;
            int y = i / sizes.x;
            grid[x, y] = array[i];
        }
        return grid;
    }

    public static T[] GetArrayFromGrid<T>(T[,] grid, int2 sizes)
    {
        T[] array = new T[sizes.x * sizes.y];
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int i = PosToIndex(x, y, sizes);
                array[i] = grid[x, y];
            }
        }
        return array;
    }
}
