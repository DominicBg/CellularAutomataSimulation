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

    OverworldBase m_currentOverworld;
    NativeSprite[] m_nativeSprites;
    int2[] positions;

    TickBlock tickBlock;

    public void OnEnd()
    {
        Dispose();
    }


    public void OnStart()
    {
        tickBlock.Init();
        SetCurrentLevel(currentOverworld);
    }

    void SetCurrentLevel(int level)
    {
        currentOverworld = level;
        m_currentOverworld = overworlds[currentOverworld];

        Level[] levels = m_currentOverworld.levels;
        m_nativeSprites = new NativeSprite[levels.Length];
        positions = new int2[levels.Length];

        for (int i = 0; i < m_nativeSprites.Length; i++)
        {
            m_nativeSprites[i] = new NativeSprite(levels[i].icon);
            positions[i] = levels[i].position;
        }
    }

    public void OnRender()
    {
        m_currentOverworld.GetBackgroundColors(out NativeArray<Color32> pixels, ref tickBlock);

        for (int i = 0; i < m_nativeSprites.Length; i++)
        {
            GridRenderer.ApplySprite(ref pixels, m_nativeSprites[i], positions[i]);
        }

        GridRenderer.RenderToScreen(pixels);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        if (InputCommand.IsButtonDown(KeyCode.A))
        {
            RotateLevel(-1);
        }
        else if(InputCommand.IsButtonDown(KeyCode.D))
        {
            RotateLevel(1);
        }
        else if(InputCommand.IsButtonDown(KeyCode.Space))
        {
            SelectLevel();
        }
    }

    void SelectLevel()
    {
        GameManager.Instance.SetLevel();
    }

    void RotateLevel(int direction)
    {
        Dispose();
        currentOverworld = (int)Mathf.Repeat(currentOverworld + direction, overworlds.Length);
        SetCurrentLevel(currentOverworld);
    }

    void Dispose()
    {
        for (int i = 0; i < m_nativeSprites.Length; i++)
        {
            m_nativeSprites[i].Dispose();
        }

    }
}
