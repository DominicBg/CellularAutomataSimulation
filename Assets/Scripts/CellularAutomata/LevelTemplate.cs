using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelTemplate : LevelBase
{
    public enum LevelPhase { into, gameplay, ending}
    LevelPhase m_levelPhase;
    int tickAtPhase;

    protected int2 playerPosition = 0;
    protected int2 shuttlePosition = 25;


    public override void Update()
    {
        tickBlock.UpdateTick();
        tickAtPhase++;
        gameLevelManager.UpdateSimulation();

        if (m_levelPhase == LevelPhase.gameplay)
        {
            player.OnUpdate(map);
        }

        if (m_levelPhase == LevelPhase.gameplay && HasPlayerReachedShuttle())
        {
            Debug.Log("BRAVO");
            map.RemoveSpriteAtPosition(player.position, ref player.physicBound);
            m_levelPhase = LevelPhase.ending;
            tickAtPhase = 0;
        }
        else if (m_levelPhase == LevelPhase.ending)
        {
            shuttlePosition += new int2(0, 1);
            if (tickAtPhase > 60)
            {
                GameManager.Instance.SetOverworld();
            }
        }
    }


    protected bool HasPlayerReachedShuttle()
    {
        Bound playerBound = SpriteRegistry.GetSprite(SpriteEnum.astronaut).GetBound(playerPosition);
        Bound shuttleBound = SpriteRegistry.GetSprite(SpriteEnum.shuttle).GetBound(shuttlePosition);
        return playerBound.IntersectWith(shuttleBound);
    }

    public override void OnRender()
    {
        var outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);

        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);

        //update every object in a loop
        if (m_levelPhase == LevelPhase.gameplay)
            GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.astronaut), player.position);

        GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.shuttle), shuttlePosition);
        GridRenderer.RenderToScreen(outputColor);
    }
}
