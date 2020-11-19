using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class GameMainMenuManager : MonoBehaviour, State
{
    [Header("Textures")]
    public LayerTexture darkBackground;
    public LayerTexture darkAstronaut;
    public LayerTexture lightBackground;
    public LayerTexture lightAstronaut;
    public LayerTexture title;

    [Header("Parameters")]
    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
    public FireRendering fireRendering;

    [Header("References")]
    public ParticleBehaviourScriptable partaicleBehaviour;
    public LevelDataScriptable mainMenuLevel;

    LevelData levelData;
    Map m_map;
    TickBlock tickBlock;
    NativeArray<ParticleSpawner> particleSpawners;

    public void OnEnd()
    {
        darkBackground.Dispose();
        darkAstronaut.Dispose();
        lightBackground.Dispose();
        lightAstronaut.Dispose();
        title.Dispose();

        m_map.Dispose();
        particleSpawners.Dispose();
    }

    public void OnStart()
    {
        tickBlock.Init();

        //Load textures
        darkBackground.Init();
        darkAstronaut.Init();
        lightBackground.Init();
        lightAstronaut.Init();
        title.Init();

        //Load simulation
        levelData = mainMenuLevel.LoadLevel();
        m_map = new Map(levelData.grid, levelData.sizes);
        particleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        NativeArray<Color32> colorArray = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);

        new CellularAutomataJob()
        {
            behaviour = partaicleBehaviour.particleBehaviour,
            map = m_map,
            nativeParticleSpawners = particleSpawners,
            tickBlock = tickBlock
        }.Run();

        ShowTextures(ref colorArray);
        //GridRenderer.ApplyMapPixels(ref colorArray, m_map, tickBlock);
        GridRenderer.RenderToScreen(colorArray);
    }

    void ShowTextures(ref NativeArray<Color32> colorArray)
    {
        bool showDark = (noise.cnoise(new float2(Time.time * randomSpeed, 0) * 0.5f + 0.5f)) > lightThreshold;

        if (showDark)
        {
            darkBackground.Render(ref colorArray);
            darkAstronaut.Render(ref colorArray);
        }
        else
        {
            lightBackground.Render(ref colorArray);
            fireRendering.Render(ref colorArray);
            lightAstronaut.Render(ref colorArray);
            GridRenderer.ApplyMapPixels(ref colorArray, m_map, tickBlock);
        }
        title.Render(ref colorArray);
    }

    [System.Serializable]
    public struct FireRendering : IRenderable
    {
        public int2 firePosition;
        public Color32[] fireColors;
        public int[] fireRadius;
        public BlendingMode fireBlending;

        public void Render(ref NativeArray<Color32> colorArray)
        {
            for (int i = 0; i < fireRadius.Length; i++)
            {
                GridRenderer.RenderCircle(ref colorArray, firePosition, fireRadius[i], fireColors[i], fireBlending);
            }
        }
    }
}
