using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BloomPostProcess
{
    public static void Apply(ref NativeArray<Color32> outputColors, float threshold, BlendingMode blending)
    {
        NativeArray<Color32> filteredImage = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new FilterImageByLuminance()
        {
            inputColors = outputColors,
            outputColors = filteredImage,
            threshold = threshold
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        GaussianBlurEffect.Apply(ref filteredImage);
        GridRenderer.CombineColors(ref outputColors, ref filteredImage, blending);
    }
    public static void Apply(ref NativeArray<Color32> outputColors, ref NativeArray<float> lightIntensities, float intensity, BlendingMode blending)
    {
        NativeArray<Color32> filteredImage = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new FilterImageByLightIntensities()
        {
            inputColors = outputColors,
            outputColors = filteredImage,
            lightIntensities = lightIntensities,
            intensity = intensity
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        GaussianBlurEffect.Apply(ref filteredImage);
        GridRenderer.CombineColors(ref outputColors, ref filteredImage, blending);
    }

    public static void DebugLightIntensities(ref NativeArray<Color32> outputColors, ref NativeArray<float> lightIntensities, float intensity)
    {
        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new DebugFilterImageByLightIntensities()
        {
            inputColors = inputColors,
            outputColors = outputColors,
            lightIntensities = lightIntensities,
            intensity = intensity
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
    }



    [BurstCompile]
    public struct FilterImageByLuminance : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> inputColors;
        public NativeArray<Color32> outputColors;
        public float threshold;

        public void Execute(int index)
        {
            float luminance = RenderingUtils.Luminance(inputColors[index]);
            if (luminance < threshold)
            {
                outputColors[index] = Color.clear;
            }
            else
            {
                outputColors[index] = (Color)inputColors[index] * math.remap(threshold, 1, 0, 1, luminance);
            }
        }
    }

    [BurstCompile]
    public struct FilterImageByLightIntensities : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> inputColors;
        public NativeArray<Color32> outputColors;
        public NativeArray<float> lightIntensities;
        public float intensity;
        public void Execute(int index)
        {
            outputColors[index] = (Color)inputColors[index] * lightIntensities[index] * intensity;         
        }
    }

    [BurstCompile]
    public struct DebugFilterImageByLightIntensities : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> inputColors;
        public NativeArray<Color32> outputColors;
        public NativeArray<float> lightIntensities;
        public float intensity;
        public void Execute(int index)
        {
            outputColors[index] = Color.white * lightIntensities[index] * intensity;
        }
    }
}
