using System.Collections;
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
    public ParticleRendering particleRendering;

    static ProfilerMarker S_SimulationRender = new ProfilerMarker("GridRenderer.SimulationRender");
    static ProfilerMarker s_SpriteRender = new ProfilerMarker("GridRenderer.SpriteRendering");

    [SerializeField] RawImage m_renderer = default;
    public static GridPostProcess postProcess;
    private static Texture2D m_texture;

    void Awake()
    {
        Instance = this;

        int2 sizes = GameManager.GridSizes;
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

    public static void ApplyMapPixels(ref NativeArray<Color32> outputColor, Map map, ref TickBlock tickBlock, int2 currentLevel, NativeArray<LightSource> lightSources)
    {
        using (S_SimulationRender.Auto())
        {
            new GridRendererJob(outputColor, map, Instance.particleRendering, tickBlock, currentLevel, lightSources).Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
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

    public static void DrawBound(ref NativeArray<Color32> outputColor, Bound bound, Color32 color, BlendingMode blending = BlendingMode.Normal)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        new ApplyPixelsJob(outputColor, positions, colors, GameManager.GridSizes, blending).Run();
        positions.Dispose();
        colors.Dispose();
    }

    public static void ApplyPixels(ref NativeArray<Color32> outputColor, ref NativeArray<int2> pixelPositions, ref NativeArray<Color32> pixelcolors, BlendingMode blending = BlendingMode.Normal)
    {
        new ApplyPixelsJob(outputColor, pixelPositions, pixelcolors, GameManager.GridSizes, blending).Run();
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
            mapSizes = GameManager.GridSizes
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        colors.Dispose();
        return outputColor;
    }

    public static void ApplySprite(ref NativeArray<Color32> outputColor, NativeSprite sprite, int2 position, bool isflipped = false)
    {
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int xx = (!isflipped) ? x : sprite.sizes.x - x - 1;

                int2 texturePos = new int2(x, y) + position;
                if (GridHelper.InBound(texturePos, GameManager.GridSizes) && sprite.pixels[xx, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.GridSizes.x);
                    outputColor[index] = sprite.pixels[xx, y];
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
    }
}
