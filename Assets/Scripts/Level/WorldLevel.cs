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
    public int2 currentLevelPosition;

    public bool inDebug = false;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];
    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;

    public void LoadLevel()
    {
        //add WorldElements
        worldObjects = GetComponentsInChildren<WorldObject>();

        LevelContainer[] levelContainers = GetComponentsInChildren<LevelContainer>();
        levels = new Dictionary<int2, LevelContainer>();
        for (int i = 0; i < levelContainers.Length; i++)
        {
            //LevelContainer instance = Instantiate(levelContainerPrefabList[i]);
            levels.Add(levelContainers[i].levelPosition, levelContainers[i]);

            LevelContainerData data = levelContainers[i].GetComponent<LevelContainerData>();
            levelContainers[i].Init(data.LoadMap());
        }

        for (int i = 0; i < worldObjects.Length; i++)
        {
            worldObjects[i].Init(CurrentLevel.map);
        }
    }

    public void OnUpdate()
    {
        if (InputCommand.IsButtonDown(KeyCode.F1))
            inDebug = !inDebug;


        if (!transitionInfo.isInTransition)
        {
            levels[currentLevelPosition].OnUpdate();
            for (int i = 0; i < worldObjects.Length; i++)
                //update according to current level, might put tickBlock in worldLevel
                worldObjects[i].OnUpdate(ref levels[worldObjects[i].currentLevel].tickBlock);
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
        currentLevelPosition = transitionInfo.nextLevelContainerPosition;

        //Bind entrance by refs
        LevelEntrance[] levelEntrances = CurrentLevel.entrances;
        for (int i = 0; i < levelEntrances.Length; i++)
        {
            if (transitionInfo.nextEntranceID == levelEntrances[i].id)
            {
                //eww
                PlayerElement player =FindObjectOfType<PlayerElement>();
                player.position = levelEntrances[i].position;
                player.physicData.position = player.position;
                player.physicData.velocity = 0;
            }
        }

        for (int i = 0; i < worldObjects.Length; i++)
        {
            worldObjects[i].UpdateLevelMap(currentLevelPosition, CurrentLevel.map);
        }
    }


    public void OnRender()
    {
        //TODO add global effect in rendering here?

        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);
        if(transitionInfo.isInTransition)
        {
            RenderTransition(ref outputColors);
        }
        else
        {
            RenderLevelContainer(levels[currentLevelPosition], ref outputColors);
        }

        PostProcessManager.Instance.Render(ref outputColors, ref levels[currentLevelPosition].tickBlock);
        GridRenderer.RenderToScreen(outputColors);
    }

    public void RenderLevelContainer(LevelContainer levelContainer, ref NativeArray<Color32> outputColors)
    {
        ref TickBlock currentTickBlock = ref levelContainer.tickBlock;

        //Nasty shit, maybe transfer worldObject in WorldContainer?
        levelContainer.PreRender(ref outputColors);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].PreRender(ref outputColors, ref currentTickBlock);

        levelContainer.Render(ref outputColors);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].Render(ref outputColors, ref currentTickBlock);

        levelContainer.PostRender(ref outputColors);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].PostRender(ref outputColors, ref currentTickBlock);

        levelContainer.RenderUI(ref outputColors);
        for (int i = 0; i < worldObjects.Length; i++)
            if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                worldObjects[i].RenderUI(ref outputColors, ref currentTickBlock);

        if (inDebug)
        {
            levelContainer.RenderDebug(ref outputColors);
            for (int i = 0; i < worldObjects.Length; i++)
                if (math.all(worldObjects[i].currentLevel == levelContainer.levelPosition))
                    worldObjects[i].RenderDebug(ref outputColors, ref currentTickBlock);
        }
    }

    void RenderTransition(ref NativeArray<Color32> outputColors)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

        RenderLevelContainer(levels[currentLevelPosition], ref currentColors);
        RenderLevelContainer(levels[transitionInfo.nextLevelContainerPosition], ref transitionColors);

        transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);

        currentColors.Dispose();
        transitionColors.Dispose();    
    }

    public void StartTransition(int2 nextPosition, int nextEntranceID, TransitionBase transition)
    {
        transitionInfo = new TransitionInfo()
        {
            isInTransition = true,
            nextEntranceID = nextEntranceID,
            nextLevelContainerPosition = nextPosition,
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
        public  float transitionRatio;
        public  int2 nextLevelContainerPosition;
        public  bool isInTransition;
        public  int nextEntranceID;
        public TransitionBase transition;
    }
}
