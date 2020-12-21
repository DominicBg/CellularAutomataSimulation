using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public static GameLevelManager Instance;

    LevelContainer currentLevelContainer;

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
        currentLevelContainer.OnUpdate();
    }

    public void OnRender()
    {
        var outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        currentLevelContainer.OnRender(ref outputColor);
        GridRenderer.RenderToScreen(outputColor);
    }
}
