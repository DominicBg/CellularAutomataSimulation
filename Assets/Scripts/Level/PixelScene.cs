using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PixelScene : MonoBehaviour
{
    public LevelElement[] levelElements;
    [HideInInspector] public LevelObject[] levelObjects ;
    [HideInInspector] public IAlwaysRenderable[] alwaysRenderables;
    [HideInInspector] public ILightSource[] lightSources;


    public Map map;
    bool updateSimulation = true;

    public void OnValidate()
    {
        FindRefs();
    }
    void Awake()
    {
        FindRefs();
    }
    void FindRefs()
    {
        levelElements = GetComponentsInChildren<LevelElement>();
        levelObjects = GetComponentsInChildren<LevelObject>();
        alwaysRenderables = GetComponentsInChildren<IAlwaysRenderable>();
        lightSources = GetComponentsInChildren<ILightSource>();
    }


    public void Init(Map map)
    {
        this.map = map;
        for (int i = 0; i < levelElements.Length; i++)
        {
            //to remove null
            levelElements[i].Init(map);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock, int2 updatePos)
    {
        if (updateSimulation)
        {
            NativeList<int2> smokeEvents = new NativeList<int2>(25, Allocator.TempJob);
            Bound updateBound = Bound.CenterAligned(updatePos, GameManager.GridSizes * 2);

            //var particleSpawners = GetParticleSpawner();
            new CellularAutomataJob()
            {
                behaviour = GameManager.ParticleBehaviour,
                map = map,
                updateBound = updateBound,
                tickBlock = tickBlock,
                deltaTime = GameManager.DeltaTime,
                settings = GameManager.PhysiXVIISetings,
                particleSmokeEvent = smokeEvents
            }.Run();
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

        for (int i = 0; i < levelElements.Length; i++)
        {
            if (levelElements[i].isEnable)
                levelElements[i].OnLateUpdate(ref tickBlock);
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

    public void RequestInit(LevelElement levelElement)
    {
        if(!levelElement.isInit)
            levelElement.Init(map);
    }
}