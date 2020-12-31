using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static RayMarchingPrimitive;

[BurstCompile]
public struct World1GemRMJob : IJobParallelFor
{
    public TickBlock tickBlock;

    public DiamondSettings dsettings;
    public CameraSettings csettings;
    public NativeArray<Color32> outputColor;
    public NativeArray<float3> normals;

 
    const float derivativeDelta = 0.0001f;

    [System.Serializable]
    public struct CameraSettings
    {
        public float cameraDepth;
        public float2 scales;
        public float3 lightPosition;
        public float distanceThreshold;
        public float speed;

        public int maxStep;
        public float stopThreshold;

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

    [System.Serializable]
    public struct DiamondSettings
    {
        public float3 diamondPosition;
        public float3 axis;
        public float height;
        public float boxAngle;
        public float3 cubeCutoutSizes;
        public float cubeCutoutY;
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
        float diamond = DiamondDistance(position, t);

        return diamond;

    }

    float DiamondDistance(float3 position, float t)
    {
        position = Translate(position, dsettings.diamondPosition);
        position = RotateAroundAxisUnsafe(position, dsettings.axis, t);

        float3 pyramidPos = position;
        //Mirror pyramid on the xz plane
        pyramidPos.y = math.abs(pyramidPos.y);
        float pyramidBottom1 = sdPyramid(pyramidPos, dsettings.height);
        float pyramidBottom2 = sdPyramid(RotateYQuater(pyramidPos), dsettings.height);

        float3 boxPos = position;
        float cutoutCube = sdBox(boxPos + math.up() * dsettings.cubeCutoutY, dsettings.cubeCutoutSizes);
        return math.max(cutoutCube, math.max(pyramidBottom1, pyramidBottom2));
    }

    float PillarDistance(float3 position, float t)
    {
        return 0;
    }

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        float2 uv = ((float2)gridPosition / GameManager.GridSizes - 0.5f) / csettings.scales;
        float3 ro = new float3(uv.x, uv.y, csettings.cameraDepth);
        float3 rd = new float3(0, 0, 1);

        //precompute
        dsettings.axis = math.normalize(dsettings.axis);

        Result result = RayMarch(ro, rd);

        Color colorFront = CalculateColor(outputColor[index], result).ReduceResolution(csettings.resolution);
        //colorBack.a = settings.backBlend;
        normals[index] = result.normal;
        outputColor[index] = colorFront;
    }

    float3 GetNormal(float3 position)
    {
        float dt = derivativeDelta;

        float t = tickBlock.tick * csettings.speed;
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
        float t = tickBlock.tick * csettings.speed;

        int i;
        for (i = 0; i < csettings.maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if(distance < csettings.stopThreshold)
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
        for (int i = 0; i < csettings.depthMaxStep; i++)
        {
            currentPos += rd * csettings.depthStepSize;

            float distance = DistanceFunction(currentPos, t);
            if(distance >= 0)
            {
                return depth;
            }
            depth += csettings.depthStepSize;
        }
        return depth;
    }

    Color CalculateColor(Color currentColor, Result result)
    {
        if (result.distance < csettings.distanceThreshold)
        {
            float lightT = math.saturate(math.dot(math.normalize(csettings.lightPosition), result.normal) + csettings.baseIntensity);
            float depthRatio = result.depth / csettings.depthMaxSize;
            Color lightColor = lightT  * csettings.color;
            currentColor.a = depthRatio;
            return RenderingUtils.Blend(lightColor, currentColor, csettings.blendDepth);
        }
        else if (result.numberSteps > csettings.maxStep * csettings.shineIntensity)
        {
            float t = math.remap(csettings.maxStep * csettings.shineIntensity, csettings.maxStep, 0, 1, result.numberSteps);
            return t * csettings.color;
        }
        return Color.clear;
    }
}
