using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelMapParticleRenderer : LevelElement
{
    public override void OnRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {        
        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }
}
