using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static PixelCamera;

public class MapRenderElement : LevelElement, IAlwaysRenderable
{
    bool debugView;
    public override void OnInit()
    {
        base.OnInit();
        CheatManager.AddCheat("Debug Map View", () => debugView = !debugView);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        GridRenderer.ApplyMapPixels(ref outputColors, map, ref tickBlock, renderPos, info.lightSources, debugView);
    }
}
