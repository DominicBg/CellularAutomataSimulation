using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct RenderRotationBoundJob : IJobParallelFor
{
    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;
    public Color color;
    public NativeArray<Color32> outputColors;
    public BlendingMode blending;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(pos);

        if (rotationBound.PointInBound(worldPos))
            outputColors[index] = RenderingUtils.Blend(outputColors[index], color, blending);
    }
}

[BurstCompile]
public struct RenderRotationBoundSpriteJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;

    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;
    public BlendingMode blending;
    public NativeSprite nativeSprite;
    public Color32 tint;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(pos);

        if (rotationBound.PointInBound(worldPos))
        {
            float2 uv = rotationBound.GetUV(worldPos);
            Color sampleColor = (Color)RenderingUtils.SampleTexture(in nativeSprite, uv) * tint;
            //sampleColor = RenderingUtils.Blend(sampleColor, tint, blending);
            //sampleColor.a = tint.a;
            outputColors[index] = RenderingUtils.Blend(outputColors[index], sampleColor, blending);
        }
    }
}
