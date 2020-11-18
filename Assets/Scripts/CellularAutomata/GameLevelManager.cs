using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public static int Tick { get; private set; }
    public static uint TickSeed { get; private set; }

    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;

    NativeArray<ParticleSpawner> nativeParticleSpawners;

    //todo generalize this
    PixelSprite[] pixelSprites = new PixelSprite[2];

    public ParticleBehaviour behaviour;

    public Map map;
    Unity.Mathematics.Random m_random;

    LevelData levelData;

    //TEMP
    public PixelSortingRenderingSettings[] pixelSortingRenderingSettings;

    public void OnStart()
    {
        LoadLevel(GameManager.Instance.currentLevel);
    }

    public void OnEnd()
    {
        Dispose();
    }

    void Dispose()
    {
        if (nativeParticleSpawners.IsCreated)
        {
            nativeParticleSpawners.Dispose();
            map.Dispose();

            for (int i = 0; i < pixelSprites.Length; i++)
            {
                pixelSprites[i].Dispose();
            }
        }
    }

    public void LoadLevel(LevelDataScriptable levelDataScriptable)
    {
        Dispose();

        levelData = levelDataScriptable.LoadLevel();
        map = new Map(levelData.grid, levelData.sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);

        pixelSprites[0] = new PixelSprite(levelData.playerPosition, levelData.playerTexture);
        pixelSprites[1] = new PixelSprite(levelData.shuttlePosition, levelData.shuttleTexture);

        player.Init(ref pixelSprites[0], map);
        m_random.InitState();
    }

    public void OnUpdate()
    {
        Tick++;
        TickSeed = m_random.NextUInt();
        FrameUpdate();
    }

    void FrameUpdate()
    {
        player.OnUpdate(ref pixelSprites[0], map);      
        new CellularAutomataJob()
        {
            tick = Tick,
            behaviour = behaviour,
            map = map,
            nativeParticleSpawners = nativeParticleSpawners,
            random = new Unity.Mathematics.Random(TickSeed)
        }.Run();

        //find a way to parralelize
        //checker pattern?
        //cellularAutomataJob.Schedule().Complete();


        //todo make this less ugly lol
        for (int i = 0; i < pixelSortingRenderingSettings.Length; i++)
        {
            GridRenderer.postProcess.pixelSortingRequestQueue.Enqueue(pixelSortingRenderingSettings[i]);
        }

        GridRenderer.RenderMapAndSprites(map, pixelSprites, Tick, TickSeed);        
    }

}
