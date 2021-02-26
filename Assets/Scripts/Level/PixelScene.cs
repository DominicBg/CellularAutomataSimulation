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
    [HideInInspector] public PhysicObject[] physicObjects;
    [HideInInspector] public IAlwaysRenderable[] alwaysRenderables;
    [HideInInspector] public ILightSource[] lightSources;
    [HideInInspector] public ILightMultiSource[] lightMultiSource;
    [HideInInspector] public PixelSceneParticleEvents sceneParticleEvents;

    public int2 CameraPosition => pixelCamera.position;
    public Player player { get; private set; }
    public PixelCamera pixelCamera { get; private set; }
    public Bound updateBound{ get; private set; }
    public Map map;
    bool updateSimulation = true;
    public int CurrentTick { get; private set; }

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
        player = GetComponentInChildren<Player>();
        levelElements = GetComponentsInChildren<LevelElement>();
        physicObjects = GetComponentsInChildren<PhysicObject>();
        levelObjects = GetComponentsInChildren<LevelObject>();
        alwaysRenderables = GetComponentsInChildren<IAlwaysRenderable>();
        lightSources = GetComponentsInChildren<ILightSource>();
        lightMultiSource = GetComponentsInChildren<ILightMultiSource>();
        sceneParticleEvents = GetComponentInChildren<PixelSceneParticleEvents>();
    }

    public void Init(Map map, PixelCamera camera)
    {
        this.pixelCamera = camera;
        this.map = map;
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(this);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock, int2 updatePos)
    {
        CurrentTick = tickBlock.tick;
        if (updateSimulation)
        {
            updateBound = Bound.CenterAligned(updatePos, GameManager.RenderSizes * 2);
            new CellularAutomataJob()
            {
                behaviour = GameManager.ParticleBehaviour,
                map = map,
                updateBound = updateBound,
                tickBlock = tickBlock,
                deltaTime = GameManager.DeltaTime,
                settings = GameManager.PhysiXVIISetings,
                particleCombustionEvents = sceneParticleEvents.particleEvents.combustionEvents
            }.Run();
            sceneParticleEvents.UpdateParticleEvents(ref tickBlock);  
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