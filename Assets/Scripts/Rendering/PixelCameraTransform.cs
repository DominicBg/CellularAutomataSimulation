﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCameraTransform : LevelObject
{
    public LevelObject target;
    public int2 focusSizes = 25;

    public override void OnInit()
    {
        position = target.position;
        target = FindObjectOfType<PlayerElement>();
    }

    public override Bound GetBound()
    {
        return Bound.CenterAligned(position, focusSizes);
    }

    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        int2 targetCenter = target.GetBound().center;
        int2 closestPoint = GetBound().ProjectPointOnbound(targetCenter);
        if (math.any(closestPoint != 0))
        {
            int2 diff = targetCenter - closestPoint;
            position += diff;
        }
    }


    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(renderPos, focusSizes), Color.cyan * 0.35f, BlendingMode.Transparency);
    }
}
