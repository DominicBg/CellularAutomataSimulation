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

    public LevelContainer mainMenuLevelPrefab;
    LevelContainer mainMenuLevel;
    TickBlock tickBlock;

    //Debug
    public float t;
    public int diamondSize;

    public void OnEnd()
    {
        lightRender.Dispose();
        darkRender.Dispose();

        if(mainMenuLevel != null)
            mainMenuLevel.Dispose();

        mainMenuLevel = null;
    }

    public void OnStart()
    {
        lightRender.Init();
        darkRender.Init();

        //Load simulation
        mainMenuLevel = Instantiate(mainMenuLevelPrefab);
        LevelContainerData data = mainMenuLevel.GetComponent<LevelContainerData>();
        mainMenuLevel.Init(data.LoadMap());

        tickBlock.Init();
    }

    public void OnUpdate()
    {
        mainMenuLevel.OnUpdate(ref tickBlock);
    }

    public void OnRender()
    {
        float noiseValue = noise.cnoise((float2)(tickBlock.tick * randomSpeed));
        noiseValue = MathUtils.unorm(noiseValue);

        bool showlight = noiseValue < lightThreshold;
        bool showGlitch = noiseValue > lightThreshold && noiseValue < glitchThreshold;

        if (showGlitch)
        {
            var lightTexture = lightRender.Render(ref tickBlock, ref mainMenuLevel.map);
            var darkTexture = darkRender.Render(ref tickBlock);

            InterlaceTextureSettings glitchSettings = new InterlaceTextureSettings();
            glitchSettings.stride = tickBlock.random.NextInt2(minStride, maxStride);
            glitchSettings.inverted = tickBlock.tick % glitchSpeed * 2 < glitchSpeed;

            var result = GridRenderer.InterlaceColors(ref lightTexture, ref darkTexture, ref glitchSettings);
            GridRenderer.RenderToScreen(result);

        }
        else if (showlight)
        {
            var lightTexture = lightRender.Render(ref tickBlock, ref mainMenuLevel.map);
            GridRenderer.RenderToScreen(lightTexture);
        }
        else
        {
            var darkTexture = darkRender.Render(ref tickBlock);
            GridRenderer.RenderToScreen(darkTexture);
        }
    }

}
