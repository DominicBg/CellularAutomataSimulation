﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public struct PixelSprite : IDisposable
{
    public NativeGrid<Color32> pixels;
    public int2 sizes;

    //TO REMOVE
    public int2 position;

    public PixelSprite(int2 position, Texture2D baseTexture)
    {
        Color32[] colors = baseTexture.GetPixels32(0);

        sizes = new int2(baseTexture.width, baseTexture.height);
        pixels = new NativeGrid<Color32>(sizes, Allocator.Persistent);

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                pixels[x, y] = colors[y * sizes.x + x];
            }
        }
        this.position = position;
    }

    public void Dispose()
    {
        pixels.Dispose();
    }
}
