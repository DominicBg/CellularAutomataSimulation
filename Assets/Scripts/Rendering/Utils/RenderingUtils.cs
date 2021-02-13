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
                return (color1 + color2).Saturate();
            case BlendingMode.Multiply:
                return (color1 * color2).Saturate();
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

    public static Color BlendTransparentAdditive(Color color1, Color color2, float transparency, float additive)
    {
        return (Transparency(color1, color2, transparency) + color2 * additive).Saturate();
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
            math.max(color1.a, color2.a)
            ).Saturate();
    }

    public static float Overlay(float a, float b)
    {
        return (a < 0.5f) ? a * b : Screen(a, b);
    }

    public static Color Overlay(Color color1, Color color2)
    {
        return new Color(Overlay(color1.r, color2.r), Overlay(color1.g, color2.g), Overlay(color1.b, color2.b), Overlay(color1.a, color2.a)).Saturate();
    }

    public static float Screen(float a, float b)
    {
        return 1 - (1 - a) * (1 - b);
    }
    public static Color Screen(Color color1, Color color2)
    {
        return new Color(Screen(color1.r, color2.r), Screen(color1.g, color2.g), Screen(color1.b, color2.b), Screen(color1.a, color2.a)).Saturate();
    }

    public static float Luminance(Color color)
    {
        return 0.216f * color.r + 0.715f * color.g + 0.0722f * color.b;
    }


    public static bool ShouldRender(Color32 color, bool useAlphaMask)
    {
        return !useAlphaMask || (useAlphaMask && color.a != 0);
    }

    public static NativeArray<Color32> GetNativeArray(Texture2D texture, Allocator allocator)
    {
        return new NativeArray<Color32>(texture.GetPixels32(), allocator);
    }

    public static Color Saturate(this Color color)
    {
        return new Color(math.saturate(color.r), math.saturate(color.g), math.saturate(color.b), math.saturate(color.a));
    }

    public static bool Equals(Color32 colorA, Color32 colorB)
    {
        return colorA.r == colorB.r && colorA.g == colorB.g && colorA.b == colorB.b && colorA.a == colorB.a;
    }

    public static Color ReduceResolution(this Color color, int resolution)
    {
        float4 color4 = new float4(color.r, color.g, color.b, color.a);
        color4 = math.floor(color4 * resolution) / resolution;
        return new Color(color4.x, color4.y, color4.z, color4.w);
    }
    public static Color ReduceResolution(this Color color, float resolution)
    {
        float4 color4 = new float4(color.r, color.g, color.b, color.a);
        color4 = math.floor(color4 * resolution) / resolution;
        return new Color(color4.x, color4.y, color4.z, color4.w);
    }

    public static float3 ToNormal(this Color normalColor)
    {
        return math.normalize(new float3(normalColor.r, normalColor.g, normalColor.b) - 0.5f);
    }

    public static Color32 SampleTexture(in NativeSprite sprite, float2 uv)
    {
        int2 pixelCoord = (int2)math.clamp((uv * sprite.sizes-1), 0, sprite.sizes-1);
        return sprite.pixels[pixelCoord];
    }

    public static Color ApplyLightOnPixel(
        int2 position, int2 pixelPos, NativeList<LightSource> lights,
        System.Func<int2, Color> getColor, System.Func<int2, Color> getNormal,
        float z = 0, float minLightIntensity = .5f, float lightResolution = 25, bool flipNormal = false)
    {
        Color color = getColor(pixelPos);
        float3 normal = getNormal(pixelPos).ToNormal();
        if (flipNormal)
            normal.x = -normal.x;

        float3 pos3D = new float3(position.x, position.y, z);
        float lightIntensity = lights.CalculateLight(pos3D, normal);
        lightIntensity = MathUtils.ReduceResolution(lightIntensity, lightResolution);
        lightIntensity = math.remap(0, 1, minLightIntensity, 1, lightIntensity);

        return color * lightIntensity;
    }
}
