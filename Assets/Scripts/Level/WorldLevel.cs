using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    // WorldObject[] worldObjects;

    public PlayerElement player;
    public float cameraSmooth;
    public float2 pixelCameraPos;
    PixelCamera pixelCamera;
    public PixelScene pixelScene;
    public PixelSceneData pixelSceneData;


    WorldLevelContainer worldLevelContainer;
    public Dictionary<int2, LevelContainer> levels;
    public Dictionary<LevelContainer, LevelContainerGroup> levelsGroups;
    public int2 currentLevelPosition;

    LevelObject[] levelObjects;

    public bool inDebug = false;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];
    public LevelContainerGroup CurrentLevelGroup => levelsGroups[CurrentLevel];

    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;

    TickBlock tickBlock;
    TickBlock worldTickBlock;
    TickBlock postProcessTickBlock;

    public bool updateWorldElement = true;
    public bool updatLevelElement = true;
    public bool usePixelCamera;

    public void LoadLevel()
    {
        pixelCamera = new PixelCamera(GameManager.GridSizes);

        tickBlock.Init();
        worldTickBlock.Init();
        postProcessTickBlock.Init();

        levelObjects = FindObjectsOfType<LevelObject>();

        Map map = pixelSceneData.LoadMap();
        pixelScene.Init(map);

        //worldObjects = GetComponentsInChildren<WorldObject>();


        //worldLevelContainer = GetComponentInChildren<WorldLevelContainer>();
        //LevelContainer[] levelContainers = GetComponentsInChildren<LevelContainer>();

        //levels = new Dictionary<int2, LevelContainer>();

        //for (int i = 0; i < levelContainers.Length; i++)
        //{
        //    levels.Add(levelContainers[i].levelPosition, levelContainers[i]);
        //    LevelContainerData data = levelContainers[i].GetComponent<LevelContainerData>();
        //    levelContainers[i].Init(data.LoadMap());
        //}

        ////Link container to groups
        //levelsGroups = new Dictionary<LevelContainer, LevelContainerGroup>();
        //LevelContainerGroup[] levelContainerGroups = GetComponentsInChildren<LevelContainerGroup>();
        //for (int i = 0; i < levelContainerGroups.Length; i++)
        //{
        //    var group = levelContainerGroups[i];
        //    for (int j = 0; j < group.levelContainers.Length; j++)
        //    {
        //        levelsGroups.Add(group.levelContainers[j], group);
        //    }
        //}

        //worldLevelContainer.Init(CurrentLevel.map, CurrentLevel);
        //worldLevelContainer.UpdateLevelMap(currentLevelPosition, CurrentLevel.map, CurrentLevel);

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

        pixelCameraPos = math.lerp(pixelCameraPos, player.GetBound().center, GameManager.DeltaTime * cameraSmooth);
        pixelScene.OnUpdate(ref tickBlock, (int2)pixelCameraPos);


        //if (!transitionInfo.isInTransition)
        //{
        //    if(updatLevelElement)
        //        levels[currentLevelPosition].OnUpdate(ref tickBlock);

        //    if(updateWorldElement)
        //        worldLevelContainer.OnUpdate(ref tickBlock);

        //    if (updatLevelElement)
        //        levels[currentLevelPosition].OnLateUpdate(ref tickBlock);

        //    if (updateWorldElement)
        //        worldLevelContainer.OnLateUpdate(ref tickBlock);

        //}
        //else
        //{
        //    transitionInfo.transitionRatio += GameManager.DeltaTime * transitionSpeed;
        //    if (transitionInfo.transitionRatio >= 1)
        //    {
        //        OnTransitionFinished();
        //    }
        //}

        PostProcessManager.Instance.Update(ref postProcessTickBlock);
    }

    void OnTransitionFinished()
    {
        transitionInfo.isInTransition = false;
        currentLevelPosition = transitionInfo.entrance.levelContainer.levelPosition;

        //eww
        PlayerElement player = FindObjectOfType<PlayerElement>();
        player.position = transitionInfo.entrance.position;
        player.physicData.position = player.position;
        player.physicData.velocity = 0;

        worldLevelContainer.UpdateLevelMap(currentLevelPosition, CurrentLevel.map, CurrentLevel);
    }


    public void OnRender()
    {
        if(usePixelCamera)
        {
            pixelCamera.Render((int2)pixelCameraPos, levelObjects, ref tickBlock, inDebug);
            return;
        }


        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);


        LevelContainerGroup levelContainerGroup = CurrentLevelGroup;
        if (transitionInfo.isInTransition)
        {
            RenderTransition(ref outputColors);
        }
        else
        {
            GridRenderer.GetBlankTexture(out NativeArray<Color32> backgroundColors);
            levelContainerGroup.RenderBackground(ref backgroundColors, ref tickBlock, currentLevelPosition);
            RenderLevelContainer(levels[currentLevelPosition], ref outputColors);

            //to be consistent with transition
            GridRenderer.ApplyTextureBehind(ref outputColors, ref backgroundColors, BlendingMode.Normal);

            backgroundColors.Dispose();
            levelContainerGroup.RenderForeground(ref outputColors, ref tickBlock, currentLevelPosition);
        }
        PostProcessManager.Instance.Render(ref outputColors, ref postProcessTickBlock);
        GridRenderer.RenderToScreen(outputColors);
    }

    public void RenderLevelContainer(LevelContainer levelContainer, ref NativeArray<Color32> outputColors)
    {
        //add world object is visible
        levelContainer.PreRender(ref outputColors, ref tickBlock);
        worldLevelContainer.PreRender(ref outputColors, ref worldTickBlock);
     
        levelContainer.Render(ref outputColors, ref tickBlock);
        worldLevelContainer.Render(ref outputColors, ref worldTickBlock);

        LightRenderer.AddLight(ref outputColors, ref levelContainer.lightSources, levelContainer.GetGlobalOffset(), GridRenderer.Instance.lightRendering.settings);


        levelContainer.PostRender(ref outputColors, ref tickBlock);
        worldLevelContainer.PostRender(ref outputColors, ref worldTickBlock);
      
        levelContainer.RenderUI(ref outputColors, ref tickBlock);
        worldLevelContainer.RenderUI(ref outputColors, ref worldTickBlock);
    
        if (inDebug)
        {
            levelContainer.RenderDebug(ref outputColors, ref tickBlock);
            worldLevelContainer.RenderDebug(ref outputColors, ref worldTickBlock);
        }
    }

    void RenderTransition(ref NativeArray<Color32> outputColors)
    {
        int2 transitionPosition = transitionInfo.entrance.levelContainer.levelPosition;

        GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

        LevelContainerGroup currentGroup = levelsGroups[levels[currentLevelPosition]];
        LevelContainerGroup transitionGroup = levelsGroups[levels[transitionPosition]];

        bool sameGroup = currentGroup == transitionGroup;

        if(sameGroup)
        {
            float2 lerpLevelPosition = math.lerp(currentLevelPosition, transitionPosition, transitionInfo.transitionRatio);
            GridRenderer.GetBlankTexture(out NativeArray<Color32> backgroundColors);
            currentGroup.RenderBackground(ref backgroundColors, ref tickBlock, lerpLevelPosition);

            RenderLevelContainer(levels[currentLevelPosition], ref currentColors);
            RenderLevelContainer(levels[transitionPosition], ref transitionColors);

            transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);

            GridRenderer.ApplyTextureBehind(ref outputColors, ref backgroundColors, BlendingMode.Normal);

            currentGroup.RenderForeground(ref outputColors, ref tickBlock, lerpLevelPosition);

            backgroundColors.Dispose();
        }
        else
        {
            currentGroup.RenderBackground(ref currentColors, ref tickBlock, currentLevelPosition);
            transitionGroup.RenderBackground(ref transitionColors, ref tickBlock, transitionPosition);
            RenderLevelContainer(levels[currentLevelPosition], ref currentColors);
            RenderLevelContainer(levels[transitionPosition], ref transitionColors);
            currentGroup.RenderForeground(ref currentColors, ref tickBlock, currentLevelPosition);
            transitionGroup.RenderForeground(ref transitionColors, ref tickBlock, transitionPosition);

            transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);
        }

        currentColors.Dispose();
        transitionColors.Dispose();    
    }

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
        worldLevelContainer?.Dispose();

        if (levels != null)
        { 
            foreach (var level in levels.Values)
            {
                level.Dispose();
                Destroy(level.gameObject);
            }
           }
        levels?.Clear();
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
