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
    Map m_map;

    public void OnEnd()
    {
        for (int i = 0; i < m_pixelSprites.Length; i++)
        {
            m_pixelSprites[i].Dispose();
        }
        m_map.Dispose();
    }

    public void OnStart()
    {
        m_currentOverworld = overworlds[currentOverworld];    

        Level[] levels = m_currentOverworld.levels;
        m_pixelSprites = new PixelSprite[levels.Length];

        for (int i = 0; i < m_pixelSprites.Length; i++)
        {
            m_pixelSprites[i] = new PixelSprite(levels[i].position, levels[i].icon);
        }

        m_map = new Map(GameManager.GridSizes);

        m_currentOverworld.GetBackgroundColors(out NativeArray<Color32> pixels);
        GridRenderer.ApplyPixelSprites(ref pixels, m_pixelSprites);
        GridRenderer.RenderToScreen(pixels);
    }

    public void OnUpdate()
    {

    }
}
