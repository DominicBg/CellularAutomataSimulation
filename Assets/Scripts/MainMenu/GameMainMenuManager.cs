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
    public Texture2D darkBackground;
    public Texture2D darkAstronaut;
    public Texture2D lightBackground;
    public Texture2D lightAstronaut;

    public Texture2D title;

    NativeArray<Color32> nativeDarkBackground;
    NativeArray<Color32> nativeDarkAstronaut;
    NativeArray<Color32> nativeLightBackground;
    NativeArray<Color32> nativeLightAstronaut;
    NativeArray<Color32> nativeTitle;

    [Header("Parameters")]
    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
    public int2 firePosition;
    public Color32[] fireColors;
    public int[] fireRadius;

    [Header("References")]
    public ParticleBehaviourScriptable partaicleBehaviour;
    public LevelDataScriptable mainMenuLevel;

    LevelData levelData;
    Map m_map;
    TickBlock tickBlock;
    NativeArray<ParticleSpawner> particleSpawners;

    public void OnEnd()
    {
        nativeDarkBackground.Dispose();
        nativeDarkAstronaut.Dispose();
        nativeLightBackground.Dispose();
        nativeLightAstronaut.Dispose();
        nativeTitle.Dispose();

        m_map.Dispose();
        particleSpawners.Dispose();
    }

    public void OnStart()
    {
        tickBlock.Init();

        //Load textures
        nativeDarkBackground = RenderingUtils.GetNativeArray(darkBackground, Allocator.Persistent);
        nativeDarkAstronaut = RenderingUtils.GetNativeArray(darkAstronaut, Allocator.Persistent);
        nativeLightBackground = RenderingUtils.GetNativeArray(lightBackground, Allocator.Persistent);
        nativeLightAstronaut = RenderingUtils.GetNativeArray(lightAstronaut, Allocator.Persistent);
        nativeTitle = RenderingUtils.GetNativeArray(title, Allocator.Persistent);

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
        GridRenderer.ApplyMapPixels(ref colorArray, m_map, tickBlock);
        GridRenderer.RenderToScreen(colorArray);
    }

    void ShowTextures(ref NativeArray<Color32> colorArray)
    {
        bool showDark = (noise.cnoise(new float2(Time.time * randomSpeed, 0) * 0.5f + 0.5f)) > lightThreshold;

        if (showDark)
        {
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeDarkBackground);
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeDarkAstronaut);
        }
        else
        {
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeLightBackground);

            for (int i = 0; i < fireRadius.Length; i++)
            {
                GridRenderer.RenderCircle(ref colorArray, firePosition, fireRadius[i], fireColors[i]);
            }

            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeLightAstronaut);
        }

        GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeTitle);
    }
}
