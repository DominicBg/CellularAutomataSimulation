using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class LevelRule : LevelElement
{
    public PlayerElement playerElement;
    public LevelObject goalElement;

    protected bool playerFinished;
    protected int tickFinished;

    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        if (playerFinished)
            return;

        Bound playerBound = playerElement.GetBound();
        Bound goalBound = goalElement.GetBound();
        if (!playerFinished && playerBound.IntersectWith(goalBound))
        {
            Debug.Log("BRAVO");
            playerFinished = true;
            tickFinished = tickBlock.tick;
            OnLevelFinish();
        }
    }

    public abstract void OnLevelFinish();
}
