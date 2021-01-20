using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelMapParticleRenderer : LevelElement, IAlwaysRenderable
{
    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        return;
        NativeArray<LightSource> sources = new NativeArray<LightSource>(0, Allocator.TempJob);
        GridRenderer.ApplyMapPixels(ref outputColor, map, ref tickBlock, renderPos, sources);
        sources.Dispose();
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }
}
