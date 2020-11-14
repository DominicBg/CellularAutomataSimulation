using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    //Put from another function
    public LevelDataScriptable levelDataScriptable;
    public LevelData levelData;

    public static int Tick { get; private set; }
    public static uint TickSeed { get; private set; }

    public int2 sizes = 100;

    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;

    //Set inside levelData
    //[SerializeField] ParticleSpawner[] particleSpawners;

    NativeArray<ParticleSpawner> nativeParticleSpawners;

    public ParticleBehaviour behaviour;
    public bool emitParticle;

    public Map map;
    Unity.Mathematics.Random m_random;

    public float desiredFPS = 60;
    float currentDeltaTime;
    float frameDuration;

    private void OnValidate()
    {
        frameDuration = 1f / desiredFPS;
    }

    private void Awake()
    {
        LoadLevel(levelDataScriptable);
    }
    private void OnDestroy()
    {
        Dispose();
    }

    void Dispose()
    {
        if (nativeParticleSpawners.IsCreated)
        {
            nativeParticleSpawners.Dispose();
            map.Dispose();
        }
    }

    private void LoadLevel(LevelDataScriptable levelDataScriptable)
    {
        Dispose();

        levelData = levelDataScriptable.LoadLevel();
        map = new Map(levelData.grid, levelData.sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);

        player.Init(5, map);
        gridRenderer.Init(sizes);
        m_random.InitState();
    }

    public void Update()
    {
        //Force correct fps
        currentDeltaTime += Time.deltaTime;
        while(currentDeltaTime >= frameDuration)
        {
            Tick++;
            TickSeed = m_random.NextUInt();
            //Store tick to Ctrl+Z?

            FrameUpdate();
            currentDeltaTime -= frameDuration;

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            LoadLevel(levelDataScriptable);
        }
    }

    void FrameUpdate()
    {
        player.OnUpdate(map);
        //if (player.TryUpdate(map) || Input.GetKey(KeyCode.Space))
        {
            new CellularAutomataJob()
            {
                emitParticle = emitParticle,
                tick = Tick,
                behaviour = behaviour,
                map = map,
                nativeParticleSpawners = nativeParticleSpawners,
                random = new Unity.Mathematics.Random(TickSeed)
            }.Run();

            //find a way to parralelize
            //checker pattern?
            //cellularAutomataJob.Schedule().Complete();
            gridRenderer.OnUpdate(map, player.sprite, Tick, TickSeed);
        }
    }
}
