using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PixelSceneData : MonoBehaviour
{
    [HideInInspector] public ParticleType[] grid;
    public int2 sizes;

    [/*HideInInspector, */SerializeField] private int2 internalSizes;

    [ContextMenu("Init")]
    public void Init()
    {
        internalSizes = sizes;
        grid = new ParticleType[sizes.x * sizes.y];
    }

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        Init();
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                float noiseValue = noise.cnoise(new float2(x, y) * 0.1f);
                grid[x + y * sizes.x] = noiseValue > .25f ? ParticleType.Rock : (noiseValue > .15f) ? ParticleType.Sand : ParticleType.None; 
            }
        }
    }

    public Map LoadMap()
    {
        ParticleType[,] particleGrid = ArrayHelper.GetGridFromArray(grid, internalSizes);
        return new Map(particleGrid, internalSizes);
    }

    //handle resizes without losing everything lol
}

//TODO add custom inspector to know if grid is created