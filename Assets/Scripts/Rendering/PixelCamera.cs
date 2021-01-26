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

    public NativeArray<Color32> Render(PixelScene pixelScene, ref TickBlock tickBlock, bool inDebug)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        renderingObjects.Clear();
        Bound viewPortBound = new Bound(position - viewPort/2, viewPort);

        //maybe do it more optimzied lol
        for (int i = 0; i < pixelScene.levelObjects.Length; i++)
        {
            if(pixelScene.levelObjects[i] != null && viewPortBound.IntersectWith(pixelScene.levelObjects[i].GetBound()))
                renderingObjects.Add(pixelScene.levelObjects[i]);
        }

        IAlwaysRenderable[] alwaysRenderables = pixelScene.alwaysRenderables;
        renderingObjects.AddRange(alwaysRenderables);

        renderingObjects.Sort((a,b) => a.RenderingLayerOrder() - b.RenderingLayerOrder());


        //int count = renderingObjects.Count;
        int renderCount = renderingObjects.Count;
        var lights = PrepareLights(pixelScene.lightSources, tickBlock.tick);

        //PreRender
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, renderPos);
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, renderPos, ref lights);
            }
            else
            { 
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, position); 
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, position, ref lights); 
            }
        }

        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].Render(ref outputColors, ref tickBlock, renderPos);
                renderingObjects[i].Render(ref outputColors, ref tickBlock, renderPos, ref lights);
            }
            else
            {
                renderingObjects[i].Render(ref outputColors, ref tickBlock, position);
                renderingObjects[i].Render(ref outputColors, ref tickBlock, position, ref lights);
            }
        }
    
        //Post render
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, renderPos);
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, renderPos, ref lights);
            }
            else
            {
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, position);
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, position, ref lights);
            }
        }
  
        //Render Light
        LightRenderer.AddLight(ref outputColors, ref lights, GetRenderingOffset(), GridRenderer.Instance.lightRendering.settings);


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

        lights.Dispose();
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
