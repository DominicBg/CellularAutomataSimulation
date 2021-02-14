using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public interface IRenderable
{
    int RenderingLayerOrder();
    bool IsVisible();

    void SkyBoxRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);
    void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);
    void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);

    void RenderReflection(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);
    void LateRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);
    void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info);

    void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);
    void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
}
