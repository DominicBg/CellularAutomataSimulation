using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public enum BlendingMode { Normal, Additive }
public static class RenderingUtils
{

    public static Color32 Blend(Color32 color1, Color32 color2, BlendingMode blending)
    {
        switch (blending)
        {
            case BlendingMode.Normal:
                return Color32.Lerp(color1, color2, color2.a / 255f);
            case BlendingMode.Additive:
                //return new Color32((int)color1.r + (int)color2.r, color1.g + color2.g, color1.b + color2.b, color1.a + color2.a);
                break;
        }
        //TODO finish the rest lol
        return color2;
    }

    public static float Luminance(Color32 color)
    {
        return 0.216f * color.r + 0.715f * color.g + 0.0722f * color.b;
    }

    public static void GetColoredCircle(int2 centerPosition, int radius, int2 mapSizes, Color32 color, Allocator allocator, out NativeArray<int2> positions, out NativeArray<Color32> colors)
    {
        positions = GridHelper.GetCircleAtPosition(centerPosition, radius, mapSizes, allocator);
        //Init with good memory?
        colors = new NativeArray<Color32>(positions.Length, allocator);
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }
    }



    public static NativeArray<Color32> GetNativeArray(Texture2D texture, Allocator allocator)
    {
        return new NativeArray<Color32>(texture.GetPixels32(), allocator);
    }
}
