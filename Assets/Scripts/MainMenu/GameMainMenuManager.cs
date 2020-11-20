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
    public StarBackgroundRendering starBackground;

    [Header("Dark Textures")]
    public LayerTexture darkBackground;
    public LayerTexture darkAstronaut;

    [Header("Light Textures")]
    public LayerTexture lightSandBackground;
    public LayerTexture lightCampFire;
    public LayerTexture lightCampFireFlame;
    public FireRendering fireRendering;
    public ShadowRendering shadowRendering;
    public LayerTexture lightAstronaut;

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
        darkBackground.Dispose();
        darkAstronaut.Dispose();

        lightCampFire.Dispose();
        lightCampFireFlame.Dispose();
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
        lightCampFireFlame.Init();
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

        new CellularAutomataJob()
        {
            behaviour = partaicleBehaviour.particleBehaviour,
            map = m_map,
            nativeParticleSpawners = particleSpawners,
            tickBlock = tickBlock
        }.Run();
        m_map.SetParticleType(particleDestroyerPosition, ParticleType.None);

        ShowTextures();
    }

    void ShowTextures()
    {
        bool showDark = (noise.cnoise(new float2(Time.time * randomSpeed, 0) * 0.5f + 0.5f)) > lightThreshold;

        if (showDark)
        {
            NativeArray<Color32> darkness = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            darkBackground.Render(ref darkness);
            darkAstronaut.Render(ref darkness);
            GridRenderer.RenderToScreen(darkness);

        }
        else
        {
            //GenerateDarkBackground(out NativeArray<Color32> background);
            //GenerateDarkforeground(out NativeArray<Color32> foreground);
            //var output = GridRenderer.CombineColors(ref background, ref foreground);
            //GridRenderer.RenderToScreen(output);

            var pass1 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            starBackground.Render(ref pass1, tickBlock.tick);
            GridRenderer.ApplyMapPixels(ref pass1, m_map, tickBlock);
            title.Render(ref pass1);

            var pass2 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            GridRenderer.ApplyParticleRenderToTexture(ref pass2, ref lightSandBackground.nativeTexture, m_map, tickBlock, lightSandBackground.blending, ParticleType.Sand);
            lightCampFire.Render(ref pass2);
            shadowRendering.Render(ref pass2, tickBlock.tick);
            GridRenderer.ApplyParticleRenderToTexture(ref pass2, ref lightCampFireFlame.nativeTexture, m_map, tickBlock, lightCampFireFlame.blending, ParticleType.Fire);

            var pass3 = GridRenderer.CombineColors(ref pass1, ref pass2);

            fireRendering.Render(ref pass3, tickBlock.tick);
            lightAstronaut.Render(ref pass3);

            GridRenderer.RenderToScreen(pass3);
        }
    }
    
    
    void GenerateDarkBackground(out NativeArray<Color32> background)
    {
        background = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        starBackground.Render(ref background, tickBlock.tick);
        GridRenderer.ApplyMapPixels(ref background, m_map, tickBlock);
        title.Render(ref background);
    }
    void GenerateDarkforeground(out NativeArray<Color32> foreGround)
    {
        foreGround = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        GridRenderer.ApplyParticleRenderToTexture(ref foreGround, ref lightSandBackground.nativeTexture, m_map, tickBlock, lightSandBackground.blending, ParticleType.Sand);
        lightCampFire.Render(ref foreGround);
        shadowRendering.Render(ref foreGround, tickBlock.tick);
        GridRenderer.ApplyParticleRenderToTexture(ref foreGround, ref lightCampFireFlame.nativeTexture, m_map, tickBlock, lightCampFireFlame.blending, ParticleType.Fire);
        fireRendering.Render(ref foreGround, tickBlock.tick);
        lightAstronaut.Render(ref foreGround);
    }



    [System.Serializable]
    public struct FireRendering : IRenderableAnimated
    {
        public int2 position;
        public Color32[] colors;
        public int[] radiusMin;
        public int[] radiusMax;
        public float speed;
        public BlendingMode blending;

        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                float sin = math.sin(tick * speed) * 0.5f + 0.5f;
                int radius = (int)math.lerp(radiusMin[i], radiusMax[i], sin);
                GridRenderer.RenderCircle(ref colorArray, position, radius, colors[i], blending);
            }
        }
    }

    [System.Serializable]
    public struct ShadowRendering : IRenderableAnimated
    {
        public int2 position;
        public Color32[] colors;
        public int2[] radiusMin;
        public int2[] radiusMax;
        public float speed;
        public BlendingMode blending;

        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                float sin = math.sin(tick * speed) * 0.5f + 0.5f;
                int2 radius = (int2)math.lerp(radiusMin[i], radiusMax[i], sin);
                GridRenderer.RenderEllipseMask(ref colorArray, position, radius, colors[i], blending);
            }
        }
    }

    [System.Serializable]
    public struct StarBackgroundRendering : IRenderableAnimated
    {
        public int2 density;
        public int radius;
        public uint seed;
        public float speed;
        public float2 sinOffsetScale;
        public float sinOffsetAmplitude;


        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            new ShiningStarBackgroundJob()
            {
                colors = colorArray,
                maxSizes = GameManager.GridSizes,
                settings = this,
                tick = tick
            }.Schedule(GameManager.GridLength, 100).Complete();
        }
    }
 
}
