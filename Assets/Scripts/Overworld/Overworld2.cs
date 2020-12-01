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
    //public float distanceThreshold;
    //public float3 lightPosition;
    //public float baseLight = 0.2f;

    public PlanetRayMarchingSettings settings;


    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        //GetBackgroundFromTexture(out backgroundColors, backgroundTexture);

        GridRenderer.GetBlankTexture(out backgroundColors);

        //NativeArray<float> distances = new NativeArray<float>(GameManager.GridLength, Allocator.TempJob);
        //NativeArray<float3> normals = new NativeArray<float3>(GameManager.GridLength, Allocator.TempJob);

        new PlanetRayMarchingJob()
        {
            gridSizes = GameManager.GridSizes,
            outputColor = backgroundColors,
            settings = settings,
            tickBlock = tickBlock,
        }.Schedule(GameManager.GridLength, 100).Complete();

        //var handle = new RayMarchingJob()
        //{
        //    distanceFunction = RayMarchingHelper.RotatingBoxDistancePointer,
        //    outputNormals = normals,
        //    outputDistances = distances,
        //    gridSizes = GameManager.GridSizes,
        //    settings = settings,
        //    tickBlock = tickBlock
        //}
        //.Schedule(GameManager.GridLength, 32); //.Complete();

        //handle = new RayMarchingColoringJob()
        //{
        //    baseLight = baseLight,
        //    distances = distances,
        //    distanceThreshold = distanceThreshold,
        //    lightPosition = lightPosition,
        //    normals = normals,
        //    outputColor = backgroundColors
        //}.Schedule(GameManager.GridLength, 32, handle);

        //handle.Complete();

        //distances.Dispose();
        //normals.Dispose();
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
