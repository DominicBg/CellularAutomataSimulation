using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCamera
{
    int2 viewPort;
    List<LevelObject> renderingObjects;
    List<IAlwaysRenderable> alwaysRenderables;

    public PixelCamera(int2 viewPort)
    {
        this.viewPort = viewPort;
        renderingObjects = new List<LevelObject>(100);
        alwaysRenderables = new List<IAlwaysRenderable>(100);

        ////hack

        //var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        //for (int i = 0; i < roots.Length; i++)
        //{
        //    alwaysRenderables.AddRange(roots[i].GetComponentsInChildren<IAlwaysRenderable>());
        //}
    }

    public NativeArray<Color32> Render(int2 cameraPos, LevelObject[] levelObjects, IAlwaysRenderable[] alwaysRenderables, ref TickBlock tickBlock, bool inDebug)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        renderingObjects.Clear();
        Bound viewPortBound = new Bound(cameraPos - viewPort/2, viewPort);

        //maybe do it more optimzied lol
        for (int i = 0; i < levelObjects.Length; i++)
        {
            if(levelObjects[i] != null && viewPortBound.IntersectWith(levelObjects[i].GetBound()))
                renderingObjects.Add(levelObjects[i]);
        }


        int count = renderingObjects.Count;
        int renderCount = alwaysRenderables.Length;

        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].PreRender(ref outputColors, ref tickBlock, cameraPos);
        for (int i = 0; i < count; i++)
            renderingObjects[i].PreRender(ref outputColors, ref tickBlock, GetRenderPosition(cameraPos, renderingObjects[i].position));


        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].Render(ref outputColors, ref tickBlock, cameraPos);
        for (int i = 0; i < count; i++)
            renderingObjects[i].Render(ref outputColors, ref tickBlock, GetRenderPosition(cameraPos, renderingObjects[i].position));

        //LightRenderer.AddLight(ref outputColors, ref levelContainer.lightSources, levelContainer.GetGlobalOffset(), GridRenderer.Instance.lightRendering.settings);

        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].PostRender(ref outputColors, ref tickBlock/*, GetRenderPosition(cameraPos, renderingObjects[i])*/);
        for (int i = 0; i < count; i++)
            renderingObjects[i].PostRender(ref outputColors, ref tickBlock);

        for (int i = 0; i < renderCount; i++)
            alwaysRenderables[i].RenderUI(ref outputColors, ref tickBlock/*, GetRenderPosition(cameraPos, renderingObjects[i])*/);
        for (int i = 0; i < count; i++)
            renderingObjects[i].RenderUI(ref outputColors, ref tickBlock);

        if (inDebug)
        {
            for (int i = 0; i < renderCount; i++)
                alwaysRenderables[i].RenderDebug(ref outputColors, ref tickBlock/*, GetRenderPosition(cameraPos, renderingObjects[i])*/);
            for (int i = 0; i < count; i++)
                renderingObjects[i].RenderDebug(ref outputColors, ref tickBlock);
        }

        //GridRenderer.RenderToScreen(outputColors);
        return outputColors;
    }

    public int2 GetRenderPosition(int2 cameraPos, int2 position)
    {
        return position - (cameraPos - viewPort / 2);
    }
}
