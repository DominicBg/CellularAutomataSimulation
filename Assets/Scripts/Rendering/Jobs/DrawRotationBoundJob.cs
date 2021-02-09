using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct RenderRotationBound : IJobParallelFor
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
