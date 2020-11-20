using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

public class GridPostProcess
{
    static ProfilerMarker s_PixelSorting = new ProfilerMarker("GridPostProcess.PixelSorting");


    public static void ApplyPixelSorting(ref NativeArray<Color32> colors, ref PixelSortingSettings settings)
    {
        using (s_PixelSorting.Auto())
        { 
            new PixelSortingRenderingJob()
            {
                colorArray = colors,
                settings = settings,
                mapSizes = GameManager.GridSizes
            }.Run();
        }
    }
}
