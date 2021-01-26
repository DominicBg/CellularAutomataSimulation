using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelExit : LevelObject
{
    public int2 sizes;
    public LevelEntrance entrance;
    public TransitionBase transition;


    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }


    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        if(GetBound().IntersectWith(player.GetBound()))
        {
            map.RemoveSpriteAtPosition(player.physicData.gridPosition, ref player.physicData.physicBound);
            //lol
            WorldLevel worldLevel = FindObjectOfType<WorldLevel>();
            worldLevel.StartTransition(entrance, transition, entrance.GetComponentInParent<PixelPartialScene>());
            Debug.Log("next level");
        }
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, new Bound(renderPos, sizes), new Color32(255, 169, 100, 200));
    }
}
