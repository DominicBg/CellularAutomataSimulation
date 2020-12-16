using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelStarBackground : LevelElement
{
    [SerializeField] StarBackgroundRendering settings;

    LevelStarBackground()
    {
        settings = StarBackgroundRendering.Default();
    }


    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        settings.Render(ref outputcolor, tickBlock.tick);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }
}
