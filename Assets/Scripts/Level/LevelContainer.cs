using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[RequireComponent(typeof(LevelContainer))]
public class LevelContainer : MonoBehaviour, IDisposable
{
    public int2 levelPosition;
    public LevelElement[] levelElements;
    public LevelEntrance[] entrances;
    public ParticleSpawnerElements particleSpawnerElements;

    public Map map;
    public NativeList<int2> particleSmokeEvent;

    public void OnValidate()
    {
        levelElements = GetComponentsInChildren<LevelElement>();
        entrances = GetComponentsInChildren<LevelEntrance>();
        particleSpawnerElements = GetComponentInChildren<ParticleSpawnerElements>();
    }

    public void Init(Map map)
    {
        this.map = map;
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(map, this);
        }
        particleSmokeEvent = new NativeList<int2>(Allocator.Persistent);
    }

    public void OnUpdate(ref TickBlock tickBlock)
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
            deltaTime = GameManager.DeltaTime,
            settings = GameManager.PhysiXVIISetings,
            particleSmokeEvent = particleSmokeEvent
        }.Run();

        HandleParticleEvents(ref tickBlock);


        //lol
        for (int i = 0; i < particleSpawnerElements.particleSpawners.Length; i++)
        {
            particleSpawnerElements.particleSpawners[i].particleSpawnCount = particleSpawners[i].particleSpawnCount;
        }
        particleSpawners.Dispose();

        //Update elements
        for (int i = 0; i < levelElements.Length; i++)
        {
            if(levelElements[i].isEnable)
                levelElements[i].OnUpdate(ref tickBlock);
        }
    }

    void HandleParticleEvents(ref TickBlock tickBlock)
    {
        //eww
        if (particleSmokeEvent.Length >= 0)
        {
            var smokeRenderer = GetComponentInChildren<LevelSmokeParticleRenderer>();
            for (int i = 0; i < particleSmokeEvent.Length; i++)
            {
                smokeRenderer.EmitParticle(particleSmokeEvent[i], ref tickBlock);
            }
        }
    }

    public void PreRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
            if (levelElements[i].isVisible)
                levelElements[i].PreRender(ref outputcolor, ref tickBlock);
    }

    public void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {   
        for (int i = 0; i < levelElements.Length; i++)
            if (levelElements[i].isVisible)
                levelElements[i].Render(ref outputcolor, ref tickBlock);
     }

    public void PostRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
            if (levelElements[i].isVisible)
                levelElements[i].PostRender(ref outputcolor, ref tickBlock);
    }

    public void RenderUI(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
            levelElements[i].RenderUI(ref outputcolor, ref tickBlock);

    }
    public void RenderDebug(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
            levelElements[i].RenderDebug(ref outputcolor, ref tickBlock);
    }

    public void Dispose()
    {
        particleSmokeEvent.Dispose();
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
