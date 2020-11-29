using System.Collections;
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

    [Header("Parameters")]
    public float randomSpeed = 1;
    public float lightThreshold = 0.8f;
    public float glitchThreshold = 0.5f;
    public int glitchSpeed;
    public int2 minStride;
    public int2 maxStride;
    public int2 particleDestroyerPosition;

    [Header("References")]
    public ParticleBehaviourScriptable partaicleBehaviour;
    public LevelContainer mainMenuLevel;

    Map m_map;
    TickBlock tickBlock;
    NativeArray<ParticleSpawner> particleSpawners;

    public void OnEnd()
    {
        lightRender.Dispose();
        darkRender.Dispose();

        m_map.Dispose();
        particleSpawners.Dispose();
    }

    public void OnStart()
    {
        tickBlock.Init();
        lightRender.Init();
        darkRender.Init();

        //Load simulation
        m_map = mainMenuLevel.LoadMap();
        particleSpawners = mainMenuLevel.GetParticleSpawner();
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
        float noiseValue = noise.cnoise((float2)(tickBlock.tick * randomSpeed));
        noiseValue = MathUtils.unorm(noiseValue);

        bool showlight = noiseValue < lightThreshold;
        bool showGlitch = noiseValue > lightThreshold && noiseValue < glitchThreshold;

        if(showGlitch)
        {
            var lightTexture = lightRender.Render(ref tickBlock, ref m_map);
            var darkTexture = darkRender.Render(ref tickBlock);

            InterlaceTextureSettings glitchSettings = new InterlaceTextureSettings();
            glitchSettings.stride = tickBlock.random.NextInt2(minStride, maxStride);
            glitchSettings.inverted = tickBlock.tick % glitchSpeed * 2 < glitchSpeed;

            var result = GridRenderer.InteraceColors(ref lightTexture, ref darkTexture, ref glitchSettings);
            GridRenderer.RenderToScreen(result);

        }
        else if (showlight)
        {
            var lightTexture =lightRender.Render(ref tickBlock, ref m_map);
            GridRenderer.RenderToScreen(lightTexture);
        }
        else
        {
            var darkTexture = darkRender.Render(ref tickBlock);
            GridRenderer.RenderToScreen(darkTexture);
        }
    }

}
