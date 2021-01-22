using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public interface IRenderable
{
    void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
    void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
    void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock); 
    void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);
    void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos);
}
