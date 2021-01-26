using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MapRenderElement : LevelElement, IAlwaysRenderable
{
    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        GridRenderer.ApplyMapPixels(ref outputColors, map, ref tickBlock, renderPos, lights);
    }
}
