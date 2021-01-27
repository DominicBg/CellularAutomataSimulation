using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CavernOfTimeBackgroundJob : IJobParallelFor
{
    public int2 gridSizes;
    public TickBlock tickBlock;

    public Settings settings;
    public NativeArray<Color32> outputColor;
    public int2 cameraPos;
    [ReadOnly] public NativeList<LightSource> lights;

    const float threshold = 0.01f;
    const float derivativeDelta = 0.0001f;
    

    [System.Serializable]
    public struct Settings
    {
        public float2 scales;
        public float speed;
        public float distanceThreshold;
        public bool isOrthographic;
        public float cubeZ;
        public float cubeZLight;
        public float3 cubeSizes;
        public Color4Dither color;
        public float3 infinity;
        public float maxStep;
        public float parallaxSpeed;
        public BlendingMode blending;
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
        position -= new float3(0, 0, settings.cubeZ);
        position = RayMarchingPrimitive.opRep(position, settings.infinity);

        position = RayMarchingPrimitive.RotateY(position, t * settings.speed);
        position = RayMarchingPrimitive.RotateXQuater(position);
        position = RayMarchingPrimitive.RotateZQuater(position);

        float distance = RayMarchingPrimitive.sdBox(position, settings.cubeSizes);
        return distance;
    }

    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        float2 uv = ((float2)gridPosition / gridSizes - 0.5f) / settings.scales;
        Result result = RayMarch(uv);
        outputColor[index] = RenderingUtils.Blend(outputColor[index], CalculateColor(result, gridPosition), settings.blending);
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
        RayMarchingPrimitive.view(uv, settings.isOrthographic, out float3 ro, out float3 rd);
        ro += new float3(cameraPos.x, cameraPos.y, 0) * settings.parallaxSpeed;

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

            if(distance < threshold)
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


    Color CalculateColor(Result result, int2 gridPos)
    {
        if (result.distance < settings.distanceThreshold)
        {
            float intensity = 0;
            int2 normalizedGridPos = gridPos * 2 - gridSizes;
            //Currently have a slight offset
            float3 pos = new float3(cameraPos + normalizedGridPos, result.pos.z * settings.cubeZLight); //+ new float3((float2)cameraPos * (settings.parallaxSpeed / 2), 0);
            for (int i = 0; i < lights.Length; i++)
            {
                intensity += lights[i].GetLightIntensity(pos, -result.normal);
            }
            intensity = math.saturate(intensity);
            return settings.color.GetColorWitLightValue(intensity, gridPos);
        }
        return Color.clear;
    }
}
