using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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
        Color32[] reflectiveColors = reflectiveTexture != null ? reflectiveTexture.GetPixels32(0) : null;

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
    public void GetRotationSprite(in RotationBound bound, PixelCamera.PixelCameraHandle cameraHandle, out NativeGrid<Color32> outputPixels, out NativeGrid<float3> outputNormals, out NativeGrid<float> outputReflections, out int2 min, out int2 max)
    {
        bound.GetCornerMinMax(out min, out max);
        int2 sizes = max - min;
        outputPixels = new NativeGrid<Color32>(sizes, Allocator.TempJob);
        outputNormals = new NativeGrid<float3>(sizes, Allocator.TempJob);
        outputReflections = new NativeGrid<float>(sizes, Allocator.TempJob);

        int2 renderPos = cameraHandle.GetRenderPosition(min);
        int threadLength = sizes.x * sizes.y;

        //precompute sincos for everythread
        bound.GetSinCosAngle(out float sin, out float cos);
        new RenderRotationBoundSpriteReflectionPass1Job()
        {
            pixels = pixels,
            normals = normals,
            reflections = reflections,

            outputPixels = outputPixels,
            outputNormals = outputNormals,
            outputReflections = outputReflections,
            
            cameraHandle = cameraHandle,
            rotationBound = bound,
            sin = sin,
            cos = cos,
            renderPos = renderPos
        }.Schedule(threadLength, 16).Complete();

        NativeGrid<Color32> tempPixels = NativeGrid<Color32>.FromGrid(outputPixels, Allocator.TempJob);
        NativeGrid<float3> tempNormal = NativeGrid<float3>.FromGrid(outputNormals, Allocator.TempJob);
        NativeGrid<float> tempReflections = NativeGrid<float>.FromGrid(outputReflections, Allocator.TempJob);

        ////two passes for better results
        new RenderRotationBoundSpriteReflectionPass2Job()
        {
            inputPixels = outputPixels,
            inputNormals = outputNormals,
            inputReflections = outputReflections,

            outputPixels = tempPixels,
            outputNormals = tempNormal,
            outputReflections = tempReflections
        }.Schedule(threadLength, 16).Complete();

        new RenderRotationBoundSpriteReflectionPass2Job()
        {
            inputPixels = tempPixels,
            inputNormals = tempNormal,
            inputReflections = tempReflections,

            outputPixels = outputPixels,
            outputNormals = outputNormals,
            outputReflections = outputReflections
        }.Schedule(threadLength, 16).Complete();

        tempPixels.Dispose();
        tempNormal.Dispose();
        tempReflections.Dispose();
    }



    public void Dispose()
    {
        pixels.Dispose();
        normals.Dispose();
        reflections.Dispose();
    }
}
