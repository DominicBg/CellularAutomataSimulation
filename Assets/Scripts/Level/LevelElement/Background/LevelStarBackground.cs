using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelStarBackground : LevelBackground
{
    [SerializeField] StarBackgroundRendering settings;
    [SerializeField] public float offset;

    LevelStarBackground()
    {
        settings = StarBackgroundRendering.Default();
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 levelPosition)
    {
        settings.Render(ref outputcolor, tickBlock.tick, levelPosition * offset);

    }
}
