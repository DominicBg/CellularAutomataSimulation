using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class LevelElement : MonoBehaviour
{
    //References
    protected Map map;

    public bool isEnable = true;
    public bool isVisible = true;
    public LevelContainer levelContainer;

    public void Init(Map map, LevelContainer levelContainer)
    {
        this.map = map;
        this.levelContainer = levelContainer;
        OnInit();
    }

    public virtual void OnInit() { }

    public abstract void OnUpdate(ref TickBlock tickBlock);
    public virtual void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }

    public virtual void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }
    public virtual void PostRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }
    public virtual void RenderUI(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }
    public virtual void RenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }

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
}
