using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public WorldLevel currentWorldLevel;

    public void OnStart()
    {
        LoadLevel(GameManager.Instance.worldLevel);
    }

    public void OnEnd()
    {
        Dispose();
    }

    void Dispose()
    {
        if(currentWorldLevel != null)
        {
            currentWorldLevel.Dispose();
            currentWorldLevel = null;
        }
    }

    public void LoadLevel(WorldLevel worldLevel)
    {
        Dispose();
        worldLevel.LoadLevel();
        currentWorldLevel = worldLevel;
    }

    public void OnUpdate()
    {
        currentWorldLevel.OnUpdate();
    }

    public void OnRender()
    {
        currentWorldLevel.OnRender();
    }
}
