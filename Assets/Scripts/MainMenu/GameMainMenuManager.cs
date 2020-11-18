using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class GameMainMenuManager : MonoBehaviour, State
{
    public Texture2D darkBackground;
    public Texture2D darkAstronaut;
    public Texture2D lightBackground;
    public Texture2D lightAstronaut;

    public Texture2D title;


    public NativeArray<Color32> nativeDarkBackground;
    public NativeArray<Color32> nativeDarkAstronaut;
    public NativeArray<Color32> nativeLightBackground;
    public NativeArray<Color32> nativeLightAstronaut;
    public NativeArray<Color32> nativeTitle;

    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
    public LevelDataScriptable mainMenuLevel;

    LevelData levelData;
    Map m_map;
    Unity.Mathematics.Random m_random;
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
        nativeDarkBackground = GetNativeArray(darkBackground, Allocator.Persistent);
        nativeDarkAstronaut = GetNativeArray(darkAstronaut, Allocator.Persistent);
        nativeLightBackground = GetNativeArray(lightBackground, Allocator.Persistent);
        nativeLightAstronaut = GetNativeArray(lightAstronaut, Allocator.Persistent);
        nativeTitle = GetNativeArray(title, Allocator.Persistent);

        levelData = mainMenuLevel.LoadLevel();
        m_map = new Map(levelData.grid, levelData.sizes);
        particleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);
        m_random.InitState();
    }

    public void OnUpdate()
    {
        NativeArray<Color32> colorArray = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);

        new CellularAutomataJob()
        {
            tick = 0,
            behaviour = new ParticleBehaviour(),
            map = m_map,
            nativeParticleSpawners = particleSpawners,
            random = new Unity.Mathematics.Random(GridRenderer.randomTick)
        }.Run();

        ShowTextures(ref colorArray);
        GridRenderer.ApplyMapPixels(ref colorArray, m_map);
        GridRenderer.RenderToScreen(colorArray);
    }

    void ShowTextures(ref NativeArray<Color32> colorArray)
    {
        bool showDark = (noise.cnoise(new float2(Time.time * randomSpeed, 0) * 0.5f + 0.5f)) > lightThreshold;

        if (showDark)
        {
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeDarkBackground, ApplyTextureJob.Blending.OverrideAlpha);
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeDarkAstronaut, ApplyTextureJob.Blending.OverrideAlpha);
        }
        else
        {
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeLightBackground, ApplyTextureJob.Blending.OverrideAlpha);
            GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeLightAstronaut, ApplyTextureJob.Blending.OverrideAlpha);
        }

        GridRenderer.ApplyTextureToColor(ref colorArray, ref nativeTitle, ApplyTextureJob.Blending.OverrideAlpha);
    }


    public NativeArray<Color32> GetNativeArray(Texture2D texture, Allocator allocator)
    {
        return new NativeArray<Color32>(texture.GetPixels32(), allocator);
    }

}
