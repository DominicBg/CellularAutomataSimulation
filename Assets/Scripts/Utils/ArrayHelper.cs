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
}
