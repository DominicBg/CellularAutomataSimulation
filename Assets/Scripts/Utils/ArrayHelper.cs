using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class ArrayHelper
{

    public static bool TryPosToIndex(int2 pos, int2 sizes, out int index)
    {
        index = pos.y * sizes.x + pos.x;
        return GridHelper.InBound(pos, sizes);
    }
    public static int PosToIndex(int2 pos, int2 sizes)
    {
        return pos.y * sizes.x + pos.x;
    }
    public static int PosToIndex(int x, int y, int2 sizes)
    {
        return y * sizes.x + x;
    }
    public static int2 IndexToPos(int i, int2 sizes)
    {
        return new int2(i % sizes.x, i / sizes.y);
    }
    public static float2 IndexToUv(int i, int2 sizes)
    {
        return (float2)IndexToPos(i, sizes) / sizes;
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
