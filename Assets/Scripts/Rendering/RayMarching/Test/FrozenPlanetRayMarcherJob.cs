using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static RayMarchingPrimitive;

[BurstCompile]
public struct FrozenPlanetRayMarchingJob: IJobParallelFor
{
    public int2 gridSizes;
    public TickBlock tickBlock;

    public Settings settings;
    public ParticleRendering particleRendering;
    public NativeArray<Color32> outputColor;

    const int maxStep = 100;
    const float threshold = 0.01f;
    const float derivativeDelta = 0.0001f;

    [System.Serializable]
    public struct Settings
    {
        public int resolution;

        public float2 scales;
        public float3 lightPosition;
        public float speed;
        public float distanceThreshold;

        public float3 planetPosition;
        public float planetRadius;
        public float3 rotationAxis;

        public float3 conePosition;
        public float2 coneC;
        public float coneH;

        public float3 coneNormal1;
        public float3 coneNormal2;

        public float smoothk;


    }

    struct Result
    {
        public float3 pos;
        public float distance;
        public int numberSteps;
        public float3 normal;
    }

    float DistanceFunction(float3 position, float t)
    {
        position += settings.planetPosition;
        position = RotateAroundAxis(position, settings.rotationAxis, t);

        float3 conePos = settings.conePosition * settings.planetRadius;

        float3 conePos1 = Transform(conePos, quaternion.LookRotation(math.normalize(settings.coneNormal1), math.up()));
        float3 conePos2 = Transform(conePos, quaternion.LookRotation(math.normalize(settings.coneNormal2), math.up()));

        float cone1 = sdCone(position + conePos1, settings.coneC, settings.coneH);
        float cone2 = sdCone(position + conePos2, settings.coneC, settings.coneH);

        float coneSmooth = expsmin(cone1, cone2, settings.smoothk);

        float planet = sdSphere(position, settings.planetRadius);
        return expsmin(planet, coneSmooth, settings.smoothk);
    }

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        float2 uv = ((float2)gridPosition / gridSizes - 0.5f) / settings.scales;
        Result result = RayMarch(uv);
        outputColor[index] = CalculateColor(result).ReduceResolution(settings.resolution);
    }

    float3 GetNormal(float3 position)
    {
        float dt = derivativeDelta;

        float t = tickBlock.tick * settings.speed;
        float d = DistanceFunction(position, t);
        float dx = DistanceFunction(position + new float3(dt, 0, 0), t);
        float dy = DistanceFunction(position + new float3(0, dt, 0), t);
        float dz = DistanceFunction(position + new float3(0, 0, dt), t);

        return math.normalize(new float3(dx, dy, dz) - d);
    }

    Result RayMarch(float2 uv)
    {
        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = new float3(0, 0, 1);

        Result result = new Result();

        float3 currentPosition = ro;
        float currentDistance = 0;
        float t = tickBlock.tick * settings.speed;

        int i;
        for (i = 0; i < maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if (distance < threshold)
            {
                break;
            }
        }

        result.numberSteps = i;
        result.pos = currentPosition;
        result.distance = currentDistance;
        result.normal = GetNormal(currentPosition);
        return result;
    }

    Color CalculateColor(Result result)
    {
        if (result.distance < settings.distanceThreshold)
        {
            float t = math.saturate(math.dot(math.normalize(settings.lightPosition), result.normal));
            return t * Color.white;
        }
        return Color.clear;
    }
}
