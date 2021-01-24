using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    public PlayerElement player;
    public PixelCamera pixelCamera;
    public PixelScene pixelScene;
    public PixelSceneData pixelSceneData;

    LevelObject[] levelObjects;
    IAlwaysRenderable[] alwaysRenderable;
    ILightSource[] lightSources;

    public bool inDebug = false;

    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;

    TickBlock tickBlock;
    TickBlock worldTickBlock;
    TickBlock postProcessTickBlock;

    public bool updateWorldElement = true;
    public bool updatLevelElement = true;

    public void LoadLevel()
    {

        tickBlock.Init();
        worldTickBlock.Init();
        postProcessTickBlock.Init();

        levelObjects = GetComponentsInChildren<LevelObject>();
        alwaysRenderable = GetComponentsInChildren<IAlwaysRenderable>();
        lightSources = GetComponentsInChildren<ILightSource>();

        pixelScene.Init(pixelSceneData.LoadMap());
        pixelCamera = new PixelCamera(pixelScene.GetComponentInChildren<PixelCameraTransform>(),GameManager.GridSizes);

        PostProcessManager.Instance = new PostProcessManager();
    }

    public void OnUpdate()
    {
        if (InputCommand.IsButtonDown(KeyCode.F1))
            inDebug = !inDebug;

        if (updatLevelElement)
            tickBlock.UpdateTick();

        if (updateWorldElement)
            worldTickBlock.UpdateTick();

        postProcessTickBlock.UpdateTick();

        //pixelCameraPos = math.lerp(pixelCameraPos, new float2(player.GetBound().center), GameManager.DeltaTime * cameraSmooth);
        pixelScene.OnUpdate(ref tickBlock, pixelCamera.position);

        PostProcessManager.Instance.Update(ref postProcessTickBlock);
    }

    public NativeArray<Color32> GetPixelCameraRender()
    {
        PixelCamera.RenderData renderData = new PixelCamera.RenderData
        {
            alwaysRenderables = alwaysRenderable,
            levelObjects = levelObjects,
            lightSources =lightSources,
            map = pixelScene.map
        };
        return pixelCamera.Render(renderData, ref tickBlock, inDebug);
    }


    public void OnRender()
    {
        var pixels = GetPixelCameraRender();
        GridRenderer.RenderToScreen(pixels);
  
        //GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);


        //LevelContainerGroup levelContainerGroup = CurrentLevelGroup;
        //if (transitionInfo.isInTransition)
        //{
        //    RenderTransition(ref outputColors);
        //}
        //else
        //{
        //    GridRenderer.GetBlankTexture(out NativeArray<Color32> backgroundColors);
        //    levelContainerGroup.RenderBackground(ref backgroundColors, ref tickBlock, currentLevelPosition);
        //    RenderLevelContainer(levels[currentLevelPosition], ref outputColors);

        //    //to be consistent with transition
        //    GridRenderer.ApplyTextureBehind(ref outputColors, ref backgroundColors, BlendingMode.Normal);

        //    backgroundColors.Dispose();
        //    levelContainerGroup.RenderForeground(ref outputColors, ref tickBlock, currentLevelPosition);
        //}
        //PostProcessManager.Instance.Render(ref outputColors, ref postProcessTickBlock);
        //GridRenderer.RenderToScreen(outputColors);
    }

    //public void RenderLevelContainer(LevelContainer levelContainer, ref NativeArray<Color32> outputColors)
    //{
    //    //add world object is visible
    //    levelContainer.PreRender(ref outputColors, ref tickBlock);
    //    worldLevelContainer.PreRender(ref outputColors, ref worldTickBlock);
     
    //    levelContainer.Render(ref outputColors, ref tickBlock);
    //    worldLevelContainer.Render(ref outputColors, ref worldTickBlock);


    //    LightRenderer.AddLight(ref outputColors, ref levelContainer.lightSources, levelContainer.GetGlobalOffset(), GridRenderer.Instance.lightRendering.settings);


    //    levelContainer.PostRender(ref outputColors, ref tickBlock);
    //    worldLevelContainer.PostRender(ref outputColors, ref worldTickBlock);
      
    //    levelContainer.RenderUI(ref outputColors, ref tickBlock);
    //    worldLevelContainer.RenderUI(ref outputColors, ref worldTickBlock);
    
    //    if (inDebug)
    //    {
    //        levelContainer.RenderDebug(ref outputColors, ref tickBlock);
    //        worldLevelContainer.RenderDebug(ref outputColors, ref worldTickBlock);
    //    }
    //}

    //void RenderTransition(ref NativeArray<Color32> outputColors)
    //{
    //    int2 transitionPosition = transitionInfo.entrance.levelContainer.levelPosition;

    //    GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
    //    GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

    //    LevelContainerGroup currentGroup = levelsGroups[levels[currentLevelPosition]];
    //    LevelContainerGroup transitionGroup = levelsGroups[levels[transitionPosition]];

    //    bool sameGroup = currentGroup == transitionGroup;

    //    if(sameGroup)
    //    {
    //        float2 lerpLevelPosition = math.lerp(currentLevelPosition, transitionPosition, transitionInfo.transitionRatio);
    //        GridRenderer.GetBlankTexture(out NativeArray<Color32> backgroundColors);
    //        currentGroup.RenderBackground(ref backgroundColors, ref tickBlock, lerpLevelPosition);

    //        RenderLevelContainer(levels[currentLevelPosition], ref currentColors);
    //        RenderLevelContainer(levels[transitionPosition], ref transitionColors);

    //        transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);

    //        GridRenderer.ApplyTextureBehind(ref outputColors, ref backgroundColors, BlendingMode.Normal);

    //        currentGroup.RenderForeground(ref outputColors, ref tickBlock, lerpLevelPosition);

    //        backgroundColors.Dispose();
    //    }
    //    else
    //    {
    //        currentGroup.RenderBackground(ref currentColors, ref tickBlock, currentLevelPosition);
    //        transitionGroup.RenderBackground(ref transitionColors, ref tickBlock, transitionPosition);
    //        RenderLevelContainer(levels[currentLevelPosition], ref currentColors);
    //        RenderLevelContainer(levels[transitionPosition], ref transitionColors);
    //        currentGroup.RenderForeground(ref currentColors, ref tickBlock, currentLevelPosition);
    //        transitionGroup.RenderForeground(ref transitionColors, ref tickBlock, transitionPosition);

    //        transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);
    //    }

    //    currentColors.Dispose();
    //    transitionColors.Dispose();    
    //}

    public void StartTransition(LevelEntrance entrance, TransitionBase transition)
    {
        transitionInfo = new TransitionInfo()
        {
            isInTransition = true,
            entrance = entrance,
            transition = transition,
            transitionRatio = 0
        };
    }

    public void Dispose()
    {
        pixelScene.Dispose();
        Destroy(gameObject);
    }

    public struct TransitionInfo
    {
        public float transitionRatio;
        public bool isInTransition;
        public LevelEntrance entrance;
        public TransitionBase transition;
    }
}
