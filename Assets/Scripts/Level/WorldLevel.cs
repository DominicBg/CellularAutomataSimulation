using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    //public LevelContainer[] levelContainerPrefabList = default;

    public Dictionary<int2, LevelContainer> levels;
    public int2 currentLevelPosition;

    public bool inDebug = false;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];
    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;

    public void LoadLevel()
    {
        LevelContainer[] levelContainers = GetComponentsInChildren<LevelContainer>();
        levels = new Dictionary<int2, LevelContainer>();
        for (int i = 0; i < levelContainers.Length; i++)
        {
            //LevelContainer instance = Instantiate(levelContainerPrefabList[i]);
            levels.Add(levelContainers[i].levelPosition, levelContainers[i]);

            LevelContainerData data = levelContainers[i].GetComponent<LevelContainerData>();
            levelContainers[i].Init(data.LoadMap());
        }
    }

    public void OnUpdate()
    {
        if (InputCommand.IsButtonDown(KeyCode.F1))
            inDebug = !inDebug;


        if (!transitionInfo.isInTransition)
        {
            levels[currentLevelPosition].OnUpdate();
        }
        else
        {
            transitionInfo.transitionRatio += GameManager.DeltaTime * transitionSpeed;
            if (transitionInfo.transitionRatio >= 1)
            {
                transitionInfo.isInTransition = false;
                currentLevelPosition = transitionInfo.nextLevelContainerPosition;
                LevelEntrance[] levelEntrances = CurrentLevel.entrances;
                for (int i = 0; i < levelEntrances.Length; i++)
                {
                    if(transitionInfo.nextEntranceID == levelEntrances[i].id)
                    {
                        //eww
                        PlayerElement player = CurrentLevel.GetComponentInChildren<PlayerElement>();
                        player.position = levelEntrances[i].position;
                        player.physicData.position = player.position;
                        player.physicData.velocity = 0;
                    }
                }
            }
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
            LevelContainer levelContainer = levels[currentLevelPosition];
            levelContainer.PreRender(ref outputColors);
            levelContainer.Render(ref outputColors);
            levelContainer.PostRender(ref outputColors);
            levelContainer.RenderUI(ref outputColors);
        }
        GridRenderer.RenderToScreen(outputColors);
    }

    public void RenderLevelContainer(LevelContainer levelContainer, ref NativeArray<Color32> outputColors)
    {
        levelContainer.PreRender(ref outputColors);
        levelContainer.Render(ref outputColors);
        levelContainer.PostRender(ref outputColors);
        levelContainer.RenderUI(ref outputColors);
        
        if(inDebug)
            levelContainer.RenderDebug(ref outputColors);
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
        foreach(var level in levels.Values)
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
