using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelContainer : MonoBehaviour
{
    public LevelDataScriptable levelDataScriptable;
    public LevelElement[] levelElements;
    public ParticleSpawnerElements particleSpawnerElements;

    GameLevelManager gameLevelManager;
    Map map;

    public void SaveGrid(ParticleType[,] particleGrid)
    {
        levelDataScriptable.grid = ArrayHelper.GetArrayFromGrid(particleGrid, GameManager.GridSizes);
    }

    public Map LoadMap()
    {
        ParticleType[,] particleGrid = ArrayHelper.GetGridFromArray(levelDataScriptable.grid, GameManager.GridSizes);
        return new Map(particleGrid, GameManager.GridSizes);
    }

    public ParticleType[,] LoadGrid()
    {
        return ArrayHelper.GetGridFromArray(levelDataScriptable.grid, GameManager.GridSizes);
    }


    public void OnValidate()
    {
        levelElements = GetComponents<LevelElement>();
        particleSpawnerElements = GetComponent<ParticleSpawnerElements>();
    }

    public void Init(GameLevelManager gameLevelManager, Map map)
    {
        this.gameLevelManager = gameLevelManager;
        this.map = map;

        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(gameLevelManager, map);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock)
    {
        gameLevelManager.UpdateSimulation();
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].OnUpdate(ref tickBlock);
        }
    }
    public void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].OnRender(ref outputcolor, ref tickBlock);
        }
    }

    public NativeArray<ParticleSpawner> GetParticleSpawner()
    {
        NativeArray<ParticleSpawner> particleSpawners = new NativeArray<ParticleSpawner>(particleSpawnerElements.particleSpawners, Allocator.Persistent);
        return particleSpawners;
    }
}
