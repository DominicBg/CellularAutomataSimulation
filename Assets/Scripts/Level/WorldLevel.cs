using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    //public LevelContainer[] levelContainerPrefabList = default;

    WorldObject[] worldObjects;
    public Dictionary<int2, LevelContainer> levels;
    public Dictionary<LevelContainer, LevelContainerGroup> levelsGroups;
    public int2 currentLevelPosition;

    public bool inDebug = false;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];
    public LevelContainerGroup CurrentLevelGroup => levelsGroups[CurrentLevel];

    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;
    TickBlock tickBlock;

    public void LoadLevel()
    {
        tickBlock.Init();
        worldObjects = GetComponentsInChildren<WorldObject>();

        LevelContainer[] levelContainers = GetComponentsInChildren<LevelContainer>();

        levels = new Dictionary<int2, LevelContainer>();

        for (int i = 0; i < levelContainers.Length; i++)
        {
            levels.Add(levelContainers[i].levelPosition, levelContainers[i]);
            LevelContainerData data = levelContainers[i].GetComponent<LevelContainerData>();
            levelContainers[i].Init(data.LoadMap());
        }

        //Link container to groups
        levelsGroups = new Dictionary<LevelContainer, LevelContainerGroup>();
        LevelContainerGroup[] levelContainerGroups = GetComponentsInChildren<LevelContainerGroup>();
        for (int i = 0; i < levelContainerGroups.Length; i++)
        {
            var group = levelContainerGroups[i];
            for (int j = 0; j < group.levelContainers.Length; j++)
            {
                levelsGroups.Add(group.levelContainers[j], group);
            }
        }

        for (int i = 0; i < worldObjects.Length; i++)
        {

            worldObjects[i].Init(CurrentLevel.map, null);
        }
    }

    public void OnUpdate()
    {
        if (InputCommand.IsButtonDown(KeyCode.F1))
            inDebug = !inDebug;


        if (!transitionInfo.isInTransition)
        {
            levels[currentLevelPosition].OnUpdate(ref tickBlock);
            for (int i = 0; i < worldObjects.Length; i++)
                if(worldObjects[i].isEnable) //update according to current level, might put tickBlock in worldLevel
                    worldObjects[i].OnUpdate(ref tickBlock);
        }
        else
        {
            transitionInfo.transitionRatio += GameManager.DeltaTime * transitionSpeed;
            if (transitionInfo.transitionRatio >= 1)
            {
                OnTransitionFinished();
            }
        }
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
     

        for (int i = 0; i < worldObjects.Length; i++)
        {
            worldObjects[i].UpdateLevelMap(currentLevelPosition, CurrentLevel.map);
        }
    }


    public void OnRender()
    {
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
        PostProcessManager.Instance.Render(ref outputColors, ref tickBlock);
        GridRenderer.RenderToScreen(outputColors);
    }

    public void RenderLevelContainer(LevelContainer levelContainer, ref NativeArray<Color32> outputColors)
    {
        //add world object is visible
        levelContainer.PreRender(ref outputColors, ref tickBlock);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].PreRender(ref outputColors, ref tickBlock);

        levelContainer.Render(ref outputColors, ref tickBlock);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].Render(ref outputColors, ref tickBlock);

        levelContainer.PostRender(ref outputColors, ref tickBlock);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].PostRender(ref outputColors, ref tickBlock);

        levelContainer.RenderUI(ref outputColors, ref tickBlock);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].RenderUI(ref outputColors, ref tickBlock);

        if (inDebug)
        {
            levelContainer.RenderDebug(ref outputColors, ref tickBlock);
            for (int i = 0; i < worldObjects.Length; i++)
                if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                    worldObjects[i].RenderDebug(ref outputColors, ref tickBlock);
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
        for (int i = 0; i < worldObjects.Length; i++)
        {
            worldObjects[i].Dispose();
        }
        foreach (var level in levels.Values)
        {
            level.Dispose();
            Destroy(level.gameObject);
        }
        levels.Clear();
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
