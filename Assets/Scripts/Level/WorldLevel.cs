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

    public LevelContainer CurrentLevel => levels[currentLevelPosition];

    private float transitionRatio;
    private int2 nextLevelContainerPosition;
    public float transitionSpeed = 1;
    private bool isInTransition;
    private int nextEntranceID;

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
        if (!isInTransition)
        {
            levels[currentLevelPosition].OnUpdate();
        }
        else
        {
            transitionRatio += GameManager.deltaTime * transitionSpeed;
            if (transitionRatio >= 1)
            {
                isInTransition = false;
                currentLevelPosition = nextLevelContainerPosition;
                LevelEntrance[] levelEntrances = CurrentLevel.entrances;
                for (int i = 0; i < levelEntrances.Length; i++)
                {
                    if(nextEntranceID == levelEntrances[i].id)
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
        if(isInTransition)
        {
            RenderTransition(ref outputColors);
        }
        else
        { 
            levels[currentLevelPosition].OnRender(ref outputColors);
        }
        GridRenderer.RenderToScreen(outputColors);
    }

    void RenderTransition(ref NativeArray<Color32> outputColors)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

        levels[currentLevelPosition].OnRender(ref currentColors);
        levels[nextLevelContainerPosition].OnRender(ref transitionColors);

        bool isHorizontal = currentLevelPosition.y == nextLevelContainerPosition.y;
        bool inverted = isHorizontal ? currentLevelPosition.x > nextLevelContainerPosition.x : currentLevelPosition.y > nextLevelContainerPosition.y;

        float t = (inverted) ? 1 - transitionRatio : transitionRatio;

        new ImageTransitionJob()
        {
            firstImage = !inverted ? currentColors : transitionColors,
            secondImage = !inverted ? transitionColors : currentColors,
            outputColors = outputColors,
            isHorizontal = isHorizontal,
            t = t
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        currentColors.Dispose();
        transitionColors.Dispose();    
    }


    //todo add types?
    public void SetNextLevelContainer(int2 nextPosition, int nextEntranceID)
    {
        isInTransition = true;
        transitionRatio = 0;
        this.nextLevelContainerPosition = nextPosition;
        this.nextEntranceID = nextEntranceID;
    }

    public void Dispose()
    {
        foreach(var level in levels.Values)
        {
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
}
