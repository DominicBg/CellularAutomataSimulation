using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Overworld2", menuName = "Overworlds/Overworld 2", order = 1)]
public class Overworld2 : OverworldBase
{
    public Texture2D backgroundTexture;
    public StarBackgroundRendering starSettings;
    public PlanetRayMarchingSettings settings;

    public RayMarcher.Settings testSettings;
    public FrozenPlanetRayMarchingJob.Settings frozenSettings;
    private FunctionPointer<RayMarcher.RaymarcherFunc> func;

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        GridRenderer.GetBlankTexture(out backgroundColors);

        new ShiningStarBackgroundJob()
        {
            colors = backgroundColors,
            maxSizes = GameManager.GridSizes,
            tick = tickBlock.tick,
            settings = starSettings
        }.Schedule(GameManager.GridLength, 100).Complete();

        new PlanetRayMarchingJob()
        {
            gridSizes = GameManager.GridSizes,
            outputColor = backgroundColors,
            settings = settings,
            tickBlock = tickBlock,
            particleRendering = GridRenderer.Instance.particleRendering
        }.Schedule(GameManager.GridLength, 100).Complete();



        //new FrozenPlanetRayMarchingJob()
        //{
        //    gridSizes = GameManager.GridSizes,
        //    outputColor = backgroundColors,
        //    settings = frozenSettings,
        //    tickBlock = tickBlock,
        //    particleRendering = GridRenderer.Instance.particleRendering
        //}.Schedule(GameManager.GridLength, 100).Complete();


        //if (!func.IsCreated)
        //    func = RayMarcher.GetFunctionPointer(FrozenPlanetRayMarchingJob.DistanceFunction);

        //new FrozenPlanetRayMarchingJob()
        //{
        //    tickBlock = tickBlock,
        //    gridSizes = GameManager.GridSizes,
        //    outputColor = backgroundColors,
        //    settings = testSettings,
        //    func = func
        //}.Schedule(GameManager.GridLength, 100).Complete();
    }

    [BurstCompile]
    public struct RayMarchingColoringJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColor;
        public NativeArray<float> distances;
        public NativeArray<float3> normals;
        public float distanceThreshold;
        public float baseLight;
        public float3 lightPosition;

        public void Execute(int i)
        {
            if (distances[i] < distanceThreshold)
            {
                float t = math.dot(math.normalize(lightPosition), normals[i]);
                outputColor[i] = (t + baseLight) * Color.white;
            }
        }
    }
}
