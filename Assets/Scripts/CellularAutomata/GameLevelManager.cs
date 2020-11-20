﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;

    NativeArray<ParticleSpawner> nativeParticleSpawners;

    PixelSprite playerSprite;
    PixelSprite shuttleSprite;

    public ParticleBehaviourScriptable particleBehaviour;

    public Map map;
    TickBlock tickBlock;
    LevelData levelData;

    public enum LevelPhase { into, gameplay, ending}
    LevelPhase m_levelPhase;
    int tickAtPhase;

    //TEMP
    public PixelSortingSettings[] pixelSortingSettings;

    public void OnStart()
    {
        m_levelPhase = LevelPhase.gameplay;
        tickAtPhase = 0;
        LoadLevel(GameManager.Instance.currentLevel);
        tickBlock.Init();
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

            playerSprite.Dispose();
            shuttleSprite.Dispose();
        }
    }

    public void LoadLevel(LevelDataScriptable levelDataScriptable)
    {
        Dispose();

        levelData = levelDataScriptable.LoadLevel();
        map = new Map(levelData.grid, levelData.sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);

        playerSprite = new PixelSprite(levelData.playerPosition, levelData.playerTexture);
        shuttleSprite = new PixelSprite(levelData.shuttlePosition, levelData.shuttleTexture);

        player.Init(ref playerSprite, map);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        tickAtPhase++;

        if (m_levelPhase == LevelPhase.gameplay)
        {
            player.OnUpdate(ref playerSprite, map);
        }

        new CellularAutomataJob()
        {
            behaviour = particleBehaviour.particleBehaviour,
            map = map,
            nativeParticleSpawners = nativeParticleSpawners,
            tickBlock = tickBlock
        }.Run();

        if (m_levelPhase == LevelPhase.gameplay && PlayerFinishedLevel())
        {
            Debug.Log("BRAVO");
            map.RemoveSpriteAtPosition(ref playerSprite);
            m_levelPhase = LevelPhase.ending;
            tickAtPhase = 0;
        }
        else if (m_levelPhase == LevelPhase.ending)
        {
            shuttleSprite.position += new int2(0, 1);
            if(tickAtPhase > 60)
            {
                GameManager.Instance.SetOverworld();
            }
        }
    }

    public void OnRender()
    {
        var outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);

        if(m_levelPhase == LevelPhase.gameplay)
            GridRenderer.ApplySprite(ref outputColor, playerSprite);

        GridRenderer.ApplySprite(ref outputColor, shuttleSprite);

        for (int i = 0; i < pixelSortingSettings.Length; i++)
            GridPostProcess.ApplyPixelSorting(ref outputColor, ref pixelSortingSettings[i]);

        GridRenderer.RenderToScreen(outputColor);
    }

    bool PlayerFinishedLevel()
    {
        return playerSprite.Bound.IntersectWith(shuttleSprite.Bound);
    }
}
