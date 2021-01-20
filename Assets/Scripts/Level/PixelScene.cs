using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PixelScene : MonoBehaviour
{
    public LevelElement[] levelElements;
    public Map map;
    bool updateSimulation = true;

    public void OnValidate()
    {
        levelElements = GetComponentsInChildren<LevelElement>();
    }

    public void Init(Map map)
    {
        this.map = map;
        for (int i = 0; i < levelElements.Length; i++)
        {
            //to remove null
            levelElements[i].Init(map, null);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock, int2 updatePos)
    {
        if (updateSimulation)
        {
            NativeArray<ParticleSpawner> spawners = new NativeArray<ParticleSpawner>(0, Allocator.TempJob);
            NativeList<int2> smokeEvents = new NativeList<int2>(25, Allocator.TempJob);
            Bound updateBound = Bound.CenterAligned(updatePos, GameManager.GridSizes * 2);

            //var particleSpawners = GetParticleSpawner();
            new CellularAutomataJob()
            {
                behaviour = GameManager.ParticleBehaviour,
                map = map,
                updateBound = updateBound,
                nativeParticleSpawners = spawners,
                tickBlock = tickBlock,
                deltaTime = GameManager.DeltaTime,
                settings = GameManager.PhysiXVIISetings,
                particleSmokeEvent = smokeEvents
            }.Run();
            spawners.Dispose();
            smokeEvents.Dispose();
            //HandleParticleEvents(ref tickBlock);

            ////lol
            //for (int i = 0; i < particleSpawnerElements.particleSpawners.Length; i++)
            //{
            //    particleSpawnerElements.particleSpawners[i].particleSpawnCount = particleSpawners[i].particleSpawnCount;
            //}
            //particleSpawners.Dispose();
        }


        //Update elements
        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isEnable)
                levelElements[i].OnUpdate(ref tickBlock);
        }
    }

    public void Dispose()
    {
        map.Dispose();
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Dispose();
        }
    }
}