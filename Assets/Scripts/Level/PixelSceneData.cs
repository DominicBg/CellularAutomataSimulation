using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class PixelSceneData
{
    [HideInInspector] public ParticleType[] grid;
    [HideInInspector] public int2 sizes;

    public void CreateEmpty(int2 sizes)
    {
        grid = new ParticleType[sizes.x * sizes.y];
        this.sizes = sizes;
    }

    public Map LoadMap()
    {
        ParticleType[,] particleGrid = ArrayHelper.GetGridFromArray(grid, sizes);
        return new Map(particleGrid, sizes);
    }
    public void SaveMap(ParticleType[,] particleGrid, int2 sizes)
    {
        this.sizes = sizes;
        grid = ArrayHelper.GetArrayFromGrid(particleGrid, sizes);
    }
    //handle resizes without losing everything lol
}

//TODO add custom inspector to know if grid is created