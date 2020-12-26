using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class RayMarcher
{
    public delegate float RaymarcherFunc(float x, float y, float z, float t);

    [System.Serializable]
    public struct Settings
    {
        public int maxStep;
        public float2 scales;
        public float distanceThreshold;
        public float derivatieDelta;
        public float speed;
    }

    public struct Result
    {
        public float distance;
        public int numberSteps;
        public float3 normal;
        public float3 position;
    }

    [BurstCompile]
    public static Result RayMarching(int2 gridPosition, int2 gridSizes, ref TickBlock tickBlock, in Settings settings, FunctionPointer<RaymarcherFunc> func)
    {
        float2 uv = ((float2)gridPosition / gridSizes - 0.5f) / settings.scales;
        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = new float3(0, 0, 1);

        Result result = new Result();

        result.distance = RayMarch(ro, rd, tickBlock.tick, settings, out int numberSteps, func);

        result.position = ro + rd * result.distance;

        result.normal = GetNormal(result.position, tickBlock.tick, settings, func);
        result.numberSteps = numberSteps;
        return result;
    }

    [BurstCompile]
    static float3 GetNormal(float3 position, int tick, in Settings settings, FunctionPointer<RaymarcherFunc> func)
    {
        float dt = settings.derivatieDelta;

        float t = tick * settings.speed;
        float d = GetFunctionDistance(position, t, func);

        float dx = GetFunctionDistance(position + new float3(dt, 0, 0), t, func);
        float dy = GetFunctionDistance(position + new float3(0, dt, 0), t, func);
        float dz = GetFunctionDistance(position + new float3(0, 0, dt), t, func);

        return math.normalize(new float3(dx, dy, dz) - d);
    }

    [BurstCompile]
    static float RayMarch(float3 ro, float3 rd, int tick, in Settings settings, out int numberstep, FunctionPointer<RaymarcherFunc> func)
    {
        float3 currentPosition = ro;
        float currentDistance = 0;
        float t = tick * settings.speed;

        int i;
        for (i = 0; i < settings.maxStep; i++)
        {
            float distance = GetFunctionDistance(currentPosition, t, func);
            currentDistance += distance;
            currentPosition += rd * distance;

            if (distance < settings.distanceThreshold)
            {
                break;
            }
        }

        numberstep = i;
        return currentDistance;
    }

    [BurstCompile]
    static float GetFunctionDistance(float3 pos, float t, FunctionPointer<RaymarcherFunc> func)
    {
        return func.Invoke(pos.x, pos.y, pos.z, t);
    }

    public static FunctionPointer<RaymarcherFunc> GetFunctionPointer(RaymarcherFunc func)
    {
        return BurstCompiler.CompileFunctionPointer(func);
    }
}
