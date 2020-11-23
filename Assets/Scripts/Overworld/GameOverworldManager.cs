using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using static OverworldBase;
using Unity.Collections;

public class GameOverworldManager : MonoBehaviour, State
{
    public int currentOverworld = 0;
    public OverworldBase[] overworlds;

    OverworldBase m_currentOverworld;
    PixelSprite[] m_pixelSprites;
    TickBlock tickBlock;

    public void OnEnd()
    {
        for (int i = 0; i < m_pixelSprites.Length; i++)
        {
            m_pixelSprites[i].Dispose();
        }
    }

 

    public void OnStart()
    {
        tickBlock.Init();
        m_currentOverworld = overworlds[currentOverworld];

        Level[] levels = m_currentOverworld.levels;
        m_pixelSprites = new PixelSprite[levels.Length];

        for (int i = 0; i < m_pixelSprites.Length; i++)
        {
            m_pixelSprites[i] = new PixelSprite(levels[i].position, levels[i].icon);
        }
    }

    public void OnRender()
    {
        m_currentOverworld.GetBackgroundColors(out NativeArray<Color32> pixels, ref tickBlock);
        GridRenderer.ApplySprites(ref pixels, m_pixelSprites);

        GridRenderer.RenderToScreen(pixels);
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
    }
}
