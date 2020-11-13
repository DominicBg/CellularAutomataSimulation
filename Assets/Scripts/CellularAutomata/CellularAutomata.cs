using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    public static int Tick { get; private set; }
    public static uint TickSeed { get; private set; }

    public int2 sizes = 100;

    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;

    [SerializeField] ParticleSpawner[] particleSpawners;

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
        Init();
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

    private void Init()
    {
        Dispose();

        map = new Map(sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(particleSpawners, Allocator.Persistent);

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
            Init();
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
            gridRenderer.OnUpdate(map, player.sprite);
        }
    }
}
