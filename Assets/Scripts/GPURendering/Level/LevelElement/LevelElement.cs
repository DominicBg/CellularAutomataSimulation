using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LevelContainer))]
public abstract class LevelElement : MonoBehaviour
{
    //References
    protected Map map;

    public bool isEnable = true;
    public bool isVisible = true;


    public virtual void Init(Map map)
    {
        this.map = map;
    }

    public abstract void OnUpdate(ref TickBlock tickBlock);
    public abstract void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock);

    public virtual void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }
    public virtual void PostRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock) { }
    public virtual void OnRenderUI(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {

    }

    public virtual void Dispose()
    {

    }
}
