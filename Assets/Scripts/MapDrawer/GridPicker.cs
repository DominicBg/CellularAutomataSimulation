﻿using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GridPicker : MonoBehaviour
{
    public CanvasScaler canvas;
    public int2 GetGridPosition(int2 gridSizes)
    {
        float2 screen = new Vector2(Screen.width, Screen.height);
        float2 mousePosition = (Vector2)Input.mousePosition;
        float2 ratio = mousePosition / screen;
        float2 resolution = canvas.referenceResolution;

        float2 result = ratio * resolution;
        float xRatio = resolution.x / resolution.y;
        float xHalfRatio = xRatio * 0.5f;
        float xExtra = gridSizes.x * xHalfRatio;

        int x = (int)math.remap(0, resolution.x, -xExtra, gridSizes.x + xExtra, result.x);
        int y = (int)math.remap(0, resolution.y, 0, gridSizes.y, result.y);
        return new int2(x, y);
    }
}
