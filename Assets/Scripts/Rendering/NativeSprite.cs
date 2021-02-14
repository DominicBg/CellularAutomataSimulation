using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct NativeSprite : IDisposable
{
    public NativeGrid<Color32> pixels;
    public NativeGrid<float3> normals;
    public NativeGrid<float> reflections;
    public int2 sizes;

    public bool UseNormals { get; private set; }
    public bool UseReflection { get; private set; }

    public Bound GetBound(int2 position) => new Bound(position, sizes);

    public NativeSprite(Texture2D baseTexture, Texture2D normalTexture = null, Texture2D reflectiveTexture = null)
    {
        Color32[] colors = baseTexture.GetPixels32(0);
        Color32[] normalColors = normalTexture != null ? normalTexture.GetPixels32(0) : null;
        Color32[] reflectiveColors = normalTexture != null ? reflectiveTexture.GetPixels32(0) : null;

        sizes = new int2(baseTexture.width, baseTexture.height);
        pixels = new NativeGrid<Color32>(sizes, Allocator.Persistent);
        normals = new NativeGrid<float3>(sizes, Allocator.Persistent);
        reflections = new NativeGrid<float>(sizes, Allocator.Persistent);

        UseNormals = normalTexture != null;
        UseReflection = reflectiveTexture != null;

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                pixels[x, y] = colors[y * sizes.x + x];

                if (UseNormals)
                    normals[x, y] = ((Color)normalColors[y * sizes.x + x]).ToNormal();

                if (UseReflection)
                    reflections[x, y] = ((Color)reflectiveColors[y * sizes.x + x]).r;
            }
        }
    }

    public void Dispose()
    {
        pixels.Dispose();
        normals.Dispose();
        reflections.Dispose();
    }
}
