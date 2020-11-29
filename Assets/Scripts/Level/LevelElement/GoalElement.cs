using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class GoalElement : LevelObject
{
    public SpriteEnum spriteEnum;

    public override Bound GetBound()
    {
        return new Bound(position, GetNativeSprite().sizes);
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplySprite(ref outputcolor, GetNativeSprite(), position);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    NativeSprite GetNativeSprite()
    {
        return SpriteRegistry.GetSprite(spriteEnum);
    }
}
