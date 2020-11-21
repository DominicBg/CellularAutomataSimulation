using System;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class MainMenuDarkRender : IDisposable
{
    [Header("Dark Textures")]
    public LayerTexture background;
    public LayerTexture campFire;
    public LayerTexture astronaut;
    public VoronoiRendering voronoiBackground;

    public void Init()
    {
        background.Init();
        campFire.Init();
        astronaut.Init();
    }
    public void Dispose()
    {
        background.Dispose();
        campFire.Dispose();
        astronaut.Dispose();
    }

    public void Render(ref TickBlock tickBlock)
    {
        NativeArray<Color32> darkness = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
        voronoiBackground.Render(ref darkness, tickBlock.tick);
        background.Render(ref darkness);
        campFire.Render(ref darkness);
        astronaut.Render(ref darkness);
        GridRenderer.RenderToScreen(darkness);
    }
}