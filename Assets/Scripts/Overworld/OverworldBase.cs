using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UINavigationGraph;

public abstract class OverworldBase : ScriptableObject
{
    public abstract void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock);

    [SerializeField] UINavigationGraph navigationGraphPrefab = default;
    public WorldLevel levelPrefab;

    public UINavigationGraph LoadNavigationGraph()
    {
        return MonoBehaviour.Instantiate(navigationGraphPrefab);
    }

    protected void GetBackgroundFromTexture(out NativeArray<Color32> backgroundColors, Texture2D texture)
    {
        if(texture.width != GameManager.RenderSizes.x || texture.height != GameManager.RenderSizes.y)
        {
            throw new System.Exception($"The background texture have sizes {texture.width}x{texture.height}, must have {GameManager.RenderSizes}");
        }
        backgroundColors = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
    }
}
