using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
public struct RayMarchingEdgeDetectorJob : IJobParallelFor
{
    [System.Serializable]
    public struct RayMarchingEdgeDetectorSettings
    {
        public float dotThreshold;
        public BlendingMode blendingMode;
    }

    public NativeArray<Color32> outputColor;
    [ReadOnly] public NativeArray<float3> normals;
    public NativeArray<Color32> edgeColor;

    public RayMarchingEdgeDetectorSettings settings;
    public void Execute(int index)
    {
        int2 position = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);

        bool hardEdge = false;
        hardEdge |= HardEdge(index, position, new int2(1, 0));
        hardEdge |= HardEdge(index, position, new int2(0, 1));

        if(hardEdge)
        {
            outputColor[index] = RenderingUtils.Blend(outputColor[index], edgeColor[index], settings.blendingMode);
        }

    }

    bool HardEdge(int index, int2 position, int2 offset)
    {
        int2 posAdjacent = position + offset;

        if (!GridHelper.InBound(posAdjacent, GameManager.RenderSizes))
            return false;

        int indexAdjacent = ArrayHelper.PosToIndex(posAdjacent, GameManager.RenderSizes);
        return math.dot(normals[index], normals[indexAdjacent]) < settings.dotThreshold;
    }

}
