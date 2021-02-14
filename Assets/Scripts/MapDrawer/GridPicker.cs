using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GridPicker : MonoBehaviour
{
    static GridPicker instance;
    public CanvasScaler canvas;

    public void Awake()
    {
        instance = this;
    }


    public static int2 GetGridPosition()
    {
        return GetGridPosition(GameManager.RenderSizes);
    }

    public static int2 GetGridPosition(int2 gridSizes)
    {
        float2 screen = new Vector2(Screen.width, Screen.height);
        float2 mousePosition = (Vector2)Input.mousePosition;
        float2 ratio = mousePosition / screen;
        float2 resolution = instance.canvas.referenceResolution;

        float2 result = ratio * resolution;
        float xRatio = resolution.x / resolution.y;
        float xHalfRatio = xRatio * 0.5f;
        float xExtra = gridSizes.x * xHalfRatio;

        int x = (int)math.remap(0, resolution.x, -xExtra, gridSizes.x + xExtra, result.x);
        int y = (int)math.remap(0, resolution.y, 0, gridSizes.y, result.y);
        return math.clamp(new int2(x, y), 0, GameManager.RenderSizes -1);
    }
}
