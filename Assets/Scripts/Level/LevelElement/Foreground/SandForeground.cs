using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SandForeground : LevelElement, IAlwaysRenderable
{
    public Settings settings;

    public override void LateRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        new SandForegroundJob()
        {
            outputColors = outputColors,
            settings = settings,
            tickBlock = tickBlock,
            sandRendering = GridRenderer.Instance.particleRendering.sandRendering,
            posOffset = renderPos
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    [BurstCompile]
    public struct SandForegroundJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColors;
        public Settings settings;
        public TickBlock tickBlock;
        public SandRendering sandRendering;
        public float2 posOffset;

        public void Execute(int index)
        {
            float2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes) + posOffset;
            float2 offset = tickBlock.tick * settings.speed;
            float2 offset2 = tickBlock.tick * settings.speed2;

            float2 posNoise = pos * settings.scale + offset;
            float2 posNoise2 = pos * settings.scale2 + offset2;

            float yBoost = (pos.y - posOffset.y) * settings.yBoost;

            float noisevalue = NoiseXVII.fbm4r(posNoise + offset2 + NoiseXVII.fbm4r(offset + posNoise + NoiseXVII.fbm4r(posNoise)));
            noisevalue = MathUtils.ReduceResolution(noisevalue, settings.resolution);

            float granularNoise = MathUtils.unorm(noise.cnoise(posNoise2 + offset2 + noise.cnoise(posNoise2)));
            granularNoise = MathUtils.ReduceResolution(granularNoise, settings.resolution);


            Color color = sandRendering.sandColor; //sandRendering.GetColor((int2)pos, ref tickBlock);
            color.a = (noisevalue * granularNoise+ yBoost) * settings.maxAlpha;
            outputColors[index] = RenderingUtils.Blend(outputColors[index], color, settings.blending);
        }
    }
    [System.Serializable]
    public struct Settings
    {
        public float2 speed;
        public float2 speed2;
        public float scale;
        public float scale2;
        public float maxAlpha;
        public BlendingMode blending;

        public float yBoost;
        public float resolution;
    }
}
