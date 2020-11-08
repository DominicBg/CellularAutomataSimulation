using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    public int2 sizes = 100;

    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;

    [SerializeField] ParticleSpawner[] particleSpawners;

    NativeArray<Particle> particles;
    NativeArray<ParticleSpawner> nativeParticleSpawners;
    Map map;
    Unity.Mathematics.Random m_random;

    private void Awake()
    {
        Init();
    }
    private void OnDestroy()
    {
        particles.Dispose();
        nativeParticleSpawners.Dispose();
    }

    private void Init()
    {
        map = new Map(sizes);
        if(particles.IsCreated)
        {
            particles.Dispose();
            nativeParticleSpawners.Dispose();
        }

        particles = new NativeArray<Particle>(sizes.x * sizes.y, Allocator.Persistent);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(particleSpawners, Allocator.Persistent);

        player.Init(0);
        gridRenderer.Init(sizes);
        m_random.InitState();
    }

    public void Update()
    {
        if (player.TryUpdate(particles, map) || Input.GetKey(KeyCode.Space))
        {
            CellularAutomataJob cellularAutomataJob = new CellularAutomataJob();
            uint seed = m_random.NextUInt();
            cellularAutomataJob.map = map;
            cellularAutomataJob.particles = particles;
            cellularAutomataJob.nativeParticleSpawners = nativeParticleSpawners;
            cellularAutomataJob.random = new Unity.Mathematics.Random(seed);
            cellularAutomataJob.Schedule().Complete();

            gridRenderer.OnUpdate(particles, map, sizes, player);
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            Init();
        }
    }
}
