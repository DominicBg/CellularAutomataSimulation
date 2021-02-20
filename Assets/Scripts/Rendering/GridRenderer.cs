using System;
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

    public static void ApplyMapPixels(ref NativeArray<Color32> outputColor, Map map, ref TickBlock tickBlock, float2 cameraPos, NativeArray<LightSource> lightSources, bool debug = false)
    {
        using (S_SimulationRender.Auto())
        {
            new GridRendererJob(outputColor, map, Instance.particleRendering, tickBlock, cameraPos, lightSources, debug).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
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
        int2 worldPosition, int2 renderPos, NativeList<LightSource> lights, in ShadingLitInfo litInfo, bool centerAligned = false)
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
                    outputColors[index] = ApplyLightOnPixel(worldPosition + localPos + centerAligendOffset, colors[localPos], normals[localPos], lights, in litInfo);
                }
            }
        }
    }

    public static void ApplySpriteSkyboxReflection(ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, in NativeGrid<float3> normals, in NativeGrid<float> reflectiveMap,
    int2 renderPos, ref EnvironementInfo info, in ReflectionInfo reflectionInfo, bool centerAligned = false)
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
                    outputColor[index] = ApplySkyboxReflection(finalPos, outputColor[index], reflectiveMap[localPos], normals[localPos], info, in reflectionInfo);
                }
            }
        }
    }

    public static void PrepareSpriteEnvironementReflection(in NativeGrid<Color32> colors, int2 renderPos, ref EnvironementInfo info, int reflectionIndex, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 localPos = new int2(x, y);
                int2 finalPos = renderPos + localPos - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    info.reflectionIndices[finalPos] = reflectionIndex;
                }
            }
        }
    }


    public static void ApplySpriteEnvironementReflection(
        ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, in NativeGrid<float3> normals, in NativeGrid<float> reflectiveMap,
        int2 renderPos, int reflectionIndex, ref EnvironementInfo info, in EnvironementReflectionInfo reflectionInfo, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 localPos = new int2(x, y);
                int2 finalPos = renderPos + localPos - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(finalPos, GameManager.RenderSizes) && colors[x, y].a != 0)
                {
                    if (info.reflectionIndices[finalPos] != reflectionIndex)
                        continue;

                    int index = ArrayHelper.PosToIndex(finalPos, GameManager.RenderSizes);

                    int2 direction = (int2)(normals[localPos].xy * reflectionInfo.distance);

                    int2 mirrorOffset = new int2(colors.Sizes.x - localPos.x - 1, localPos.y);
                    int2 nonMirrorOffset = localPos;
                    //Used when we want surface to reflect without mirror, for backlighting and 1 pixel width reflection
                    int2 finalOffset = (reflectiveMap[localPos] < reflectionInfo.mirrorReflectionThreshold) ? mirrorOffset : nonMirrorOffset;

                    int2 samplePos = renderPos + direction + finalOffset;
                    if (!GridHelper.InBound(samplePos, GameManager.RenderSizes))
                        continue;

                    Color sampleColor = outputColor[ArrayHelper.PosToIndex(samplePos, GameManager.RenderSizes)];
                    if (reflectionInfo.blurRadius <= 1)
                    {   
                        //Take sample directly
                        outputColor[index] = Blend(outputColor[index], sampleColor.Alpha(reflectiveMap[localPos] * reflectionInfo.amount), reflectionInfo.blending);
                        continue;
                    }

                    //Add Blur
                    Color color = Color.clear;
                    var circlePos = GridHelper.GetCircleAtPosition(samplePos, reflectionInfo.blurRadius, GameManager.RenderSizes, Allocator.Temp);
                    for (int i = 0; i < circlePos.Length; i++)
                    {
                        color += outputColor[ArrayHelper.PosToIndex(circlePos[i], GameManager.RenderSizes)];
                    }
                    color /= circlePos.Length;
                    color.a = reflectionInfo.blurIntensity;
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

    public static void ApplySpriteCustom(ref NativeArray<Color32> outputColor, in NativeGrid<Color32> colors, int2 renderPos, 
        Func<int2, bool> canRender, Func<int2, Color> getcolor, bool centerAligned = false)
    {
        for (int x = 0; x < colors.Sizes.x; x++)
        {
            for (int y = 0; y < colors.Sizes.y; y++)
            {
                int2 localPos = new int2(x, y);
                int2 texturePos = renderPos + localPos - (centerAligned ? colors.Sizes / 2 : 0);
                if (GridHelper.InBound(texturePos, GameManager.RenderSizes) && canRender(localPos))
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.RenderSizes.x);
                    outputColor[index] = getcolor(localPos);
                }
            }
        }
    }


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
