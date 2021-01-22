using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelEntrance : LevelObject
{
    public int2 sizes;
    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors,new Bound(renderPos, sizes), new Color32(100, 100, 255, 200));
    }

    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }
}
