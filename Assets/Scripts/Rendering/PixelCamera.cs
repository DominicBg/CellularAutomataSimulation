using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCamera
{
    public PixelCameraTransform transform;
    int2 viewPort;
    List<IRenderable> renderingObjects;


    public int2 position
    {
        get => transform.position + transform.offset;
        set => transform.position = value;
    }

    public PixelCamera(PixelCameraTransform transform, int2 viewPort)
    {
        this.transform = transform;
        this.viewPort = viewPort;
        renderingObjects = new List<IRenderable>(100);
    }


    public NativeArray<Color32> Render(PixelScene pixelScene, ref TickBlock tickBlock, bool inDebug, System.Action<NativeArray<Color32>> onRenderPass = null)
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


        int renderCount = renderingObjects.Count;
        var lights = PrepareLights(pixelScene.lightSources, pixelScene.lightMultiSource, tickBlock.tick);

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
            onRenderPass?.Invoke(outputColors);
        }

        //Render
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
            onRenderPass?.Invoke(outputColors);
        }

        //Late render
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, renderPos);
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, renderPos, ref lights);
            }
            else
            {
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, position);
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, position, ref lights);
            }
            onRenderPass?.Invoke(outputColors);
        }

        //Render Light
        LightRenderer.AddLight(ref outputColors, ref lights, GetRenderingOffset(), GridRenderer.Instance.lightRendering.settings);

        //Post Process render
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
            onRenderPass?.Invoke(outputColors);
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
            onRenderPass?.Invoke(outputColors);
        }

        lights.Dispose();
        return outputColors;
    }

    NativeList<LightSource> PrepareLights(ILightSource[] lightSources, ILightMultiSource[] lightMultiSource, int tick)
    {
        NativeList<LightSource> nativeLights = new NativeList<LightSource>(lightSources.Length, Allocator.TempJob);
        for (int i = 0; i < lightSources.Length; i++)
        {
            if(lightSources[i].IsVisible())
                nativeLights.Add(lightSources[i].GetLightSource(tick));
        }
        for (int i = 0; i < lightMultiSource.Length; i++)
        {
            if (lightMultiSource[i].IsVisible())
                lightMultiSource[i].GetLightSource(nativeLights, tick);
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

    public PixelCameraHandle GetHandle()
    {
        return new PixelCameraHandle() { cameraPosition = position, viewPort = viewPort };
    }

    //Simpler Camera Version for Burst
    public struct PixelCameraHandle
    {
        public int2 cameraPosition;
        public int2 viewPort;

        public int2 GetRenderPosition(int2 position)
        {
            return position - (cameraPosition - viewPort / 2);
        }
        public int2 GetGlobalPosition(int2 renderPos)
        {
            return renderPos + (cameraPosition - viewPort / 2);
        }
    }
}
