using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LevelCavernBackground : LevelElement, IAlwaysRenderable
{
    public Color tone;
    public float parallaxOffset;
    public NoiseCutoff noiseCutoff;
    public NoiseXVII.Noise[] noises;




    [System.Serializable]

    public struct NoiseCutoff
    {
        public bool useNoise;
        public float noiseThreshold;
    }


    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        NativeArray<NoiseXVII.Noise> nativeNoises = new NativeArray<NoiseXVII.Noise>(noises, Allocator.TempJob);
        new RenderCaveJob()
        {
            cameraPos = renderPos,
            map = map,
            outputColors = outputColors,
            parallax = parallaxOffset,
            rockRendering = GridRenderer.Instance.particleRendering.rockRendering,
            tickBlock = tickBlock,
            tone = tone,
            lights = lights,
            noiseCutoff = noiseCutoff,
            nativeNoises = nativeNoises,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        nativeNoises.Dispose();
    }

    [BurstCompile]
    public struct RenderCaveJob : IJobParallelFor
    {
        public Color tone;
        public RockRendering rockRendering;
        public Map map;
        public int2 cameraPos;
        public float parallax;
        public TickBlock tickBlock;

        public NoiseCutoff noiseCutoff;

        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeList<LightSource> lights;
        [ReadOnly] public NativeArray<NoiseXVII.Noise> nativeNoises;

        public void Execute(int index)
        {
            int2 gridPosition = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
            int2 parallaxOffset = (int2)((float2)cameraPos * parallax);
            int2 position = gridPosition - GameManager.GridSizes/2 + parallaxOffset;
            if(noiseCutoff.useNoise)
            {
                float value = 0;
                for (int i = 0; i < nativeNoises.Length; i++)
                {
                    value += nativeNoises[i].CalculateValue(position);
                }

                if (value < noiseCutoff.noiseThreshold)
                    return;
            }

            Color rockColor = rockRendering.GetColor(position, lights, cameraPos - parallaxOffset);
            outputColors[index] = rockColor * tone;
        }
    }
}
