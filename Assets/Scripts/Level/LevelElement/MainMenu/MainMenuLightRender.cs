using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class MainMenuLightRender : IDisposable
{
    [Header("Light Textures")]
    public LayerTexture sandBackground;
    public LayerTexture campFire;
    public LayerTextureSheet campFireFlame;
    public LayerTexture astronaut;
    public FireRendering fireRendering;
    public ShadowRendering shadowRendering;
    public StarBackgroundRendering starBackground;
    public LayerTexture title;

    public void Init()
    {
        campFire.Init();
        campFireFlame.Init();
        sandBackground.Init();
        astronaut.Init();
        title.Init();
    }
    public void Dispose()
    {
        campFire.Dispose();
        campFireFlame.Dispose();
        sandBackground.Dispose();
        astronaut.Dispose();
        title.Dispose();
    }

    public NativeArray<Color32> Render(ref TickBlock tickBlock, ref Map map)
    {
        var pass1 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        starBackground.Render(ref pass1, tickBlock.tick);
        GridRenderer.ApplyMapPixels(ref pass1, map, tickBlock);
        title.Render(ref pass1);

        var pass2 = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        GridRenderer.ApplyParticleRenderToTexture(ref pass2, ref sandBackground.nativeTexture, map, tickBlock, sandBackground.blending, ParticleType.Sand);
        campFire.Render(ref pass2);
        shadowRendering.Render(ref pass2, tickBlock.tick);

        var pass3 = GridRenderer.CombineColors(ref pass1, ref pass2);

        fireRendering.Render(ref pass3, tickBlock.tick);
        campFireFlame.Render(ref pass3, tickBlock.tick);
        astronaut.Render(ref pass3);

        return pass3;
    }

    [System.Serializable]
    public struct FireRendering : IRenderableAnimated
    {
        public int2 position;
        public Color32[] colors;
        public int[] radiusMin;
        public int[] radiusMax;
        public float speed;
        public BlendingMode blending;

        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                float sin = math.sin(tick * speed) * 0.5f + 0.5f;
                int radius = (int)math.lerp(radiusMin[i], radiusMax[i], sin);
                GridRenderer.RenderCircle(ref colorArray, position, radius, colors[i], blending);
            }
        }
    }

    [System.Serializable]
    public struct ShadowRendering : IRenderableAnimated
    {
        public int2 position;
        public Color32[] colors;
        public int2[] radiusMin;
        public int2[] radiusMax;
        public float speed;
        public BlendingMode blending;

        public void Render(ref NativeArray<Color32> colorArray, int tick)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                float sin = math.sin(tick * speed) * 0.5f + 0.5f;
                int2 radius = (int2)math.lerp(radiusMin[i], radiusMax[i], sin);
                GridRenderer.RenderEllipseMask(ref colorArray, position, radius, colors[i], blending);
            }
        }
    }
}
