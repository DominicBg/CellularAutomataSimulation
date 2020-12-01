using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PlanetRayMarchingSettings
{
    public float distanceThreshold;
    public float3 lightPosition;
    public float baseLight;

    public float2 scales;
    public float derivatieDelta;
    public float speed;
    public bool ortho;

    [Header("SDF")]
    public float3 rotationAxis;
    public float3 boxBound;
    public float modulo;
    public float3 offset;
    public bool repeat;
}

[BurstCompile]
public struct PlanetRayMarchingJob : IJobParallelFor
{
    public int2 gridSizes;
    public TickBlock tickBlock;

    public PlanetRayMarchingSettings settings;

    public NativeArray<Color32> outputColor;

    const int maxStep = 100;
    const float threshold = 0.01f;


    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        //float2 uv = ((float2)gridPosition / gridSizes -0.5f) / settings.scales;
        float2 uv = settings.repeat ?
            ((float2)gridPosition / gridSizes) / settings.scales :
            ((float2)gridPosition / gridSizes - 0.5f) / settings.scales;

        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = (settings.ortho) ? new float3(0, 0, 1) : math.normalize(new float3(uv.x, uv.y, 1));

        //numberStep might used for effects
        float distance = RayMarch(ro, rd, out int numberStep);

        float3 position = ro + rd * distance;
        float3 normal = GetNormal(position);
        outputColor[index] = CalculateColor(distance, position, normal);

    }

    float DistanceFunction(float3 position, float t)
    {
        if(settings.repeat)
            position = (position  + settings.offset) % settings.modulo - settings.offset;

        quaternion boxRotation = quaternion.AxisAngle(math.normalize(settings.rotationAxis), t);
        //quaternion invBoxRotation = math.inverse(boxRotation);

        position = math.mul(boxRotation, position);

        float3 q = math.abs(position) - settings.boxBound;
        return math.length(math.max(q, 0)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0);
    }

    float3 GetNormal(float3 position)
    {
        float dt = settings.derivatieDelta;

        float t = tickBlock.tick * settings.speed;
        float d = DistanceFunction(position, t);
        float dx = DistanceFunction(position + new float3(dt, 0, 0), t);
        float dy = DistanceFunction(position + new float3(0, dt, 0), t);
        float dz = DistanceFunction(position + new float3(0, 0, dt), t);

        return math.normalize(new float3(dx, dy, dz) - d);
    }

    float RayMarch(float3 ro, float3 rd, out int numberstep)
    {
        float3 currentPosition = ro;
        float currentDistance = 0;
        float t = tickBlock.tick * settings.speed;

        int i;
        for (i = 0; i < maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if(distance < threshold)
            {
                break;
            }
        }

        numberstep = i;
        return currentDistance;
    }

    Color CalculateColor(float distance, float3 position, float3 normal)
    {
        if (distance < settings.distanceThreshold)
        {
            float t = math.saturate(math.dot(math.normalize(settings.lightPosition), normal));
            return (t + settings.baseLight) * Color.white;
        }
        return Color.black;
    }
}
