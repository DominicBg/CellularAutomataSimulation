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
    public int quantizationSize = 3;

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

        Dictionary<int2, float> positionIntensity = new Dictionary<int2, float>();

        for (int i = 0; i < cinderPositions.Length; i++)
        {
            int2 quantizedPosition = cinderPositions[i] / quantizationSize;
            float intensity = 1 - math.saturate((float)tickIdle[i] / fadeOffTick);
            if (positionIntensity.ContainsKey(quantizedPosition))
            {
                positionIntensity[quantizedPosition] += intensity;
            }
            else
            {
                positionIntensity.Add(quantizedPosition, intensity);
            }
            //LightSource source = lightSource.GetLightSource(cinderPositions[i], tick);
            //source.intensity *= 1 - math.saturate((float)tickIdle[i] / fadeOffTick);
            //list.Add(source);
        }

        foreach(int2 quantizedPos in positionIntensity.Keys)
        {
            int2 recenterOffset = (quantizationSize / 2);
            int2 finalPos = (quantizedPos * quantizationSize) + recenterOffset;
            LightSource source = lightSource.GetLightSource(finalPos, tick);
            source.intensity *= positionIntensity[quantizedPos];
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
