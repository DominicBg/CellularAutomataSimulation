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
        public int2 loopoffset;
        public float cubeScalarDiff;
        public EaseXVII.Ease ease;
    }

    struct Result
    {
        public float3 pos;
        public float distance;
        public int numberSteps;
        public float3 normal;
    }

    int cubeRotation;
    float cubeScalar;

    float DistanceFunction(float3 position, float t)
    {
        position -= new float3(0, 0, settings.cubeZ);
        position = RayMarchingPrimitive.opRep(position, settings.infinity);

        position = RayMarchingPrimitive.RotateY(position, t);
        position = RayMarchingPrimitive.RotateYQuater(position, cubeRotation);
        position = RayMarchingPrimitive.RotateXQuater(position);
        position = RayMarchingPrimitive.RotateZQuater(position);

        float distance = RayMarchingPrimitive.sdBox(position, settings.cubeSizes * cubeScalar);
        return distance;
    }

    float2 parallaxOffset; // = (int2)((float2)cameraPos * settings.parallaxSpeed);
    public void Execute(int index)
    {
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        //light doesnt reflect in the background anymore rip
        parallaxOffset = ((float2)cameraPos * settings.parallaxSpeed);
        int2 pixelPos = gridPosition - GameManager.GridSizes/2 + (int2)parallaxOffset;
        int2 modPixelPos = (int2)math.step(settings.scales, ((pixelPos + settings.loopoffset) % (settings.scales * 2)));
        cubeRotation = (modPixelPos.x + modPixelPos.y) % 4;

        cubeScalar = cubeRotation % 2 == 0 ? 1 : settings.cubeScalarDiff;

        float3 ro = new float3(pixelPos / settings.scales, 0);
        float3 rd = new float3(0,0,1);

        Result result = RayMarch(ro, rd);
        outputColor[index] = RenderingUtils.Blend(outputColor[index], CalculateColor(result, gridPosition), settings.blending);
    }

    float3 GetNormal(float3 position)
    {
        float dt = derivativeDelta;

        float t = GetTime();
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
        float t = GetTime();

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
            //int2 normalizedGridPos = gridPos * 2 - gridSizes;
            //Currently have a slight offset
            //float2 cameraAndParallaxOffset = cameraPos - parallaxOffset;
            float3 pos = new float3(gridPos + cameraPos - parallaxOffset, result.pos.z * settings.cubeZLight);

            for (int i = 0; i < lights.Length; i++)
            {
                intensity += lights[i].GetLightIntensity(pos, -result.normal);
            }
            intensity = math.saturate(intensity);
            return settings.color.GetColorWitLightValue(intensity, gridPos);
        }
        return Color.clear;
    }

    private float GetTime()
    {
        float pi2 = math.PI * 2;
        float x = (tickBlock.tick * settings.speed) % pi2;
        x /= pi2;
        return EaseXVII.Evaluate(x, settings.ease) * pi2;
    }
}
