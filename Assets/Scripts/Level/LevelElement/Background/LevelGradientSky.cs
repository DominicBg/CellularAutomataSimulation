using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LevelGradientSky : LevelElement, IAlwaysRenderable
{
    public Color[] gradientColors;
    public bool isVertical;
    public float resolution;
    public BlendingMode blending;

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        var nativeGradients = new NativeArray<Color>(gradientColors, Allocator.TempJob);
        new GradientJob()
        {
            outputColors = outputColors,
            gradientColors = nativeGradients,
            isVertical = isVertical,
            resolution = resolution,
            blending = blending
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        nativeGradients.Dispose();
    }


    struct GradientJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColors;
        [ReadOnly] public NativeArray<Color> gradientColors;
        public bool isVertical;
        public float resolution;
        public BlendingMode blending;

        public void Execute(int index)
        {
            float2 uv = ArrayHelper.IndexToUv(index, GameManager.GridSizes);

            float value = isVertical ? uv.y : uv.x;
            float steps = 1f / (gradientColors.Length - 1);

            for (int i = 0; i < gradientColors.Length - 1; i++)
            {
                float threshold1 = steps * i;
                float threshold2 = steps * (i + 1);
                if (value >= threshold1 && value < threshold2)
                {
                    float t = math.unlerp(threshold1, threshold2, value);
                    t = MathUtils.ReduceResolution(t, resolution);
                    Color newColor = Color.Lerp(gradientColors[i], gradientColors[i + 1], t);
                    outputColors[index] = RenderingUtils.Blend(outputColors[index], newColor, blending);
                    return;
                }
            }
        }
    }
}
