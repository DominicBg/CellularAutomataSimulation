using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class GameMainMenuManager : MonoBehaviour, State
{
    public LayerTexture title;

    [Header("Dark Textures")]
    public LayerTexture darkBackground;
    public LayerTexture darkAstronaut;

    [Header("Light Textures")]
    public LayerTexture lightSandBackground;
    public LayerTexture lightCampFire;

    public LayerTexture lightAstronaut;

    [Header("Parameters")]
    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
    public FireRendering fireRendering;
    public int2 particleDestroyerPosition;

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
        lightCampFire.Dispose();
        lightSandBackground.Dispose();
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
        lightCampFire.Init();
        lightSandBackground.Init();
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
        m_map.SetParticleType(particleDestroyerPosition, ParticleType.None);

        ShowTextures(ref colorArray);
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
            GridRenderer.ApplyParticleRenderToTexture(ref colorArray, ref lightSandBackground.nativeTexture, m_map, tickBlock, ParticleType.Sand);
            lightCampFire.Render(ref colorArray);
            fireRendering.Render(ref colorArray, tickBlock.tick);
            lightAstronaut.Render(ref colorArray);
            GridRenderer.ApplyMapPixels(ref colorArray, m_map, tickBlock);
        }
        title.Render(ref colorArray);
    }

    [System.Serializable]
    public struct FireRendering : IRenderableAnimated
    {
        public int2 firePosition;
        public Color32[] fireColors;
        public int[] fireRadiusMin;
        public int[] fireRadiusMax;
        public float speed;
        public BlendingMode fireBlending;

        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            for (int i = 0; i < fireColors.Length; i++)
            {
                float sin = math.sin(tick * speed) * 0.5f + 0.5f;
                int radius = (int)math.lerp(fireRadiusMin[i], fireRadiusMax[i], sin);
                GridRenderer.RenderCircle(ref colorArray, firePosition, radius, fireColors[i], fireBlending);
            }
        }
    }
}
