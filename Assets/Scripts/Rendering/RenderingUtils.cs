using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class RenderingUtils
{
    public static float Luminance(Color32 color)
    {
        return 0.216f * color.r + 0.715f * color.g + 0.0722f * color.b;
    }

    public static NativeArray<Color32> GetNativeArray(Texture2D texture, Allocator allocator)
    {
        return new NativeArray<Color32>(texture.GetPixels32(), allocator);
    }
}
