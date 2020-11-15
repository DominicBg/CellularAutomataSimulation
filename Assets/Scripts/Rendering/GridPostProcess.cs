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

    public NativeQueue<PixelSortingRenderingSettings> pixelSortingRequestQueue;

    public void OnStart()
    {
        pixelSortingRequestQueue = new NativeQueue<PixelSortingRenderingSettings>(Allocator.Persistent);
    }
    public void OnEnd()
    {
        pixelSortingRequestQueue.Dispose();
    }

    public void ApplyPostProcess(ref NativeArray<Color32> colors, int2 sizes)
    {
        using (s_PixelSorting.Auto())
        { 
            new PixelSortingRenderingJob()
            {
                colorArray = colors,
                requestQueue = pixelSortingRequestQueue,
                mapSizes = sizes
            }.Run();
        }

        //Run other requests
    }
}
