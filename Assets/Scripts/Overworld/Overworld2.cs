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
    public RayMarchingSettings settings;
    public float distanceThreshold;

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        //GetBackgroundFromTexture(out backgroundColors, backgroundTexture);
     
        GridRenderer.GetBlankTexture(out backgroundColors);

        NativeArray<float> distances = new NativeArray<float>(GameManager.GridLength, Allocator.TempJob);
        NativeArray<float3> normals = new NativeArray<float3>(GameManager.GridLength, Allocator.TempJob);

        new RayMarchingJob()
        {
            distanceFunction = RayMarchingHelper.BoxDistancePointer,
            outputNormals = normals,
            outputDistances = distances,
            gridSizes = GameManager.GridSizes,
            settings = settings,
            tickBlock = tickBlock
        }
        .Schedule(GameManager.GridLength, 100).Complete();

        //shitty
        for (int i = 0; i < backgroundColors.Length; i++)
        {
            if (distances[i] < distanceThreshold)
               backgroundColors[i] = math.dot(new float3(0,0, -1), normals[i]) * Color.white;
        }

        distances.Dispose();
        normals.Dispose();
    } 
}