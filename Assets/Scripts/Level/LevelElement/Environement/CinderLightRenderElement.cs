using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CinderLightRenderElement : LevelElement, ILightMultiSource
{
    public LightSourceScriptable lightSource;
    public int fadeOffTick = 120;
    public void GetLightSource(NativeList<LightSource> list, int tick)
    {
        NativeList<int2> cinderPositions = new NativeList<int2>(100, Allocator.TempJob);
        NativeList<int> tickIdle = new NativeList<int>(100, Allocator.TempJob);
        new MapParticleScanJob()
        {
            map = map,
            outputPositions = cinderPositions,
            tickIdle = tickIdle,
            type = ParticleType.Cinder,
            updateBound = scene.updateBound
        }.Run();

        for (int i = 0; i < cinderPositions.Length; i++)
        {
            LightSource source = lightSource.GetLightSource(cinderPositions[i], tick);
            source.intensity *= 1 - math.saturate((float)tickIdle[i] / fadeOffTick);
            list.Add(source);
        }

        tickIdle.Dispose();
        cinderPositions.Dispose();
    }

    bool ILightMultiSource.IsVisible()
    {
        return isVisible;
    }
}
