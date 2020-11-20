using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public enum BlendingMode { Normal, Transparency, Additive, Multiply, Screen, Overlay, HardLight, SoftLight }
public static class RenderingUtils
{
    public static Color Blend(Color color1, Color color2, BlendingMode blending)
    {
        switch (blending)
        {
            case BlendingMode.Normal:
                return Color.Lerp(color1, color2, color2.a);
            case BlendingMode.Transparency:
                return Transparency(color1, color2, color2.a);
            case BlendingMode.Additive:
                return (color1 + color2).Clamp01();
            case BlendingMode.Multiply:
                return (color1 * color2).Clamp01();
            case BlendingMode.Screen:
                return Screen(color1, color2);
            case BlendingMode.Overlay:
                return Overlay(color1, color2);

            case BlendingMode.HardLight:
                break;
            case BlendingMode.SoftLight:
                break;
        }
        //TODO finish the rest lol
        return color2;
    }

    public static float Transparency(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
    }
    public static Color Transparency(Color color1, Color color2, float t)
    {
        return new Color(
            Transparency(color1.r, color2.r, t),
            Transparency(color1.g, color2.g, t),
            Transparency(color1.b, color2.b, t),
            1
            ).Clamp01();
    }

    public static float Overlay(float a, float b)
    {
        return (a < 0.5f) ? a * b : Screen(a, b);
    }

    public static Color Overlay(Color color1, Color color2)
    {
        return new Color(Overlay(color1.r, color2.r), Overlay(color1.g, color2.g), Overlay(color1.b, color2.b), Overlay(color1.a, color2.a)).Clamp01();
    }

    public static float Screen(float a, float b)
    {
        return 1 - (1 - a) * (1 - b);
    }
    public static Color Screen(Color color1, Color color2)
    {
        return new Color(Screen(color1.r, color2.r), Screen(color1.g, color2.g), Screen(color1.b, color2.b), Screen(color1.a, color2.a)).Clamp01();
    }

    public static float Luminance(Color32 color)
    {
        return 0.216f * color.r + 0.715f * color.g + 0.0722f * color.b;
    }

    public static void GetColoredCircle(int2 centerPosition, int radius, int2 mapSizes, Color32 color, Allocator allocator, out NativeArray<int2> positions, out NativeArray<Color32> colors)
    {
        //todo burst
        positions = GridHelper.GetCircleAtPosition(centerPosition, radius, mapSizes, allocator);

        //Init with good memory?
        colors = new NativeArray<Color32>(positions.Length, allocator);
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }
    }

    public static void GetEllipseMask(int2 centerPosition, int2 radius, int2 mapSizes, Color32 color, Allocator allocator, out NativeArray<Color32> colors)
    {
        //todo burst
        var positions = GridHelper.GetEllipseAtPosition(centerPosition, radius, mapSizes, allocator);
        //Init with good memory?

        colors = new NativeArray<Color32>(mapSizes.x * mapSizes.y, allocator);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        //Remove ellipse
        for (int i = 0; i < positions.Length; i++)
        {
            int index = ArrayHelper.PosToIndex(positions[i], mapSizes);
            colors[index] = Color.clear;
        }
        positions.Dispose();
    }




    public static NativeArray<Color32> GetNativeArray(Texture2D texture, Allocator allocator)
    {
        return new NativeArray<Color32>(texture.GetPixels32(), allocator);
    }

    public static Color Clamp01(this Color color)
    {
        return new Color(math.saturate(color.r), math.saturate(color.g), math.saturate(color.b), math.saturate(color.a));
    }
}
