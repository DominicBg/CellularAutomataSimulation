using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCamera
{
    PixelCameraTransform transform;
    int2 viewPort;
    List<IRenderable> renderingObjects;
    public int2 position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public PixelCamera(PixelCameraTransform transform, int2 viewPort)
    {
        this.transform = transform;
        this.viewPort = viewPort;
        renderingObjects = new List<IRenderable>(100);
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
        renderingObjects.AddRange(alwaysRenderables);

        renderingObjects.Sort((a,b) => a.RenderingLayerOrder() - b.RenderingLayerOrder());


        //int count = renderingObjects.Count;
        int renderCount = renderingObjects.Count;
        var nativeLights = PrepareLights(renderData.lightSources, tickBlock.tick);

        //PreRender
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, GetRenderPosition(((LevelObject)renderingObjects[i]).position));
            else
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, position);
        }

        //Render Map
        GridRenderer.ApplyMapPixels(ref outputColors, renderData.map, ref tickBlock, position, nativeLights);


        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
                renderingObjects[i].Render(ref outputColors, ref tickBlock, GetRenderPosition(((LevelObject)renderingObjects[i]).position));
            else
                renderingObjects[i].Render(ref outputColors, ref tickBlock, position);
        }
    
        //Render Light
        LightRenderer.AddLight(ref outputColors, ref nativeLights, GetRenderingOffset(), GridRenderer.Instance.lightRendering.settings);

        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, GetRenderPosition(((LevelObject)renderingObjects[i]).position));
            else
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, position);
        }
  

        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
                renderingObjects[i].RenderUI(ref outputColors, ref tickBlock/*, GetRenderPosition(((LevelObject)renderingObjects[i]).position)*/);
            else
                renderingObjects[i].RenderUI(ref outputColors, ref tickBlock/*, position*/);
        }
      
        if (inDebug)
        {
            for (int i = 0; i < renderCount; i++)
            {
                if (renderingObjects[i] is LevelObject)
                    renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock, GetRenderPosition(((LevelObject)renderingObjects[i]).position));
                else
                    renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock, position);
            }
        }

        nativeLights.Dispose();
        return outputColors;
    }

    NativeList<LightSource> PrepareLights(ILightSource[] lightSources, int tick)
    {
        NativeList<LightSource> nativeLights = new NativeList<LightSource>(lightSources.Length, Allocator.TempJob);
        for (int i = 0; i < lightSources.Length; i++)
        {
            if(lightSources[i].isVisible())
                nativeLights.Add(lightSources[i].GetLightSource(tick));
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
