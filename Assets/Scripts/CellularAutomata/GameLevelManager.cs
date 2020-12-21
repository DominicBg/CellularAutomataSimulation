using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public static GameLevelManager Instance;

    public float transitionSpeed = 1;
    LevelContainer currentLevelContainer;


    LevelContainer transitionLevelContainer;
    public bool inTransition;
    public float transitionRatio;

    //test
    public LevelDataScriptable testLevelData;


    public void OnStart()
    {
        Instance = this;
        LoadLevel(GameManager.Instance.levelData);
    }

    public void OnEnd()
    {
        Dispose();
    }

    void Dispose()
    {
        if (currentLevelContainer != null)
        {
            currentLevelContainer.Dispose();
            currentLevelContainer = null;
        }
    }

    public void LoadLevel(LevelDataScriptable levelData)
    {
        Dispose();

        currentLevelContainer = levelData.LoadLevelContainer();
        currentLevelContainer.Init(levelData.LoadMap());
    }
    
    public void OnUpdate()
    {
        if(!inTransition)
        {
            currentLevelContainer.OnUpdate();
        }
        else
        {
            transitionRatio += GameManager.deltaTime * transitionSpeed;
            if(transitionRatio >= 1)
            {
                inTransition = false;

                currentLevelContainer.Dispose();
                currentLevelContainer = transitionLevelContainer;
                transitionLevelContainer = null;

            }
        }
   
    }

    public void OnRender()
    {
        if(inTransition)
        {
            GridRenderer.GetBlankTexture(out NativeArray<Color32> currentColors);
            GridRenderer.GetBlankTexture(out NativeArray<Color32> transitionColors);

            currentLevelContainer.OnRender(ref transitionColors);
            transitionLevelContainer.OnRender(ref currentColors);

            GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);
            new ImageTransitionJob()
            {
                firstImage = currentColors,
                secondImage = transitionColors,
                outputColors = outputColors,
                isHorizontal = true,
                t = transitionRatio
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

            GridRenderer.RenderToScreen(outputColors);
        }
        else
        {
            GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);
            currentLevelContainer.OnRender(ref outputColors);
            GridRenderer.RenderToScreen(outputColors);
        }

    }

    public void SetTransition(bool isHorizontal, bool inverted, LevelDataScriptable levelData)
    {
        transitionRatio = 0;
        inTransition = true;

        transitionLevelContainer = levelData.LoadLevelContainer();
        transitionLevelContainer.Init(levelData.LoadMap());
    }

    [ContextMenu("Test Transition")]
    public void TestTransition()
    {
        SetTransition(true, true, testLevelData);
    }
}
