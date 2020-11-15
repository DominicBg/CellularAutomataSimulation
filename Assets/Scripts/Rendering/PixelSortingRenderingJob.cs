using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PixelSortingRenderingJob : IJob
{
    public NativeQueue<PixelSortingRenderingSettings> requestQueue;
    public NativeArray<Color32> colorArray;
    public int2 mapSizes;
    public void Execute()
    {
        while(requestQueue.Count > 0)
        {
            PixelSortingRenderingSettings settings = requestQueue.Dequeue();
            ApplyPixelSorting(ref settings);
        }
    }

    void ApplyPixelSorting(ref PixelSortingRenderingSettings settings)
    {
        ref Bound bound = ref settings.bound;

        NativeArray<int> positions;

        //Rotate the sorting 
        if (settings.yx)
            bound.GetPositionsIndexArrayYX(out positions, mapSizes, Allocator.Temp);
        else
            bound.GetPositionsIndexArrayXY(out positions, mapSizes, Allocator.Temp);


        NativeArray<Color32> boundColorArray = new NativeArray<Color32>(positions.Length, Allocator.Temp);

        for (int i = 0; i < boundColorArray.Length; i++)
        {
            boundColorArray[i] = colorArray[positions[i]];
        }

        Sort(boundColorArray, settings.isDescending, settings.order);

        for (int i = 0; i < boundColorArray.Length; i++)
        {
            int positionIndex = positions[i];
            colorArray[positionIndex] = boundColorArray[i];
        }

        positions.Dispose();
        boundColorArray.Dispose();
    }

    void Sort(NativeArray<Color32> boundColorArray, bool isDescending, PixelSortingRenderingSettings.Order order)
    {
        //todo implement more than selection sort lol
        for (int i = 0; i < boundColorArray.Length; i++)
        {
            //In isDescending, min = max
            int minIndex = i;
            float minValue = GetValue(boundColorArray[i], order);

            for (int j = i + 1; j < boundColorArray.Length; j++)
            {
                float value = GetValue(boundColorArray[j], order);
                
                if(isDescending)
                {
                    if (value > minValue)
                    {
                        minValue = value;
                        minIndex = j;
                    }
                }
                else //if asc
                {
                    if (value < minValue)
                    {
                        minValue = value;
                        minIndex = j;
                    }
                }

            }

            //swap
            Color32 tempColor = boundColorArray[minIndex];
            boundColorArray[minIndex] = boundColorArray[i];
            boundColorArray[i] = tempColor;
        }
    }

    float GetValue(Color32 color, PixelSortingRenderingSettings.Order order)
    {
        switch (order)
        {
            case PixelSortingRenderingSettings.Order.Addition:
                return color.r + color.b + color.g;
            case PixelSortingRenderingSettings.Order.Luminance:
                return RenderingUtils.Luminance(color);
        }
        //default is luminance
        return RenderingUtils.Luminance(color);
    }
}
