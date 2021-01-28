using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PixelScene : MonoBehaviour
{
    public LevelElement[] levelElements;
    [HideInInspector] public LevelObject[] levelObjects;
    [HideInInspector] public IAlwaysRenderable[] alwaysRenderables;
    [HideInInspector] public ILightSource[] lightSources;
    [HideInInspector] public ILightMultiSource[] lightMultiSource;

    public PlayerElement player { get; private set; }
    public Bound updateBound{ get; private set; }
    public Map map;
    bool updateSimulation = true;

    public void OnValidate()
    {
        FindRefs();
    }
    void Awake()
    {
        FindRefs();
        player = GetComponentInChildren<PlayerElement>();
    }
    void FindRefs()
    {
        levelElements = GetComponentsInChildren<LevelElement>();
        levelObjects = GetComponentsInChildren<LevelObject>();
        alwaysRenderables = GetComponentsInChildren<IAlwaysRenderable>();
        lightSources = GetComponentsInChildren<ILightSource>();
        lightMultiSource = GetComponentsInChildren<ILightMultiSource>();
    }

    public void Init(Map map)
    {
        this.map = map;
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(this);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock, int2 updatePos)
    {
        if (updateSimulation)
        {
            NativeList<int2> smokeEvents = new NativeList<int2>(25, Allocator.TempJob);
            updateBound = Bound.CenterAligned(updatePos, GameManager.GridSizes * 2);

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
            levelElement.Init(this);
    }
}