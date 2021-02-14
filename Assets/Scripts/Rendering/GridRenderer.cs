﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;
using static RenderingUtils;

public class GridRenderer : MonoBehaviour
{
    public static GridRenderer Instance { get; private set; }
    public ParticleRendering particleRendering;

    static ProfilerMarker S_SimulationRender = new ProfilerMarker("GridRenderer.SimulationRender");

    [SerializeField] RawImage m_renderer = default;
    [SerializeField] TextMeshProUGUI m_rendererText = default;
    [SerializeField] MeshRenderer m_cubeRenderer;
    public LightRenderingScriptable lightRendering;

    public static GridPostProcess postProcess;
    private static Texture2D m_texture;

    void Awake()
    {
        Instance = this;

        int2 sizes = GameManager.RenderSizes;
        m_texture = new Texture2D(sizes.x, sizes.y, TextureFormat.RGBA32, false, true);
        m_texture.filterMode = FilterMode.Point;
    }

    public static void GetBlankTexture(out NativeArray<Color32> outputColor)
    {
        outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
    }
    public static NativeArray<Color32> GetBlankTexture()
    {
        return new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
    }

    public static void GetBlankTexture(out NativeArray<Color32> outputColor, Color baseColor)
    {
        outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        for (int i = 0; i < outputColor.Length; i++)
        {
            outputColor[i] = baseColor;
        }
    }
    public static void GetGradientTexture(out NativeArray<Color32> outputColor, Color topColor, Color bottomColor, int resolution)
    {
        outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        for (int x = 0; x < GameManager.RenderSizes.x; x++)
        {
            for (int y = 0; y < GameManager.RenderSizes.y; y++)
            {
                int i = y * GameManager.RenderSizes.x + x;
                float t = MathUtils.ReduceResolution((float)y / GameManager.RenderSizes.y, resolution);
                outputColor[i] = Color.Lerp(topColor, bottomColor, t);
            }
        }
    }

    public static void ApplyMapPixels(ref NativeArray<Color32> outputColor, Map map, ref TickBlock tickBlock, float2 cameraPos, NativeArray<LightSource> lightSources)
    {
        using (S_SimulationRender.Auto())
        {
            new GridRendererJob(outputColor, map, Instance.particleRendering, tickBlock, cameraPos, lightSources).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        }
    }

    public static void ApplyParticleRenderToTexture(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> textureColor, Map map, TickBlock tickBlock, NativeArray<LightSource> lightSources, BlendingMode blending, ParticleType particleType)
    {
        //todo profile
        new ApplyParticleRenderToTextureJob(outputColor, textureColor, map, Instance.particleRendering, tickBlock, lightSources, blending, particleType).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public static void DrawEllipse(ref NativeArray<Color32> outputColor, int2 position, int2 radius, Color32 innerColor, Color32 outerColor, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        new CreateEllipseJob()
        {
            outputColor = outputColor,
            position = position,
            radius = radius,
            innerColor = innerColor,
            outerColor = outerColor,
            blending = blending,
            useAlphaMask = useAlphaMask
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public static void DrawBound(ref NativeArray<Color32> outputColor, Bound worldBound, int2 cameraPos, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        worldBound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] -= cameraPos - GameManager.RenderSizes / 2;
            colors[i] = color;
        }
        new ApplyPixelsJob(outputColor, positions, colors, blending).Run();
        positions.Dispose();
        colors.Dispose();
    }

    public static void DrawBound(ref NativeArray<Color32> outputColor, Bound bound, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        new ApplyPixelsJob(outputColor, positions, colors, blending).Run();
        positions.Dispose();
        colors.Dispose();
    }

    public static void DrawRotationBound(ref NativeArray<Color32> outputColors, in RotationBound bound, PixelCamera pixelCamera, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        new RenderRotationBoundJob()
        {
            cameraHandle = pixelCamera.GetHandle(),
            color = color,
            outputColors = outputColors,
            rotationBound = bound,
            blending = blending
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
    public static void DrawRotationSprite(ref NativeArray<Color32> outputColors, in RotationBound bound, PixelCamera pixelCamera, in NativeSprite nativeSprite, Color tint, BlendingMode blending = BlendingMode.Normal)
    {
        new RenderRotationBoundSpriteJob()
        {
            cameraHandle = pixelCamera.GetHandle(),
            nativeSprite = nativeSprite,
            outputColors = outputColors,
            rotationBound = bound,
            tint = tint,
            blending = blending
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
    public static void DrawRotationSprite(ref NativeArray<Color32> outputColors, in RotationBound bound, PixelCamera pixelCamera, in NativeSprite nativeSprite, BlendingMode blending = BlendingMode.Normal)
    {
        new RenderRotationBoundSpriteJob()
        {
            cameraHandle = pixelCamera.GetHandle(),
            nativeSprite = nativeSprite,
            outputColors = outputColors,
            rotationBound = bound,
            tint = Color.white,
            blending = blending
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }


    public static void ApplyPixels(ref NativeArray<Color32> outputColor, ref NativeArray<int2> pixelPositions, ref NativeArray<Color32> pixelcolors, BlendingMode blending = BlendingMode.Normal)
    {
        new ApplyPixelsJob(outputColor, pixelPositions, pixelcolors, blending).Run();
    }

    public static void ApplyTexture(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> texture, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        new ApplyTextureJob(outputColor, texture, blending, useAlphaMask).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public static void ApplyTextureBehind(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> behindTexture, BlendingMode blending = BlendingMode.Normal)
    {
        new ApplyTextureBehindJob(outputColor, behindTexture, blending).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }


    /// <summary>
    /// Apply colorsB ontop colorsA
    /// </summary>
    public static NativeArray<Color32> CombineColors(ref NativeArray<Color32> colorsA, ref NativeArray<Color32> colorsB, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        new ApplyTextureJob(colorsA, colorsB, blending, useAlphaMask).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        colorsB.Dispose();
        return colorsA;
    }

    public static NativeArray<Color32> InterlaceColors(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> colors, ref InterlaceTextureSettings settings)
    {
        new InterlaceTextureJob()
        {
            settings = settings,
            colors = colors,
            outputColor = outputColor,
            mapSizes = GameManager.RenderSizes
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        colors.Dispose();
        return outputColor;
    }

    [Obsolete("Use the function with NativeGrid<Color32>")]
    public static void ApplySprite(ref NativeArray<Color32> outputColor, in NativeSprite sprite, int2 position, bool centerAligned = false)
    {
        ApplySprite(ref outputColor, in sprite, position, centerAligned, false);
    }

    public static void ApplySprite(ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, int2 renderPos, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 texturePos = renderPos + new int2(x, y) - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(texturePos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.RenderSizes.x);
                    outputColor[index] = colors[x, y];
                }
            }
        }
    }

    public static void ApplyLitSprite(ref NativeArray<Color32> outputColors, in NativeGrid<Color32> colors, in NativeGrid<float3> normals,
        int2 worldPosition, int2 renderPos, NativeList<LightSource> lights, float minLight = 0.5f, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 centerAligendOffset = -(centerAligned ? colors.Sizes / 2 : 0);
                int2 localPos = new int2(x, y);
                int2 finalPos = renderPos + localPos + centerAligendOffset;
                if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes);
                    outputColors[index] = ApplyLightOnPixel(worldPosition + localPos + centerAligendOffset, colors[localPos], normals[localPos], lights, 0, minLight, 25);
                }
            }
        }
    }

    public static void ApplySpriteSkyboxReflection(ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, in NativeGrid<float3> normals, in NativeGrid<float> reflectiveMap,
    int2 renderPos, EnvironementInfo info, ReflectionInfo reflectionInfo, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 localPos = new int2(x, y);
                int2 finalPos = renderPos + localPos - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes);
                    outputColor[index] = ApplySkyboxReflection(renderPos, outputColor[index], reflectiveMap[localPos], normals[localPos], info, reflectionInfo);
                }
            }
        }
    }


    public static void ApplySpriteEnvironementReflection(ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, in NativeGrid<float3> normals, in NativeGrid<float> reflectiveMap,
        int2 renderPos, ReflectionInfo reflectionInfo, int blurRadius, float blurIntensity, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 localPos = new int2(x, y);
                int2 finalPos = renderPos + localPos - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes);

                    int2 direction = (int2)(normals[localPos].xy * reflectionInfo.distance);
                    int2 samplePos = finalPos + direction;
                    if (!GridHelper.InBound(samplePos, GameManager.RenderSizes))
                        continue;

                    Color sampleColor = outputColor[ArrayHelper.PosToIndex(samplePos, GameManager.RenderSizes)];
                    if (blurRadius <= 1)
                    {   
                        //Take sample directly
                        outputColor[index] = Blend(outputColor[index], sampleColor.Alpha(reflectiveMap[localPos] * reflectionInfo.amount), reflectionInfo.blending);
                        continue;
                    }

                    //Add Blur
                    Color color = Color.clear;
                    var circlePos = GridHelper.GetCircleAtPosition(samplePos, blurRadius, GameManager.RenderSizes, Allocator.Temp);
                    for (int i = 0; i < circlePos.Length; i++)
                    {
                        color += outputColor[ArrayHelper.PosToIndex(circlePos[i], GameManager.RenderSizes)];
                    }
                    color /= circlePos.Length;
                    color.a = blurIntensity;
                    circlePos.Dispose();
                    color = Blend(sampleColor, color, BlendingMode.Normal);
                    
                    outputColor[index] = Blend(outputColor[index], color.Alpha(reflectiveMap[localPos] * reflectionInfo.amount), reflectionInfo.blending);        
                }
            }
        }
    }


    [Obsolete("Use the function with NativeGrid<Color32>")]
    public static void ApplySprite(ref NativeArray<Color32> outputColor, in NativeSprite sprite, int2 position, bool2 isFlipped, bool centerAligned = false)
    {
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int xx = (!isFlipped.x) ? x : sprite.sizes.x - x - 1;
                int yy = (!isFlipped.y) ? y : sprite.sizes.y - y - 1;

                int2 texturePos = new int2(x, y) + position - (centerAligned ? sprite.sizes/2 : 0);
                if (GridHelper.InBound(texturePos, GameManager.RenderSizes) && sprite.pixels[xx, yy].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.RenderSizes.x);
                    outputColor[index] = sprite.pixels[xx, yy];
                }
            }
        }
    }


    //public static void ApplyLitSprite(ref NativeArray<Color32> outputColors, NativeSprite sprite, NativeSprite normalMap,
    //    int2 position, int2 renderPos, bool2 isFlipped, NativeList<LightSource> lights, float minLight = 0.5f, bool isCenterAligned = false)
    //{
    //    int2 sizes = sprite.sizes;
    //    Func<int2, int2, bool> canDrawPixel = (pixelPos, _) => sprite.pixels[pixelPos].a != 0;
    //    Func<int2, int2, Color> getColor = (pixelPos, _) => sprite.pixels[pixelPos];
    //    Func<int2, int2, Color> getNormal = (pixelPos, _) => normalMap.pixels[pixelPos];
    //    Func<int2, int2, Color> getLightColor = (pixelPos, _) => ApplyLightOnPixel(position, pixelPos, lights, getColor, getNormal, 0, minLight, 25, isFlipped.x);

    //    ApplyCustomRender(ref outputColors, renderPos, sizes, isFlipped, canDrawPixel, getLightColor, isCenterAligned);
    //}

    ////BORKEN
    //public static void ApplySkyBoxReflection(ref NativeArray<Color32> outputColors, NativeSprite sprite, NativeSprite normalMap, NativeSprite reflectionMap,
    //    int2 renderPos, bool2 isFlipped, EnvironementInfo info, ReflectionInfo reflectionInfo)
    //{
    //    var outputColorsPtr = outputColors;
    //    var infoPtr = info;

    //    Func<int2, int2, bool> canDrawPixel = (pixelPos, _) => reflectionMap.pixels[pixelPos].r != 0 && sprite.pixels[pixelPos].a != 0;
    //    Func<int2, int2, Color> getCurrentColor = (pixelPos, finalPos) => outputColorsPtr[ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes)];
    //    Func<int2, int2, Color> getBlendSkyTexture = (pixelPos, finalPos) => 
    //        ApplySkyboxReflection(pixelPos, getCurrentColor(pixelPos, finalPos), ((Color)reflectionMap.pixels[pixelPos]).r, normalMap.pixels[pixelPos].ToNormal(), info, reflectionInfo, isFlipped);

    //    ApplyCustomRender(ref outputColors, renderPos, sprite.sizes, isFlipped, canDrawPixel, getBlendSkyTexture, false);
    //}

    //public static void ApplyCustomRender(
    //    ref NativeArray<Color32> outputColors, int2 position, int2 sizes, bool2 isFlipped, 
    //    Func<int2, int2, bool> canRender, Func<int2, int2, Color> renderColor,
    //    bool centerAligned = false)
    //{
    //    for (int x = 0; x < sizes.x; x++)
    //    {
    //        for (int y = 0; y < sizes.y; y++)
    //        {
    //            int xx = (!isFlipped.x) ? x : sizes.x - x - 1;
    //            int yy = (!isFlipped.y) ? y : sizes.y - y - 1;

    //            int2 finalPos = new int2(x, y) + position - (centerAligned ? sizes / 2 : 0);
    //            int2 pixelPos = new int2(xx, yy);
    //            if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && canRender(pixelPos, finalPos))
    //            {
    //                int index = ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes.x);
    //                outputColors[index] = renderColor(pixelPos, finalPos);
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Render the outputColor to the screen and dispose the array
    /// </summary>
    public static void RenderToScreen(NativeArray<Color32> outputColor)
    {
        m_texture.SetPixelData(outputColor, 0);
        outputColor.Dispose();
        m_texture.Apply();
        Instance.m_renderer.texture = m_texture;
        //Instance.m_cubeRenderer.sharedMaterial.SetTexture("_MainTex", m_texture);
    }
    public static void RenderToText(NativeArray<Color32> outputColor)
    {
        NativeArray<char> outputASCII = new NativeArray<char>(outputColor.Length, Allocator.TempJob);
        new ASCIIRenderJob()
        {
            outputColors = outputColor,
            outputASCII = outputASCII
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        //cache it
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < outputASCII.Length; i++)
        {
            stringBuilder.Append(outputASCII[i]);
            if ((i + 1) % GameManager.RenderSizes.x == 0)
                stringBuilder.Append('\n');
        }
        Instance.m_rendererText.text = stringBuilder.ToString();

        outputASCII.Dispose();
        outputColor.Dispose();
    }



}
