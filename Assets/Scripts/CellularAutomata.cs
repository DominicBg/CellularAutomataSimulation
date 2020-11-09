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

    NativeArray<Particle> particles;
    NativeArray<ParticleSpawner> nativeParticleSpawners;

    //NativeArray<PixelSprite> sprites; cancer a rendre burstable lol
    PixelSprite[] sprites = new PixelSprite[1];

    Map map;
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
        if (particles.IsCreated)
        {
            particles.Dispose();
            nativeParticleSpawners.Dispose();
        }
        map.Dispose();
    }

    private void Init()
    {
        map = new Map(sizes);
        Dispose();

        particles = new NativeArray<Particle>(sizes.x * sizes.y, Allocator.Persistent);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(particleSpawners, Allocator.Persistent);

        player.Init(0, particles, map);
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
        if (player.TryUpdate(particles, map) || Input.GetKey(KeyCode.Space))
        {
            //Recopy player sprites in native array
            sprites[0] = player.sprite;

            new CellularAutomataJob()
            {
                map = map,
                particles = particles,
                nativeParticleSpawners = nativeParticleSpawners,
                random = new Unity.Mathematics.Random(TickSeed)
            }.Run();
            //cellularAutomataJob.map = map;
            //cellularAutomataJob.particles = particles;
            //cellularAutomataJob.nativeParticleSpawners = nativeParticleSpawners;
            //cellularAutomataJob.random = new Unity.Mathematics.Random(TickSeed);

            //find a way to parralelize
            //checker pattern?
            //cellularAutomataJob.Schedule().Complete();
            //cellularAutomataJob.Run();
            gridRenderer.OnUpdate(particles, map, sprites);
        }
    }
}
