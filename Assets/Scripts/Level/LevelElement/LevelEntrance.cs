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

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        GridRenderer.DrawBound(ref outputColors, GetBound(), new Color32(0, 0, 255, 100));
    }

    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }
}
