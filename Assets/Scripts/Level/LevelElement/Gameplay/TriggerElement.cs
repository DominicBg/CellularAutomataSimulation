using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class TriggerElement : LevelObject
{
    public int2 sizes;
    public bool triggerOnce;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    private bool isInCollision;
    private bool hasTrigger;
    public override Bound GetBound()
    {
        return Bound.CenterAligned(position, sizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if (hasTrigger && triggerOnce)
            return;

        if(CollideWith(player))
        {
            if (!isInCollision)
                onTriggerEnter.Invoke();
            else
                onTriggerStay.Invoke();

            hasTrigger = true;
            isInCollision = true;
        }
        else if(isInCollision)
        {
            onTriggerExit.Invoke();
            isInCollision = false;
        }
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(renderPos, sizes), Color.magenta * 0.75f);
    }
}
