using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct LayerTexture : IRenderable, IDisposable
{
    public Texture2D texture;
    public BlendingMode blending;

    public NativeArray<Color32> nativeTexture;

    public void Dispose()
    {
        nativeTexture.Dispose();
    }

    public void Init()
    {
        nativeTexture = RenderingUtils.GetNativeArray(texture, Allocator.Persistent);
    }

    public void Render(ref NativeArray<Color32> colorArray)
    {
        GridRenderer.ApplyTexture(ref colorArray, ref nativeTexture);
    }
}
