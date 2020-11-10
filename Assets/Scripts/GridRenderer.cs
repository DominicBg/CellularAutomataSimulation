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
        public Color mudColor;
    }
    [System.Serializable]
    public struct WaterRendering
    {
        public float bubbleInnerThreshold;
        public float bubbleOuterThreshold;
        public float scaling;
        public Color waterColor;
        public Color bubbleOuterColor;
        public Color bubbleInnerColor;
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

    public RawImage renderer;
    private Color32[] colors;

    public void OnUpdate(Map map, PixelSprite[] pixelSprites)
    {
        int size = map.ArrayLength;
        EnsureColorArray(size);

        NativeArray<Color32> outputColor = new NativeArray<Color32>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new GridRendererJob()
        {
            colorArray = outputColor,
            map = map,
            particleRendering = particleRendering,
            //particles = particles,
            tick = CellularAutomata.Tick,
            random = new Unity.Mathematics.Random(CellularAutomata.TickSeed)
        }.Schedule(size, 1).Complete();


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
        texture.SetPixels32(colors);

        texture.Apply();
        renderer.texture = texture;
    }

    void EnsureColorArray(int size)
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
