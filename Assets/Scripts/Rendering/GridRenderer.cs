using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

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
        outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        ApplyMapPixels(ref outputColor, map, tickBlock);
        ApplyPixelSprites(ref outputColor, pixelSprites);
        ApplyPostProcess(ref outputColor);
    }

    public static void ApplyMapPixels(ref NativeArray<Color32> outputColor, Map map, TickBlock tickBlock)
    {
        using (S_SimulationRender.Auto())
        {
            new GridRendererJob()
            {
                colorArray = outputColor,
                map = map,
                particleRendering = Instance.particleRendering,
                tick = tickBlock.tick,
                random = new Unity.Mathematics.Random(tickBlock.tickSeed)
            }.Schedule(GameManager.GridLength, 1).Complete();
        }
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

    public static void ApplyTextureToColor(ref NativeArray<Color32> outputColor, Texture2D texture, ApplyTextureJob.Blending blending)
    {
        NativeArray<Color32> nativeTexture = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
        new ApplyTextureJob()
        {
            outputColor = outputColor,
            texture = nativeTexture,
            blending = blending,
        }.Schedule(GameManager.GridLength, 1).Complete();
        nativeTexture.Dispose();
    }

    public static void ApplyTextureToColor(ref NativeArray<Color32> outputColor, ref NativeArray<Color32> texture, ApplyTextureJob.Blending blending)
    {
        new ApplyTextureJob()
        {
            outputColor = outputColor,
            texture = texture,
            blending = blending,
        }.Schedule(GameManager.GridLength, 1).Complete();
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
