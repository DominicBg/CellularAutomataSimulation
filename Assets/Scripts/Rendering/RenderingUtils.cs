using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderingUtils
{
    public static float Luminance(Color32 color)
    {
        return 0.216f * color.r + 0.715f * color.g + 0.0722f * color.b;
    }
}
