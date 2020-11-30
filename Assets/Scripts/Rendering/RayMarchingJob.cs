using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static RayMarchingHelper;

[System.Serializable]
public struct RayMarchingSettings
{
    public float2 scales;
    public float cameraDist;
    public float derivatieDelta;
    public float speed;
}

[BurstCompile(CompileSynchronously = true)]
public struct RayMarchingJob : IJobParallelFor
{
    public FunctionPointer<CalculateDistancefunction> distanceFunction;

    public int2 gridSizes;
    public TickBlock tickBlock;

    public RayMarchingSettings settings;


    public NativeArray<float> outputDistances;
    public NativeArray<float3> outputNormals;

    const int maxStep = 100;
    const float threshold = 0.01f;

    public void Execute(int index)
    {
        int2 position = ArrayHelper.IndexToPos(index, gridSizes);

        float2 uv = ((float2)position / gridSizes -0.5f) / settings.scales;

        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = new float3(0, 0, 1);

        //numberStep might used for effects
        float distance = RayMarch(ro, rd, out int numberStep);
        outputDistances[index] = distance;

        float offX = RayMarch(ro + new float3(settings.derivatieDelta, 0, 0), rd, out int numberStepX);
        float offY = RayMarch(ro + new float3(0, settings.derivatieDelta, 0), rd, out int numberStepY);

        float deltaX = (distance - offX) / settings.derivatieDelta;
        float deltaY = (distance - offY) / settings.derivatieDelta;
        float3 normal = math.normalize(new float3(deltaX, deltaY, -1));
        outputNormals[index] = normal;
    }

    float RayMarch(float3 ro, float3 rd, out int numberstep)
    {
        float3 currentPosition = ro;
        float currentDistance = 0;

        int i;
        for (i = 0; i < maxStep; i++)
        {
            float distance = distanceFunction.Invoke(currentPosition.x, currentPosition.y, currentPosition.z, tickBlock.tick * settings.speed);
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
}
