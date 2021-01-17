using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public interface ITextureRenderable
{
    void Render(ref NativeArray<Color32> colorArray);
}

public interface ITextureRenderableAnimated
{
    void Render(ref NativeArray<Color32> colorArray, int tick);
}
