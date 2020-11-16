using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class OverworldBase : ScriptableObject
{
    public abstract void GetBackgroundColors(out NativeArray<Color32> backgroundColors);

    public Level[] levels;


    protected void GetBackgroundFromTexture(out NativeArray<Color32> backgroundColors, Texture2D texture)
    {
        if(texture.width != GameManager.GridSizes.x || texture.height != GameManager.GridSizes.y)
        {
            throw new System.Exception($"The background texture have sizes {texture.width}x{texture.height}, must have {GameManager.GridSizes}");
        }
        backgroundColors = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
    }

    [System.Serializable]
    public struct Level
    {
        public int2 position;
        public Texture2D icon;
        public LevelDataScriptable levelDataScriptable;
        public int[] connectionIndex;
    }
}
