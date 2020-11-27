using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct LayerTextureSheet : IRenderableAnimated, IDisposable
{
    public Texture2D[] textures;
    public int tickPerTexture;
    public int2 position;
    public BlendingMode blending;

    public PixelSprite[] sprites;

    public void Dispose()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].Dispose();
        }
    }

    public void Init()
    {
        sprites = new PixelSprite[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            sprites[i] = new PixelSprite(position, textures[i]);
        }
    }

    public void Render(ref NativeArray<Color32> colorArray, int tick)
    {
        int i = (tick / tickPerTexture) % sprites.Length;
        sprites[i].position = position;

        GridRenderer.ApplySprite(ref colorArray, sprites[i], sprites[i].position);
    }
}
