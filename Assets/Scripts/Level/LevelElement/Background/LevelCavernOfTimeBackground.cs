using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LevelCavernOfTimeBackground: LevelElement, IAlwaysRenderable
{
    public CavernOfTimeBackgroundJob.Settings settings;

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        new CavernOfTimeBackgroundJob()
        {
            tickBlock = tickBlock,
            outputColor = outputColors,
            settings = settings,
            cameraPos = renderPos,
            lights = lights
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
}
