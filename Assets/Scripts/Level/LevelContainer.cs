using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelContainer : MonoBehaviour, IDisposable
{
    public LevelElement[] levelElements;
    public ParticleSpawnerElements particleSpawnerElements;

    GameLevelManager gameLevelManager;
    Map map;

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
            if(levelElements[i].isEnable)
                levelElements[i].OnUpdate(ref tickBlock);
        }
    }
    public void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isVisible)
                levelElements[i].OnRender(ref outputcolor, ref tickBlock);
        }
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].OnRenderUI(ref outputcolor, ref tickBlock);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < levelElements.Length; i++)
            levelElements[i].Dispose();
    }

    public NativeArray<ParticleSpawner> GetParticleSpawner()
    {
        if(particleSpawnerElements == null)
        {
            return new NativeArray<ParticleSpawner>(0, Allocator.Persistent);
        }

        NativeArray<ParticleSpawner> particleSpawners = new NativeArray<ParticleSpawner>(particleSpawnerElements.particleSpawners, Allocator.Persistent);
        return particleSpawners;
    }

    public void Unload()
    {
        Destroy(gameObject);
    }
}
