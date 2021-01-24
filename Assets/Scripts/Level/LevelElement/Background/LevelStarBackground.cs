using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelStarBackground : LevelElement, IAlwaysRenderable
{
    [SerializeField] StarBackgroundRendering settings;
    [SerializeField] public float offset;

    LevelStarBackground()
    {
        settings = StarBackgroundRendering.Default();
    }

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        settings.Render(ref outputColors, tickBlock.tick, (int2)((float2)renderPos * offset));
    }
}
