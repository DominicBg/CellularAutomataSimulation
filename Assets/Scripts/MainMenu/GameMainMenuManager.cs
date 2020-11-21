﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class GameMainMenuManager : MonoBehaviour, State
{
    public MainMenuLightRender lightRender; 
    public MainMenuDarkRender darkRender; 

    //public LayerTexture title;
    //public StarBackgroundRendering starBackground;
    //public VoronoiRendering voronoiBackground;

    //[Header("Dark Textures")]
    //public LayerTexture darkBackground;
    //public LayerTexture darkAstronaut;

    //[Header("Light Textures")]
    //public LayerTexture lightSandBackground;
    //public LayerTexture lightCampFire;
    //public LayerTexture lightCampFireFlame;
    //public FireRendering fireRendering;
    //public MainMenuLightRender.ShadowRendering shadowRendering;
    //public LayerTexture lightAstronaut;

    [Header("Parameters")]
    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
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
        lightRender.Dispose();
        darkRender.Dispose();
        //darkBackground.Dispose();
        //darkAstronaut.Dispose();

        //lightCampFire.Dispose();
        //lightCampFireFlame.Dispose();
        //lightSandBackground.Dispose();
        //lightAstronaut.Dispose();


        //title.Dispose();

        m_map.Dispose();
        particleSpawners.Dispose();
    }

    public void OnStart()
    {
        tickBlock.Init();
        lightRender.Init();
        darkRender.Init();
        ////Load textures
        //darkBackground.Init();
        //darkAstronaut.Init();

        //lightCampFire.Init();
        //lightCampFireFlame.Init();
        //lightSandBackground.Init();
        //lightAstronaut.Init();

        //title.Init();

        //Load simulation
        levelData = mainMenuLevel.LoadLevel();
        m_map = new Map(levelData.grid, levelData.sizes);
        particleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();

        new CellularAutomataJob()
        {
            behaviour = partaicleBehaviour.particleBehaviour,
            map = m_map,
            nativeParticleSpawners = particleSpawners,
            tickBlock = tickBlock
        }.Run();
        m_map.SetParticleType(particleDestroyerPosition, ParticleType.None);
    }

    public void OnRender()
    {
        bool showDark = MathUtils.unorm(noise.cnoise(new float2(Time.time * randomSpeed, 0))) > lightThreshold;

        if (showDark)
        {
            darkRender.Render(ref tickBlock);
            //NativeArray<Color32> darkness = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            //darkBackground.Render(ref darkness);
            //voronoiBackground.Render(ref darkness, tickBlock.tick);
            //darkAstronaut.Render(ref darkness);
            //GridRenderer.RenderToScreen(darkness);
        }
        else
        {
            lightRender.Render(ref tickBlock, ref m_map);

            //var pass1 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            //starBackground.Render(ref pass1, tickBlock.tick);
            //GridRenderer.ApplyMapPixels(ref pass1, m_map, tickBlock);
            //title.Render(ref pass1);

            //var pass2 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            //GridRenderer.ApplyParticleRenderToTexture(ref pass2, ref lightSandBackground.nativeTexture, m_map, tickBlock, lightSandBackground.blending, ParticleType.Sand);
            //lightCampFire.Render(ref pass2);
            //shadowRendering.Render(ref pass2, tickBlock.tick);
            //GridRenderer.ApplyParticleRenderToTexture(ref pass2, ref lightCampFireFlame.nativeTexture, m_map, tickBlock, lightCampFireFlame.blending, ParticleType.Fire);

            //var pass3 = GridRenderer.CombineColors(ref pass1, ref pass2);

            //fireRendering.Render(ref pass3, tickBlock.tick);
            //lightAstronaut.Render(ref pass3);

            //GridRenderer.RenderToScreen(pass3);
        }
    }

    //[System.Serializable]
    //public struct FireRendering : IRenderableAnimated
    //{
    //    public int2 position;
    //    public Color32[] colors;
    //    public int[] radiusMin;
    //    public int[] radiusMax;
    //    public float speed;
    //    public BlendingMode blending;

    //    public void Render(ref NativeArray<Color32> colorArray, int tick)
    //    {
    //        for (int i = 0; i < colors.Length; i++)
    //        {
    //            float sin = math.sin(tick * speed) * 0.5f + 0.5f;
    //            int radius = (int)math.lerp(radiusMin[i], radiusMax[i], sin);
    //            GridRenderer.RenderCircle(ref colorArray, position, radius, colors[i], blending);
    //        }
    //    }
    //}

    //[System.Serializable]
    //public struct ShadowRendering : IRenderableAnimated
    //{
    //    public int2 position;
    //    public Color32[] colors;
    //    public int2[] radiusMin;
    //    public int2[] radiusMax;
    //    public float speed;
    //    public BlendingMode blending;

    //    public void Render(ref NativeArray<Color32> colorArray, int tick)
    //    {
    //        for (int i = 0; i < colors.Length; i++)
    //        {
    //            float sin = math.sin(tick * speed) * 0.5f + 0.5f;
    //            int2 radius = (int2)math.lerp(radiusMin[i], radiusMax[i], sin);
    //            GridRenderer.RenderEllipseMask(ref colorArray, position, radius, colors[i], blending);
    //        }
    //    }
    //}
}
