using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PixelCameraTransform : LevelObject
{
    public LevelObject target;
    public int2 focusSizes = 25;
    public int snapMaxDistance = 2;
    public float elasticSmooth = 5;
    public int2 boundOffset;
    public int2 offset;

    public override void OnInit()
    {
        target = FindObjectOfType<Player>();
        position = target.position;
    }


    public override Bound GetBound()
    {
        return Bound.CenterAligned(position - boundOffset, focusSizes);
    }

    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        int2 targetCenter = target.GetBound().center;
        int2 closestPoint = GetBound().ProjectPointOnbound(targetCenter);
        if (math.any(closestPoint != 0))
        {
            int2 diff = targetCenter - closestPoint;

            if(math.any(math.abs(diff) > snapMaxDistance))
            {
                position = (int2)math.lerp(position, targetCenter, GameManager.DeltaTime * elasticSmooth);
            }
            else 
            {
                position += diff;
            
            }
        }
    }


    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(renderPos - boundOffset, focusSizes), Color.cyan * 0.35f, BlendingMode.Transparency);
    }
}
