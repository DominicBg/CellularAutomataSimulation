using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GridRenderer : MonoBehaviour
{
    Texture2D texture;
    public ParticleRendering particleRendering;

    [System.Serializable]
    public struct ParticleRendering
    {
        public Color noneColor;
        public WaterRendering waterRendering;
        public SandRendering sandRendering;
        public IceRendering iceRendering;
        public RockRendering rockRendering;
        public Color mudColor;
        public Color snowColor;
    }

    [System.Serializable]
    public struct WaterRendering
    {
        [Header("Bubble Sin")]
        public float bubbleSineAmplitude;
        public float bubbleSineSpeed;
        public float bubbleSineNoiseAmplitude;
        public float2 bubbleSineOffSynch;
        public float2 bubbleSineNoiseSpeed;

        [Header("Bubble Color")]
        public Color bubbleOuterColor;
        public Color bubbleInnerColor;
        public float bubbleInnerThreshold;
        public float bubbleOuterThreshold;

        [Header("Other")]
        public float scaling;
        public Color waterColor;
        public float2 speed;
    }

    [System.Serializable]
    public struct SandRendering
    {
        public float shimmerThreshold;
        public float waveThreshold;
        public float2 waveScale;
        public float2 waveSpeed;
        public float2 waveScrollSpeed;
        public Color sandColor;
        public Color shimmerColor;
        public Color waveColor;
    }

    [System.Serializable]
    public struct IceRendering
    {
        public float thresholdShineReflection;
        public float reflectionShineSpeed;
        public Color reflectionShineColor;
        public Color iceColor;
        public float reflectionXDifference;
        public float reflectionShineAngle;
    }

    [System.Serializable]
    public struct RockRendering
    {
        public Color rockColor;
        public Color crackColor;
        public float noiseCrackThreshold;
        public float noiseScale;
    }

    public RawImage renderer;
    private Color32[] m_colors;

    public void OnUpdate(Map map, PixelSprite[] pixelSprites, int tick, uint tickSeed)
    {   
        FillColorArray(map, pixelSprites, tick, tickSeed, ref m_colors);
        RenderToScreen(m_colors);
    }

    public void FillColorArray(Map map, PixelSprite[] pixelSprites, int tick, uint TickSeed, ref Color32[] colors)
    {
        int size = map.ArrayLength;
        EnsureColorArray(ref colors, size);

        NativeArray<Color32> outputColor = new NativeArray<Color32>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new GridRendererJob()
        {
            colorArray = outputColor,
            map = map,
            particleRendering = particleRendering,
            tick = tick,
            random = new Unity.Mathematics.Random(TickSeed)
        }.Schedule(size, 1).Complete();

        //AddPixelSprite(outputColor, map, pixelSprite);

        for (int i = 0; i < pixelSprites.Length; i++)
        {
            AddPixelSprite(outputColor, map, pixelSprites[i]);
        }

        //Copy NativeArray to ColorArray
        for (int i = 0; i < size; i++)
        {
            colors[i] = outputColor[i];
        }
        outputColor.Dispose();
    }

    public void RenderToScreen(Color32[] colors)
    {
        texture.SetPixels32(colors);
        texture.Apply();
        renderer.texture = texture;
    }

    void EnsureColorArray(ref Color32[] colors, int size)
    {
        if (colors == null || colors.Length != size)
        {
            colors = new Color32[size];
        }
    }

    public void Init(int2 sizes)
    {
        texture = new Texture2D(sizes.x, sizes.y, TextureFormat.RGBA32, false, true);
        texture.filterMode = FilterMode.Point;
    }

    //This is going to be cancer to burst lol
    void AddPixelSprite(NativeArray<Color32> outputColor, Map map, PixelSprite sprite)
    {
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 texturePos = new int2(x, y) + sprite.position;
                if(sprite.collisions[x,y])
                {
                    int index = ArrayHelper.PosToIndex(texturePos, map.Sizes);
                    outputColor[index] = sprite.pixels[x, y];
                }
            }
        }
    }
}
