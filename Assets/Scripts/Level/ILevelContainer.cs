using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public interface ILevelContainer
{
    void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);

    void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);

    void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);

    void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);

    void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock);

    void OnUpdate(ref TickBlock tickBlock);

    void OnLateUpdate(ref TickBlock tickBlock);

    void Dispose();
}
