using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelRule : LevelElement
{
    public PlayerElement playerElement;
    public GoalElement goalElement;

    bool playerFinished;

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        //render bravo
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        Bound playerBound = playerElement.GetBound();
        Bound goalBound = goalElement.GetBound();
        if (!playerFinished && playerBound.IntersectWith(goalBound))
        {
            Debug.Log("BRAVO");
            playerFinished = true;
        }
    }
}
