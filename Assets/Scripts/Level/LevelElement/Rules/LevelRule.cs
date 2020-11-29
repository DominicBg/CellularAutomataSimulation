using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class LevelRule : LevelElement
{
    public PlayerElement playerElement;
    public GoalElement goalElement;

    protected bool playerFinished;
    protected int tickFinished;


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(playerFinished)
        {
            tickFinished++;
        }

        Bound playerBound = playerElement.GetBound();
        Bound goalBound = goalElement.GetBound();
        if (!playerFinished && playerBound.IntersectWith(goalBound))
        {
            Debug.Log("BRAVO");
            playerFinished = true;
            OnLevelFinish();
        }
    }

    public abstract void OnLevelFinish();
}
