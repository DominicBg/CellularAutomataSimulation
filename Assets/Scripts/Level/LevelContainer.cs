using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[RequireComponent(typeof(LevelContainer))]
public class LevelContainer : MonoBehaviour, IDisposable
{
    public LevelElement[] levelElements;
    public LevelEntrance[] entrances;
    public ParticleSpawnerElements particleSpawnerElements;

    public TickBlock tickBlock;
    public Map map;

    public bool inDebug;

    public void OnValidate()
    {
        levelElements = GetComponentsInChildren<LevelElement>();
        entrances = GetComponentsInChildren<LevelEntrance>();
        particleSpawnerElements = GetComponentInChildren<ParticleSpawnerElements>();
    }

    public void Init(Map map)
    {
        this.map = map;
        tickBlock.Init();
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(map);
        }
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();

        //Update simulation
        var particleSpawners = GetParticleSpawner();
        new CellularAutomataJob()
        {
            behaviour = GameManager.ParticleBehaviour,
            map = map,
            nativeParticleSpawners = particleSpawners,
            tickBlock = tickBlock,
            settings = GameManager.PhysiXVIISetings
        }.Run();
        particleSpawners.Dispose();

        //Update elements
        for (int i = 0; i < levelElements.Length; i++)
        {
            if(levelElements[i].isEnable)
                levelElements[i].OnUpdate(ref tickBlock);
        }
    }
    public void OnRender(ref NativeArray<Color32> outputcolor)
    {
        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isVisible)
                levelElements[i].PreRender(ref outputcolor, ref tickBlock);
        }
        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isVisible)
                levelElements[i].Render(ref outputcolor, ref tickBlock);
        }
        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isVisible)
                levelElements[i].PostRender(ref outputcolor, ref tickBlock);
        }
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].RenderUI(ref outputcolor, ref tickBlock);
        }

        if(inDebug)
        {
            for (int i = 0; i < levelElements.Length; i++)
            {
                levelElements[i].OnRenderDebug(ref outputcolor, ref tickBlock);
            }
        }

        PostProcessManager.Instance.Render(ref outputcolor, ref tickBlock);
    }

    public void Dispose()
    {
        map.Dispose();
        for (int i = 0; i < levelElements.Length; i++)
            levelElements[i].Dispose();

        if(gameObject != null)
            Destroy(gameObject);
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
}
