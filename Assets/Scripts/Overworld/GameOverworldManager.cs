using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using static OverworldBase;
using Unity.Collections;
using Unity.Mathematics;

public class GameOverworldManager : MonoBehaviour, State
{
    public int currentOverworld = 0;
    public OverworldBase[] overworlds;

    UINavigationGraph navigationGraph;
    OverworldBase m_currentOverworld;
    // NativeSprite[] m_nativeSprites;
    //int2[] positions;

    TickBlock tickBlock;

    public void OnEnd()
    {
        Dispose();
    }


    public void OnStart()
    {
        tickBlock.Init();
        SetCurrentOverworld(currentOverworld);
    }

    void SetCurrentOverworld(int overworldIndex)
    {
        if(navigationGraph != null)
        {
            Destroy(navigationGraph.gameObject);
        }


        currentOverworld = overworldIndex;
        m_currentOverworld = overworlds[currentOverworld];
        navigationGraph = m_currentOverworld.LoadNavigationGraph();
        navigationGraph.Init(this, 0);
    }

    public void OnRender()
    {
        m_currentOverworld.GetBackgroundColors(out NativeArray<Color32> ouputColors, ref tickBlock);
        navigationGraph.OnRender(ref ouputColors);
        GridRenderer.RenderToScreen(ouputColors);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        navigationGraph.OnUpdate();
    }

    public void SelectLevel(int i)
    {
        GameManager.Instance.worldLevelPrefab = m_currentOverworld.levelPrefab;
        //GameManager.Instance.levelData = m_currentOverworld.levels[i];
        GameManager.Instance.SetLevel();
    }

    public void RotateLevel(int direction)
    {
        currentOverworld = (int)Mathf.Repeat(currentOverworld + direction, overworlds.Length);
        SetCurrentOverworld(currentOverworld);
    }

    void Dispose()
    {
        if (navigationGraph != null)
        {
            Destroy(navigationGraph.gameObject);
            navigationGraph = null;
        }
    }
}
