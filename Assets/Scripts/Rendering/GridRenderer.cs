﻿using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] ParticleRendering particleRendering;

    static ProfilerMarker S_SimulationRender = new ProfilerMarker("GridRenderer.SimulationRender");
    static ProfilerMarker s_SpriteRender = new ProfilerMarker("GridRenderer.SpriteRendering");
    static ProfilerMarker s_PostProcessRender = new ProfilerMarker("GridRenderer.PostProcessRender");

    [SerializeField] RawImage m_renderer;
    public static GridPostProcess postProcess;
    private static Texture2D m_texture;

    public int innerLoopBatchCount = 10;

    //Consider getting init by GameManager
    void Awake()
    {
        Instance = this;

        int2 sizes = GameManager.GridSizes;
        m_texture = new Texture2D(sizes.x, sizes.y, TextureFormat.RGBA32, false, true);
        m_texture.filterMode = FilterMode.Point;
        postProcess = new GridPostProcess();
        postProcess.OnStart();
    }

    void OnDestroy()
    {
        postProcess.OnEnd();
    }

    public static void RenderMapAndSprites(Map map, PixelSprite[] pixelSprites, TickBlock tickBlock)
    {   
        FillColorArray(out NativeArray<Color32> outputColor, map, pixelSprites, tickBlock);
        RenderToScreen(outputColor);
    }

    public static void FillColorArray(out NativeArray<Color32> outputColor, Map map, PixelSprite[] pixelSprites, TickBlock tickBlock)
    {
        outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        ApplyMapPixels(ref outputColor, map, tickBlock);
        ApplyPixelSprites(ref outputColor, pixelSprites);
        ApplyPostProcess(ref outputColor);
    }

    public static void ApplyMapPixels(ref NativeArray<Color32> outputColor, Map map, TickBlock tickBlock)
    {
        using (S_SimulationRender.Auto())
        {
            new GridRendererJob(outputColor, map, Instance.particleRendering, tickBlock).Schedule(GameManager.GridLength, 1).Complete();
        }
    }

    public static void ApplyParticleRenderToTexture(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> textureColor, Map map, TickBlock tickBlock, BlendingMode blending, ParticleType particleType)
    {
        //todo profile
        new ApplyParticleRenderToTextureJob(outputColor, textureColor, map, Instance.particleRendering, tickBlock, blending, particleType).Schedule(GameManager.GridLength, 1).Complete();
    }

    public static void ApplyPixelSprites(ref NativeArray<Color32> outputColor, PixelSprite[] pixelSprites)
    {
        using (s_SpriteRender.Auto())
        {
            for (int i = 0; i < pixelSprites.Length; i++)
            {
                AddPixelSprite(outputColor, pixelSprites[i]);
            }
        }
    }

    public static void RenderCircle(ref NativeArray<Color32> outputColor, int2 position, int radius, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        GetColoredCircle(position, radius,
                    GameManager.GridSizes, color, Allocator.TempJob,
                    out NativeArray<int2> positions, out NativeArray<Color32> colors);

        ApplyPixels(ref outputColor, ref positions, ref colors, blending);
        positions.Dispose();
        colors.Dispose();
    }

    public static void RenderEllipseMask(ref NativeArray<Color32> outputColor, int2 position, int2 radius, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        GetEllipseMask(position, radius,
                    GameManager.GridSizes, color, Allocator.TempJob,
                    out NativeArray<Color32> colors);

        ApplyTextureToColor(ref outputColor, ref colors, blending, useAlphaMask: true);
        colors.Dispose();
    }


    public static void ApplyPixels(ref NativeArray<Color32> outputColor, ref NativeArray<int2> pixelPositions, ref NativeArray<Color32> pixelcolors, BlendingMode blending = BlendingMode.Normal)
    {
        new ApplyPixelsJob(outputColor, pixelPositions, pixelcolors, GameManager.GridSizes, blending).Run();
    }

    public static void ApplyTextureToColor(ref NativeArray<Color32> outputColor, Texture2D texture, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        NativeArray<Color32> nativeTexture = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
        new ApplyTextureJob(outputColor, nativeTexture, blending, useAlphaMask).Schedule(GameManager.GridLength, Instance.innerLoopBatchCount).Complete();
        nativeTexture.Dispose();
    }

    public static void ApplyTextureToColor(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> texture, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        new ApplyTextureJob(outputColor, texture, blending, useAlphaMask).Schedule(GameManager.GridLength, Instance.innerLoopBatchCount).Complete();
    }

    /// <summary>
    /// Apply colors to output colors and dispose colors
    /// </summary>

    public static NativeArray<Color32> CombineColors(ref NativeArray<Color32> colorsA, ref NativeArray<Color32> colorsB, BlendingMode blending = BlendingMode.Normal, bool useAlphaMask = false)
    {
        new ApplyTextureJob(colorsA, colorsB, blending, useAlphaMask).Schedule(GameManager.GridLength, Instance.innerLoopBatchCount).Complete();
        colorsB.Dispose();
        return colorsA;
    }

    public static void ApplyPostProcess(ref NativeArray<Color32> outputColor)
    {
        using (s_PostProcessRender.Auto())
        {
            postProcess.ApplyPostProcess(ref outputColor);
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
    }

    //This is going to be cancer to burst lol
    static void AddPixelSprite(NativeArray<Color32> outputColor, PixelSprite sprite)
    {
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 texturePos = new int2(x, y) + sprite.position;
                if(sprite.collisions[x,y])
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.GridSizes.x);
                    outputColor[index] = sprite.pixels[x, y];
                }
            }
        }
    }
}
