using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(CompileSynchronously = true)]
public static class RayMarchingHelper
{
    [BurstCompile(CompileSynchronously = true)]
    public static float SphereDistanceFunction(float x, float y, float z, float t)
    {
        float3 spherePosition = new float3(0, 0, 15);
        float3 position = new float3(x, y, z);
        float radius = 5;
        return math.distance(position, spherePosition) - radius;
    }

    public static FunctionPointer<CalculateDistancefunction> SphereDistancePointer => BurstCompiler.CompileFunctionPointer<CalculateDistancefunction>(SphereDistanceFunction);


    [BurstCompile(CompileSynchronously = true)]
    public static float BoxDistanceFunction(float x, float y, float z, float t)
    {
        float3 box = 2;
        float3 p = new float3(x, y, z);
        float3 q = math.abs(p) - box;
        return math.length(math.max(q, 0)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0);
    }

    public static FunctionPointer<CalculateDistancefunction> BoxDistancePointer => BurstCompiler.CompileFunctionPointer<CalculateDistancefunction>(BoxDistanceFunction);





    public delegate float CalculateDistancefunction(float x, float y, float z, float t);
}
