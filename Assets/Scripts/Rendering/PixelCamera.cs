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
    HashSet<IRenderable> renderingHash;

    public int2 position
    {
        get => transform.position + transform.offset + transform.shakeOffset;
        set => transform.position = value;
    }

    public PixelCamera(PixelCameraTransform transform, int2 viewPort)
    {
        this.transform = transform;
        this.viewPort = viewPort;
        renderingObjects = new List<IRenderable>(100);
        renderingHash = new HashSet<IRenderable>();
    }


    public NativeArray<Color32> Render(PixelScene pixelScene, ref TickBlock tickBlock, bool inDebug, System.Action<NativeArray<Color32>> onRenderPass = null)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        renderingObjects.Clear();
        renderingHash.Clear();

        Bound viewPortBound = new Bound(position - viewPort / 2, viewPort);

        //Add always renderable before the objects, because objects have a hash to prevent double rendering
        IAlwaysRenderable[] alwaysRenderables = pixelScene.alwaysRenderables;
        renderingObjects.AddRange(alwaysRenderables);

        //maybe do it more optimzied lol
        for (int i = 0; i < pixelScene.levelObjects.Length; i++)
        {
            if (!renderingHash.Contains(pixelScene.levelObjects[i]) && pixelScene.levelObjects[i] != null && viewPortBound.IntersectWith(pixelScene.levelObjects[i].GetBound()))
            {
                renderingObjects.Add(pixelScene.levelObjects[i]);
                renderingHash.Add(pixelScene.levelObjects[i]);
            }
        }

        renderingObjects.Sort((a, b) => a.RenderingLayerOrder() - b.RenderingLayerOrder());


        int renderCount = renderingObjects.Count;
        EnvironementInfo info = new EnvironementInfo();
        info.lightSources = PrepareLights(pixelScene.lightSources, pixelScene.lightMultiSource, tickBlock.tick);
        info.cameraHandle = GetHandle();

        //SkyBoxRender
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].SkyBoxRender(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            {
                renderingObjects[i].SkyBoxRender(ref outputColors, ref tickBlock, position, ref info);
            }
            onRenderPass?.Invoke(outputColors);

        }
        info.skybox = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        info.reflectionIndices = new NativeGrid<int>(GameManager.RenderSizes, Allocator.TempJob);


        //PreRender
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            { 
                renderingObjects[i].PreRender(ref outputColors, ref tickBlock, position, ref info); 
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
                renderingObjects[i].Render(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            {
                renderingObjects[i].Render(ref outputColors, ref tickBlock, position, ref info);
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
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            {
                renderingObjects[i].LateRender(ref outputColors, ref tickBlock, position, ref info);
            }
            onRenderPass?.Invoke(outputColors);
        }

        //Reflection Pass
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].RenderReflection(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            {
                renderingObjects[i].RenderReflection(ref outputColors, ref tickBlock, position, ref info);
            }
            onRenderPass?.Invoke(outputColors);
        }


        //Render Light
        LightRenderer.AddLight(ref outputColors, ref info.lightSources, GetRenderingOffset(), GridRenderer.Instance.lightRendering.settings);

        //Post Process render
        for (int i = 0; i < renderCount; i++)
        {
            if (!renderingObjects[i].IsVisible())
                continue;

            if (renderingObjects[i] is LevelObject)
            {
                int2 renderPos = GetRenderPosition(((LevelObject)renderingObjects[i]).position);
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, renderPos, ref info);
            }
            else
            {
                renderingObjects[i].PostRender(ref outputColors, ref tickBlock, position, ref info);
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
                if (!renderingObjects[i].IsVisible())
                    continue;
                if (renderingObjects[i] is LevelObject)
                    renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock, GetRenderPosition(((LevelObject)renderingObjects[i]).position));
                else
                    renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock, position);
            }
            onRenderPass?.Invoke(outputColors);
        }

        info.Dispose();
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
        //return position + GetRenderingOffset();
        return GetRenderPosition(this.position, viewPort, position);
    }

    public int2 GetGlobalPosition(int2 renderPosition)
    {
        return GetGlobalPosition(this.position, viewPort, renderPosition);
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
            //return position - (cameraPosition - viewPort / 2);
            return PixelCamera.GetRenderPosition(cameraPosition, viewPort, position);
        }
        public int2 GetGlobalPosition(int2 renderPos)
        {
            //return renderPos + (cameraPosition - viewPort / 2);
            return PixelCamera.GetGlobalPosition(cameraPosition, viewPort, renderPos);
        }
    }

    public static int2 GetRenderPosition(int2 cameraPosition, int2 viewPort, int2 position)
    {
        return position - (cameraPosition - viewPort / 2);
    }
    public static int2 GetGlobalPosition(int2 cameraPosition, int2 viewPort, int2 renderPos)
    {
        return renderPos + (cameraPosition - viewPort / 2);
    }

}

public struct EnvironementInfo
{
    public NativeArray<Color32> skybox;
    public NativeList<LightSource> lightSources;
    public NativeGrid<int> reflectionIndices;
    public PixelCamera.PixelCameraHandle cameraHandle;

    int internalReflectionIndex;

    public int GetReflectionIndex()
    {
        return internalReflectionIndex++;
    }

    ///// <summary>
    ///// This is only possible when calling from a Render function
    ///// </summary>
    public Color SampleSkybox(int2 renderPos)
    {
        if (!GridHelper.InBound(renderPos, GameManager.RenderSizes))
            return Color.clear;

        return skybox[ArrayHelper.PosToIndex(renderPos, GameManager.RenderSizes)];
    }

    public void Dispose()
    {
        skybox.Dispose();
        lightSources.Dispose();
        reflectionIndices.Dispose();
    }
}