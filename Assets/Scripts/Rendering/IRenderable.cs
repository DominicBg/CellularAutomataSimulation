using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public interface IRenderable
{
    int RenderingLayerOrder();
    bool IsVisible();

    void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
    void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
    void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);

    void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights);
    void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights);
    void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights);

    void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);
    void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
}
