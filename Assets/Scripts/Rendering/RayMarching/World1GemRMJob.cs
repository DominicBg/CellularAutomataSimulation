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

    public CameraTransform camera;
    public DiamondSettings diamond;
    public RenderSettings render;
    public PillarSettings pillar;
    public LightSettings light;
    public NativeArray<Color32> outputColor;

    public NativeArray<float3> normals;
    public NativeArray<Color32> edgeColor;

 
    const float derivativeDelta = 0.0001f;
    [System.Serializable]
    public struct CameraTransform
    {
        public float3 pitchYawRoll;
        public float3 lookAtPosition;
        public float distance;
    }

    [System.Serializable]
    public struct RenderSettings
    {
        public float cameraDepth;
        public float2 scales;
        //public float3 lightPosition;
        public float distanceThreshold;
        public float speed;

        public int maxStep;
        public float stopThreshold;

        //public Color color;
        //public float baseIntensity;

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
        [Header("Transform")]
        public float3 diamondPosition;
        public float3 axis;
        public float3 pivot;
        public float height;
        public float boxAngle;
        public float3 cubeCutoutSizes;
        public float cubeCutoutY;

        [Header("Colors")]
        public Color32 edgeColor;
        public Color4Dither color4Dither;
        public const int materialID = 1;
        public Color shineColor;
    }

    [System.Serializable]
    public struct PillarSettings
    {
        [Header("Transform")]
        public float3 boxTopSize;
        public float3 boxTopPosition;

        public float3 boxPillarSize;
        public float3 boxPillarPosition;

        public float3 boxBottom1Size;
        public float3 boxBottom1Position;

        public float3 boxBottom2Size;
        public float3 boxBottom2Position;

        [Header("Colors")]
        public Color4Dither color4Dither;
        public const int materialID = 2;
    }

    [System.Serializable]
    public struct LightSettings
    {
        public float3 position;
        public float intensity;
        public Color color;
        public bool normalize;
        public bool useSquare;
    }

    struct Result
    {
        public float3 pos;
        public float distance;
        public int numberSteps;
        public float3 normal;
        public float depth;
        public int materialID;
    }

    float DistanceFunction(float3 position, float t)
    {
        float diamond = DiamondDistance(position, t);
        float pillar = PillarDistance(position, t);
        return math.min(diamond, pillar);
    }

    int GetMaterialID(float3 position, float t)
    {
        float diamond = DiamondDistance(position, t);
        float pillar = PillarDistance(position, t);
        return (diamond < pillar) ? DiamondSettings.materialID : PillarSettings.materialID;
    }

    float DiamondDistance(float3 position, float t)
    {
        position = Translate(position, diamond.diamondPosition);
        position = RotateAroundAxisUnsafe(position - diamond.pivot, diamond.axis, t);
        position += diamond.pivot;

        float3 pyramidPos = position;
        //Mirror pyramid on the xz plane
        pyramidPos.y = math.abs(pyramidPos.y);
        float pyramidBottom1 = sdPyramid(pyramidPos, diamond.height);
        float pyramidBottom2 = sdPyramid(RotateYQuater(pyramidPos), diamond.height);

        float3 boxPos = position;
        float cutoutCube = sdBox(boxPos + math.up() * diamond.cubeCutoutY, diamond.cubeCutoutSizes);
        return math.max(cutoutCube, math.max(pyramidBottom1, pyramidBottom2));
    }

    float PillarDistance(float3 position, float t)
    {
        float boxTop = sdBox(position - pillar.boxTopPosition, pillar.boxTopSize);
        float boxPillar = sdBox(position - pillar.boxPillarPosition, pillar.boxPillarSize);
        float boxBottom1 = sdBox(position - pillar.boxBottom1Position, pillar.boxBottom1Size);
        float boxBottom2 = sdBox(position - pillar.boxBottom2Position, pillar.boxBottom2Size);

        return math.min(boxTop, math.min(boxPillar, math.min(boxBottom1, boxBottom2)));
    }

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

        float2 uv = ((float2)gridPosition / GameManager.GridSizes - 0.5f) / render.scales;
        quaternion fromLookAt = quaternion.Euler(math.radians(camera.pitchYawRoll));

        float3 dir = math.mul(fromLookAt, new float3(0, 0, 1));

        float3 position = camera.lookAtPosition + dir * camera.distance;
        quaternion rotation = quaternion.LookRotation(-dir, math.up());
        float3 ro = position + math.mul(rotation, new float3(uv.x, uv.y, 0));
        float3 rd = math.mul(rotation, new float3(0, 0, 1)); 

        //precompute
        diamond.axis = math.normalize(diamond.axis);

        Result result = RayMarch(ro, rd);

        Color colorFront = CalculateColor(gridPosition, outputColor[index], ref result).ReduceResolution(render.resolution);

        normals[index] = result.normal;
        edgeColor[index] = GetEdgeColor(ref result);

        outputColor[index] = colorFront;
    }

    float3 GetNormal(float3 position)
    {
        float dt = derivativeDelta;

        float t = tickBlock.tick * render.speed;
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
        float t = tickBlock.tick * render.speed;

        int i;
        for (i = 0; i < render.maxStep; i++)
        {
            float distance = DistanceFunction(currentPosition, t);
            currentDistance += distance;
            currentPosition += rd * distance;

            if(distance < render.stopThreshold)
            {
                break;
            }
        }

        result.numberSteps = i;
        result.pos = currentPosition;
        result.distance = currentDistance;
        result.normal = GetNormal(currentPosition);
        result.depth = CalculateDepth(currentPosition, rd, t);
        result.materialID = GetMaterialID(currentPosition, t);
        return result;
    }

    float CalculateDepth(float3 currentPos, float3 rd, float t)
    {
        float depth = 0;
        for (int i = 0; i < render.depthMaxStep; i++)
        {
            currentPos += rd * render.depthStepSize;

            float distance = DistanceFunction(currentPos, t);
            if(distance >= 0)
            {
                return depth;
            }
            depth += render.depthStepSize;
        }
        return depth;
    }

    Color CalculateColor(int2 gridPosition, Color currentColor, ref Result result)
    {
        switch(result.materialID)
        {
            case DiamondSettings.materialID: return CalculateDiamondColor(gridPosition, currentColor, ref result);
            case PillarSettings.materialID: return CalculatePillarColor(gridPosition, ref result);
        }
     
        return Color.clear;
    }

    Color CalculateDiamondColor(int2 gridPosition, Color currentColor, ref Result result)
    {
        if (result.distance < render.distanceThreshold)
        {
            float lightT = CalculateLight(result.pos, result.normal);
            float depthRatio = result.depth / render.depthMaxSize;
            //Color lightColor = lightT  * render.color;
            currentColor.a = depthRatio;
            //return RenderingUtils.Blend(lightColor, currentColor, render.blendDepth);
            return diamond.color4Dither.GetColorWitLightValue(lightT, gridPosition);
        }
        else if (result.numberSteps > render.maxStep * render.shineIntensity)
        {
            float t = math.remap(render.maxStep * render.shineIntensity, render.maxStep, 0, 1, result.numberSteps);
            return t * diamond.shineColor;
        }
        return Color.clear;
    }

    Color CalculatePillarColor(int2 gridPosition, ref Result result)
    {
        if (result.distance < render.distanceThreshold)
        {
            float lightT = CalculateLight(result.pos, result.normal);
            Color lightColor = pillar.color4Dither.GetColorWitLightValue(lightT, gridPosition);
            return lightColor;
        }
        return Color.clear;
    }

    float CalculateLight(float3 point, float3 normal)
    {
        float3 lightDiff = light.position - point;
        float dist = math.length(lightDiff);
        float3 lightDir = lightDiff / dist;
        
        if(light.useSquare)
            dist = dist * dist;

        float intensity = math.dot(lightDir, normal);
        if (light.normalize)
            intensity = MathUtils.unorm(intensity);

        intensity = math.saturate(intensity);

        //add color??
        return intensity * light.intensity / dist;
        //return/* math.saturate(MathUtils.unorm(math.dot(lightDir, normal)) + render.baseIntensity)*/;
    }

    Color32 GetEdgeColor(ref Result result)
    {
        switch (result.materialID)
        {
            case DiamondSettings.materialID: return diamond.edgeColor;
        }

        return Color.clear;
    }
}
