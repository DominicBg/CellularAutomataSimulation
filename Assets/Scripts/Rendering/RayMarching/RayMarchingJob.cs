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

[BurstCompile]
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
        int2 gridPosition = ArrayHelper.IndexToPos(index, gridSizes);

        float2 uv = ((float2)gridPosition / gridSizes -0.5f) / settings.scales;

        float3 ro = new float3(uv.x, uv.y, 0);
        float3 rd = new float3(0, 0, 1);

        //numberStep might used for effects
        float distance = RayMarch(ro, rd, out int numberStep);
        outputDistances[index] = distance;

        float3 position = ro + rd * distance;
        outputNormals[index] = GetNormal(position);
    }


    float3 GetNormal(float3 position)
    {
        float dt = settings.derivatieDelta;

        float d = distanceFunction.Invoke(position.x, position.y, position.z, tickBlock.tick * settings.speed);
        float dx = distanceFunction.Invoke(position.x + dt, position.y, position.z, tickBlock.tick * settings.speed);
        float dy = distanceFunction.Invoke(position.x, position.y + dt, position.z, tickBlock.tick * settings.speed);
        float dz = distanceFunction.Invoke(position.x, position.y, position.z + dt, tickBlock.tick * settings.speed);

        return math.normalize(new float3(dx, dy, dz) - d);
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
