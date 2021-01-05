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
    public float3 position;
    public float3 boxBound;
    public float objectSize;
    public float twist;

    public float lightFactor;

    [Header("replicate")]
    public float cMin;
    public float cMax;
    public float l;
    public float shadowAlpha;
    public ParticleType particleType;
    public BlendingMode blendingMode;
    public int colorResolution;
}

[BurstCompile]
public struct PlanetRayMarchingJob : IJobParallelFor
{
    public int2 gridSizes;
    public TickBlock tickBlock;

    public PlanetRayMarchingSettings settings;
    public ParticleRendering particleRendering;
    public NativeArray<Color32> outputColor;
    [ReadOnly] public NativeArray<LightSource> lightSources;

    const int maxStep = 100;
    const float threshold = 0.01f;

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        float2 uv = ((float2)gridPosition / gridSizes - 0.5f) / settings.scales;
        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = (settings.ortho) ? new float3(0, 0, 1) : math.normalize(new float3(uv.x, uv.y, 1));

        //numberStep might used for effects
        float distance = RayMarch(ro, rd, out int numberStep);

        float3 position = ro + rd * distance;
        float3 normal = GetNormal(position);
        Color shadowcolor = CalculateColor(distance, position, normal, numberStep);

        if (shadowcolor.a != 0)
        {
            shadowcolor.ReduceResolution(settings.colorResolution);
            shadowcolor.a = settings.shadowAlpha;
            //eww
            Map map = new Map();
            Color iceColor = ParticleRenderUtil.GetColorForType(gridPosition, settings.particleType, ref particleRendering, ref tickBlock, ref map, lightSources);
            outputColor[index] = RenderingUtils.Blend(iceColor, shadowcolor, settings.blendingMode);
        }
    }

    float DistanceFunction(float3 position, float t)
    {
        float distBoxNice = BoxNice(position, t);
        //float distSphere = Sphere(position, t);
        return distBoxNice;
    }

    float BoxNice(float3 position, float t)
    {
        position = RayMarchingPrimitive.Translate(position, settings.position);
        position = RayMarchingPrimitive.RotateAroundAxis(position, settings.rotationAxis, t);

        float c = math.remap(-1, 1, settings.cMin, settings.cMax, math.sin(t));
        if (c != 0 && settings.l != 0)
        {
            position = RayMarchingPrimitive.opRepLim(position, c, settings.l);
        }
        position = RayMarchingPrimitive.opTwist(position, settings.twist);
        return RayMarchingPrimitive.sdBox(position, settings.objectSize);
    }

    //float Sphere(float3 position, float t)
    //{
    //    position = RayMarchingPrimitive.Translate(position, settings.position);
    //    //position = RayMarchingPrimitive.RotateAroundAxis(position, settings.rotationAxis, t);

    //    float c = math.remap(-1, 1, settings.cMin, settings.cMax, math.sin(t));
    //    if (c != 0 && settings.l != 0)
    //    {
    //        position = RayMarchingPrimitive.opRepLim(position, c, settings.l);
    //    }
    //    position = RayMarchingPrimitive.opTwist(position, settings.twist);
    //    return RayMarchingPrimitive.sdSphere(position, settings.sphereRadius);
    //}

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

    Color CalculateColor(float distance, float3 position, float3 normal, int numberOfStep)
    {
        if (distance < settings.distanceThreshold)
        {
            float t = math.saturate(math.dot(math.normalize(settings.lightPosition), normal));
            return (t + settings.baseLight) * Color.white;
        }
        else if(numberOfStep > maxStep * settings.lightFactor)
        {
            float t = math.remap(maxStep * settings.lightFactor, maxStep, 0, 1, numberOfStep);
            return t * Color.white;
        }
        return Color.clear;
    }
}
