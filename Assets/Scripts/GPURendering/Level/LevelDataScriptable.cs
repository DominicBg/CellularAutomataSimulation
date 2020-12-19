using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LevelDataScriptable : ScriptableObject
{
    [HideInInspector] public ParticleType[] grid;
    [SerializeField] LevelContainer levelContainerPrefab = default;

    public LevelContainer LoadLevelContainer()
    {
        return MonoBehaviour.Instantiate(levelContainerPrefab);
    }

    public void SaveGrid(ParticleType[,] particleGrid)
    {
        grid = ArrayHelper.GetArrayFromGrid(particleGrid, GameManager.GridSizes);
    }

    public Map LoadMap()
    {
        ParticleType[,] particleGrid = ArrayHelper.GetGridFromArray(grid, GameManager.GridSizes);
        return new Map(particleGrid, GameManager.GridSizes);
    }

    public ParticleType[,] LoadGrid()
    {
        return ArrayHelper.GetGridFromArray(grid, GameManager.GridSizes);
    }

}
