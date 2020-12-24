using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    public LevelPosition[] levelContainerPrefabList = default;

    public Dictionary<int2, LevelContainer> levels;
    public int2 currentLevelPosition;

    public bool inDebug = false;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];
    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;
    //private float transitionRatio;
    //private int2 nextLevelContainerPosition;
    //public float transitionSpeed = 1;
    //private bool isInTransition;
    //private int nextEntranceID;

    public void LoadLevel()
    {
        levels = new Dictionary<int2, LevelContainer>();
        for (int i = 0; i < levelContainerPrefabList.Length; i++)
        {
            LevelContainer instance = Instantiate(levelContainerPrefabList[i].levelContainerPrefab);
            levels.Add(levelContainerPrefabList[i].position, instance);

            LevelContainerData data = instance.GetComponent<LevelContainerData>();
            instance.Init(data.LoadMap());

            //todo, delete
            //Delete data
        }
    }

    public void OnUpdate()
    {
        if (!transitionInfo.isInTransition)
        {
            levels[currentLevelPosition].OnUpdate();
        }
        else
        {
            transitionInfo.transitionRatio += GameManager.deltaTime * transitionSpeed;
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
            levels[currentLevelPosition].OnRender(ref outputColors, inDebug);
        }
        GridRenderer.RenderToScreen(outputColors);
    }

    void RenderTransition(ref NativeArray<Color32> outputColors)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

        levels[currentLevelPosition].OnRender(ref currentColors, inDebug);
        levels[transitionInfo.nextLevelContainerPosition].OnRender(ref transitionColors, inDebug);

        transitionInfo.transition.Transition(ref outputColors, ref currentColors, ref transitionColors, transitionInfo.transitionRatio);
        //bool isHorizontal = currentLevelPosition.y == nextLevelContainerPosition.y;
        //bool inverted = isHorizontal ? currentLevelPosition.x > nextLevelContainerPosition.x : currentLevelPosition.y > nextLevelContainerPosition.y;

        //float t = (inverted) ? 1 - transitionRatio : transitionRatio;

        //new SlideTransitionJob()
        //{
        //    firstImage = !inverted ? currentColors : transitionColors,
        //    secondImage = !inverted ? transitionColors : currentColors,
        //    outputColors = outputColors,
        //    isHorizontal = isHorizontal,
        //    t = t
        //}.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        currentColors.Dispose();
        transitionColors.Dispose();    
    }


    //todo add types?
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

    [System.Serializable]
    public class LevelPosition
    {
        public LevelContainer levelContainerPrefab;
        public int2 position;
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
