using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static RayMarchingPrimitive;

[BurstCompile]
public struct GemRayMarchingJob : IJobParallelFor
{
    public TickBlock tickBlock;

    public Settings settings;
    public NativeArray<Color32> outputColor;
    public NativeArray<float3> normals;

 
    const float derivativeDelta = 0.0001f;

    [System.Serializable]
    public struct Settings
    {
        public float cameraDepth;
        public float2 scales;
        public float3 lightPosition;
        public float speed;

        public float distanceThreshold;
        public int maxStep;
        public float stopThreshold;

        public float3 diamondPosition;
        public float3 octaherdronOffset;
        public float3 axis;
        public float diamonh;
        public float boxAngle;
        public float3 cubeCutout;
        public float cubeHeight;
        public Color color;
        public float baseIntensity;

        public float shineIntensity;
        public int resolution;

        public Color colorForDepthMin;
        public Color colorForDepthMax;
        public float blendDepthRatio;
        public BlendingMode blendDepth;
        public int depthMaxStep;
        public float depthStepSize;
        public float depthMinSize;
        public float depthMaxSize;

    }

    struct Result
    {
        public float3 pos;
        public float distance;
        public int numberSteps;
        public float3 normal;
        public float depth;
    }

    float DistanceFunction(float3 position, float t)
    {
        position = Translate(position, settings.diamondPosition);
        position = RotateAroundAxisUnsafe(position, settings.axis, t);

        float3 pyramidPos = position;
        //Mirror pyramid on the xz plane
        pyramidPos.y = math.abs(pyramidPos.y);
        float pyramidBottom1 = sdPyramid(pyramidPos, settings.diamonh);
        float pyramidBottom2 = sdPyramid(RotateYQuater(pyramidPos), settings.diamonh);

        float3 boxPos = position;
        float cutoutCube = sdBox(boxPos + math.up() * settings.cubeHeight, settings.cubeCutout);
        return math.max(cutoutCube, math.max(pyramidBottom1, pyramidBottom2));
    }

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        float2 uv = ((float2)gridPosition / GameManager.GridSizes - 0.5f) / settings.scales;
        float3 ro = new float3(uv.x, uv.y, settings.cameraDepth);
        float3 rd = new float3(0, 0, 1);

        //precompute
        settings.axis = math.normalize(settings.axis);

        Result result = RayMarch(ro, rd);

        Color colorFront = CalculateColor(outputColor[index], result).ReduceResolution(settings.resolution);
        //colorBack.a = settings.backBlend;
        normals[index] = result.normal;
        outputColor[index] = colorFront;
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

    Result RayMarch(float3 ro, float3 rd)
    {
        Result result = new Result();

        float3 currentPosition = ro;
        float currentDistance = 0;
        float t = tickBlock.tick * settings.speed;

        int i;
        for (i = 0; i < settings.maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if(distance < settings.stopThreshold)
            {
                break;
            }
        }

        result.numberSteps = i;
        result.pos = currentPosition;
        result.distance = currentDistance;
        result.normal = GetNormal(currentPosition);
        result.depth = CalculateDepth(currentPosition, rd, t); ;
        return result;
    }

    float CalculateDepth(float3 currentPos, float3 rd, float t)
    {
        float depth = 0;
        for (int i = 0; i < settings.depthMaxStep; i++)
        {
            currentPos += rd * settings.depthStepSize;

            float distance = DistanceFunction(currentPos, t);
            if(distance >= 0)
            {
                return depth;
            }
            depth += settings.depthStepSize;
        }
        return depth;
    }

    Color CalculateColor(Color currentColor, Result result)
    {
        if (result.distance < settings.distanceThreshold)
        {
            float lightT = math.saturate(math.dot(math.normalize(settings.lightPosition), result.normal) + settings.baseIntensity);
            float depthRatio = result.depth / settings.depthMaxSize;
            //float depthT = math.saturate(math.remap(settings.depthMinSize, settings.depthMaxSize, 0, 1, depthRatio));
            Color lightColor = lightT  * settings.color;
            // depthcolor = Color.Lerp(settings.colorForDepthMin, settings.colorForDepthMax, depthT);
            //depthcolor.a = settings.blendDepthRatio;
            currentColor.a = depthRatio;
            return RenderingUtils.Blend(lightColor, currentColor, settings.blendDepth);
        }
        else if (result.numberSteps > settings.maxStep * settings.shineIntensity)
        {
            float t = math.remap(settings.maxStep * settings.shineIntensity, settings.maxStep, 0, 1, result.numberSteps);
            return t * settings.color;
        }
        return Color.clear;
    }
}
