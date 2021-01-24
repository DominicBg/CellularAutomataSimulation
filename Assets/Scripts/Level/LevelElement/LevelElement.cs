using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class LevelElement : MonoBehaviour, IRenderable
{
    //References
    protected Map map;

    public bool isEnable = true;
    public bool isVisible = true;
    public int renderingLayerOrder = 0;
    public bool isInit = false;

    public void Init(Map map)
    {
        this.map = map;
        OnInit();
        isInit = true;
    }

    public void OnValidate()
    {
        PixelScene pixelScene = GetComponentInParent<PixelScene>();
        pixelScene?.OnValidate();
        pixelScene?.OnValidate();
        pixelScene?.RequestInit(this);
    }

    public virtual void OnInit() { }

    public virtual void OnUpdate(ref TickBlock tickBlock) { }
    public virtual void OnLateUpdate(ref TickBlock tickBlock) { }
    public virtual void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos) { }

    public virtual void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos) { }
    public virtual void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos) { }
    public virtual void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock) { }
    public virtual void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos) { }

    public virtual void Dispose()
    {

    }

    protected T GetLevelElement<T>() where T : LevelElement
    {
        return transform.parent.GetComponentInChildren<T>();
    }
    protected T[] GetLevelElements<T>() where T : LevelElement
    {

        return transform.parent.GetComponentsInChildren<T>();
    }

    protected T[] GetInterfaces<T>()
    {
        //lol
        return transform.parent.GetComponentsInChildren<T>();
    }

    public void Render(ref NativeArray<Color32> colorArray)
    {
        throw new System.NotImplementedException();
    }

    public int RenderingLayerOrder()
    {
        return renderingLayerOrder;
    }

    public bool IsVisible()
    {
        return isVisible;
    }
}
