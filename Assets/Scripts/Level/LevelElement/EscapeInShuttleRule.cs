using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EscapeInShuttleRule : LevelRule
{
    public int timeFlightShuttle = 60;

    public override void OnLevelFinish()
    {
        playerElement.isEnable = false;
        playerElement.isVisible = false;
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        base.OnUpdate(ref tickBlock);

        if(playerFinished)
        {
            if(tickFinished > timeFlightShuttle)
            {
                GameManager.Instance.SetOverworld();
            }
            else
            {
                goalElement.position += new int2(0, 1);
                //sin wave animation of cute
            }
        }
    }
}
