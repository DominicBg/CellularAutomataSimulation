using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public struct PixelSprite
{
    public Color32[,] pixels { get; private set; }
    public bool[,] collisions { get; private set; }
    public int2 sizes { get; private set; }

    public int2 position;

    public PixelSprite(int2 position, Texture2D baseTexture)
    {
        Color32[] colors = baseTexture.GetPixels32(0);

        sizes = new int2(baseTexture.width, baseTexture.height);
        pixels = new Color32[sizes.x, sizes.y];
        collisions = new bool[sizes.x, sizes.y];

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                pixels[x, y] = colors[y * sizes.x + x];
                collisions[x, y] = pixels[x, y].a != 0;
            }
        }

        this.position = position;
    }
}
