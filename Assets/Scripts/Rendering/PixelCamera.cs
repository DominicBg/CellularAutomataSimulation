using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCamera
{
    PixelCameraTransform transform;
    int2 viewPort;
    List<LevelObject> renderingObjects;
    public int2 position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public PixelCamera(PixelCameraTransform transform, int2 viewPort)
    {
        this.transform = transform;
        this.viewPort = viewPort;
        renderingObjects = new List<LevelObject>(100);
    }

    //have a prerender pass that computes the lights positions n filtering
    public struct RenderData
    {
        public LevelObject[] levelObjects;
        public IAlwaysRenderable[] alwaysRenderables;
        public ILightSource[] lightSources;
        public Map map;
    }

    public NativeArray<Color32> Render(RenderData renderData, ref TickBlock tickBlock, bool inDebug)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        renderingObjects.Clear();
        Bound viewPortBound = new Bound(position - viewPort/2, viewPort);

        //maybe do it more optimzied lol
        for (int i = 0; i < renderData.levelObjects.Length; i++)
        {
            if(renderData.levelObjects[i] != null && viewPortBound.IntersectWith(renderData.levelObjects[i].GetBound()))
                renderingObjects.Add(renderData.levelObjects[i]);
        }
        IAlwaysRenderable[] alwaysRenderables = renderData.alwaysRenderables;

        int count = renderingObjects.Count;
        int renderCount = renderData.alwaysRenderables.Length;
        var nativeLights = PrepareLights(renderData.lightSources, tickBlock.tick);

        //PreRender
        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].PreRender(ref outputColors, ref tickBlock, position);
        for (int i = 0; i < count; i++)
            renderingObjects[i].PreRender(ref outputColors, ref tickBlock, GetRenderPosition(renderingObjects[i].position));

        //Render Map
        GridRenderer.ApplyMapPixels(ref outputColors, renderData.map, ref tickBlock, position, nativeLights);

        //Render
        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].Render(ref outputColors, ref tickBlock, position);
        for (int i = 0; i < count; i++)
            renderingObjects[i].Render(ref outputColors, ref tickBlock, GetRenderPosition(renderingObjects[i].position));

        //Render Light
        LightRenderer.AddLight(ref outputColors, ref nativeLights, GetRenderingOffset(), GridRenderer.Instance.lightRendering.settings);

        //Render PostRender
        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].PostRender(ref outputColors, ref tickBlock/*, GetRenderPosition(cameraPos, renderingObjects[i])*/);
        for (int i = 0; i < count; i++)
            renderingObjects[i].PostRender(ref outputColors, ref tickBlock);

        //Render UI
        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].RenderUI(ref outputColors, ref tickBlock/*, GetRenderPosition(cameraPos, renderingObjects[i])*/);
        for (int i = 0; i < count; i++)
            renderingObjects[i].RenderUI(ref outputColors, ref tickBlock);

        if (inDebug)
        {
            for (int i = 0; i < renderCount; i++)
                alwaysRenderables[i].RenderDebug(ref outputColors, ref tickBlock, 0);
            for (int i = 0; i < count; i++)
                renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock, GetRenderPosition(renderingObjects[i].position));
        }

        nativeLights.Dispose();
        return outputColors;
    }

    NativeArray<LightSource> PrepareLights(ILightSource[] lightSources, int tick)
    {
        NativeArray<LightSource> nativeLights = new NativeArray<LightSource>(lightSources.Length, Allocator.TempJob);
        for (int i = 0; i < nativeLights.Length; i++)
        {
            nativeLights[i] = lightSources[i].GetLightSource(tick);
        }
        return nativeLights;
    }

    public int2 GetRenderPosition(int2 position)
    {
        return position + GetRenderingOffset();
    }

    int2 GetRenderingOffset()
    {
        return -(position - viewPort / 2);
    }

    public Bound GetViewingBound()
    {
        return Bound.CenterAligned(position, viewPort);
    }
}
