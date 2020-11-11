﻿using System.Collections;
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
        if (nativeParticleSpawners.IsCreated)
        {
            nativeParticleSpawners.Dispose();
            //sprites.Dispose();
            map.Dispose();
        }
    }

    private void Init()
    {
        Dispose();

        map = new Map(sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(particleSpawners, Allocator.Persistent);
        //sprites = new NativeArray<PixelSprite>(1, Allocator.Persistent);

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
        if (player.TryUpdate(map) || Input.GetKey(KeyCode.Space))
        {
            //Recopy player sprites in native array
            //sprites[0] = player.sprite;

            new CellularAutomataJob()
            {
                tick = Tick,
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
