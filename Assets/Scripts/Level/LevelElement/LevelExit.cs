﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelExit : LevelObject
{
    public int2 sizes;
    public int2 nextLevelContainerPosition;
    public int nextLevelEntrance;

    public TransitionBase transition;
    PlayerElement player;

    public override void Init(Map map)
    {
        base.Init(map);
        //nasto
        player = FindObjectOfType<PlayerElement>();
    }

    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(GetBound().IntersectWith(player.GetBound()))
        {

            //lol
            WorldLevel worldLevel = FindObjectOfType<WorldLevel>();
            worldLevel.StartTransition(nextLevelContainerPosition, nextLevelEntrance, transition);
            Debug.Log("next level");
        }
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        GridRenderer.DrawBound(ref outputColors, GetBound(), new Color32(255, 69, 0, 100));
    }
}
