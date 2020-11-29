﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UINavigationGraph;

public abstract class OverworldBase : ScriptableObject
{
    public abstract void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock);

    [SerializeField] UINavigationGraph navigationGraphPrefab;
    public LevelDataScriptable[] levels;

    public UINavigationGraph LoadNavigationGraph()
    {
        return MonoBehaviour.Instantiate(navigationGraphPrefab);
    }

    protected void GetBackgroundFromTexture(out NativeArray<Color32> backgroundColors, Texture2D texture)
    {
        if(texture.width != GameManager.GridSizes.x || texture.height != GameManager.GridSizes.y)
        {
            throw new System.Exception($"The background texture have sizes {texture.width}x{texture.height}, must have {GameManager.GridSizes}");
        }
        backgroundColors = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
    }

    //[System.Serializable]
    //public struct Level
    //{
    //    public int2 position;
    //    public Texture2D icon;
    //    public LevelContainer levelContainer;
    //    public int[] connectionIndex;
    //}
}
