using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct RenderRotationBoundJob : IJobParallelFor
{
    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;
    public Color color;
    public NativeArray<Color32> outputColors;
    public BlendingMode blending;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(pos);

        if (rotationBound.PointInBound(worldPos))
            outputColors[index] = RenderingUtils.Blend(outputColors[index], color, blending);
    }
}

[BurstCompile]
public struct RenderRotationBoundSpriteJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;

    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;
    public BlendingMode blending;
    public NativeSprite nativeSprite;
    public Color32 tint;
    public float sin, cos;
    public bool superSample;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(pos);
        Color sampleColor = GetColor(worldPos);

        if(sampleColor.a > 0.05f)
        {
            outputColors[index] = RenderingUtils.Blend(outputColors[index], sampleColor, blending);
        }
        else if(superSample)
        {
            NativeArray<Color> colors = new NativeArray<Color>(4, Allocator.Temp);
            //try super sampler XVII
            colors[0] = GetColor(worldPos + new int2(0, 1));
            colors[1] = GetColor(worldPos + new int2(1, 0));
            colors[2] = GetColor(worldPos + new int2(0, -1));
            colors[3] = GetColor(worldPos + new int2(-1, 0));

            //find a pair, if there's 2 take this pixel instead
            for (int i = 0; i < colors.Length; i++)
            {
                for (int j = i + 1; j < colors.Length; j++)
                {
                    if(colors[i] == colors[j] && colors[i].a >= 0.05f)
                    {
                        outputColors[index] = RenderingUtils.Blend(outputColors[index], colors[i], blending);
                        colors.Dispose();
                        return;
                    }
                }
            }
            colors.Dispose();
        }
    }

    Color GetColor(int2 worldPos)
    {
        if (rotationBound.TryGetUV(worldPos, sin, cos, out float2 uv))
        {
            return (Color)RenderingUtils.SampleTexture(in nativeSprite, uv) * tint;
        }
        return Color.clear;
    }         
}


[BurstCompile]
public struct RenderRotationBoundSpritePass1Job : IJobParallelFor
{
    public NativeGrid<Color32> outputColors;

    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;
    public NativeSprite nativeSprite;
    public Color32 tint;
    public float sin, cos;
    public int2 renderPos;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, outputColors.Sizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(renderPos + pos);
        outputColors[pos] = GetColor(worldPos); 
    }

    Color GetColor(int2 worldPos)
    {
        if (rotationBound.TryGetUV(worldPos, sin, cos, out float2 uv))
        {
            return (Color)RenderingUtils.SampleTexture(in nativeSprite, uv) * tint;
        }
        return Color.clear;
    }
}

[BurstCompile]
public struct RenderRotationBoundSpritePass2Job : IJobParallelFor
{
    [ReadOnly] public NativeGrid<Color32> inputColors;
    public NativeGrid<Color32> outputColors;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, outputColors.Sizes);

        if (inputColors[pos].a >= 0.05f)
        {
            outputColors[pos] = inputColors[pos];
            return;
        }

        NativeArray<Color> colors = new NativeArray<Color>(4, Allocator.Temp);

        //try super sampler XVII
        colors[0] = GetColor(pos + new int2( 0,  1));
        colors[1] = GetColor(pos + new int2( 1,  0));
        colors[2] = GetColor(pos + new int2( 0, -1));
        colors[3] = GetColor(pos + new int2(-1,  0));

        //find a pair, if there's 2 take this pixel instead
        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = i + 1; j < colors.Length; j++)
            {
                if (colors[i] == colors[j] && colors[i].a >= 0.05f)
                {
                    outputColors[pos] = colors[i];
                    colors.Dispose();
                    return;
                }
            }
        }
        colors.Dispose();
    }

    public Color GetColor(int2 pos)
    {
        if(GridHelper.InBound(pos, inputColors.Sizes))
        {
            return inputColors[pos];
        }
        return Color.clear;
    }
}


[BurstCompile]
public struct RenderRotationBoundSpriteReflectionPass1Job : IJobParallelFor
{
    public NativeGrid<Color32> pixels;
    public NativeGrid<float3> normals;
    public NativeGrid<float> reflections;

    public NativeGrid<Color32> outputPixels;
    public NativeGrid<float3> outputNormals;
    public NativeGrid<float> outputReflections;

    public RotationBound rotationBound;
    public PixelCamera.PixelCameraHandle cameraHandle;

    public float sin, cos;
    public int2 renderPos;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, outputPixels.Sizes);
        int2 worldPos = cameraHandle.GetGlobalPosition(renderPos + pos);
        if (rotationBound.TryGetUV(worldPos, sin, cos, out float2 uv) && GridHelper.InBound(pos, outputPixels.Sizes))
        {
            outputPixels[pos] = RenderingUtils.SampleNativeGrid(in pixels, uv);
            float3 normal = RenderingUtils.SampleNativeGrid(in normals, uv);
            outputNormals[pos] = new float3(MathUtils.Rotate(normal.xy, cos, sin), normal.z);
            outputReflections[pos] = RenderingUtils.SampleNativeGrid(in reflections, uv);
        }
        else
        {
            outputPixels[pos] = Color.clear;
            outputNormals[pos] = 0;
            outputReflections[pos] = 0;
        }
    }
}

[BurstCompile]
public struct RenderRotationBoundSpriteReflectionPass2Job : IJobParallelFor
{
    [ReadOnly] public NativeGrid<Color32> inputPixels;
    [ReadOnly] public NativeGrid<float3> inputNormals;
    [ReadOnly] public NativeGrid<float> inputReflections;

    public NativeGrid<Color32> outputPixels;
    public NativeGrid<float3> outputNormals;
    public NativeGrid<float> outputReflections;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, inputPixels.Sizes);

        if (inputPixels[pos].a >= 0.05f)
        {
            inputPixels[pos] = inputPixels[pos];
            return;
        }

        NativeArray<Color> colors = new NativeArray<Color>(4, Allocator.Temp);
        NativeArray<int2> positions = new NativeArray<int2>(4, Allocator.Temp);
        positions[0] = pos + new int2(0, 1);
        positions[1] = pos + new int2(1, 0);
        positions[2] = pos + new int2(0, -1);
        positions[3] = pos + new int2(-1, 0);

        //try super sampler XVII
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = GetColor(positions[i]);
        }
  
        //find a pair, if there's 2 take this pixel instead
        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = i + 1; j < colors.Length; j++)
            {
                if (colors[i] == colors[j] && colors[i].a >= 0.05f)
                {
                    int2 samplePos = positions[i];
                    outputPixels[pos] = inputPixels[samplePos];
                    outputNormals[pos] = inputNormals[samplePos];
                    outputReflections[pos] = inputReflections[samplePos];

                    positions.Dispose();
                    colors.Dispose();
                    return;
                }
            }
        }
        colors.Dispose();
        positions.Dispose();
    }

    public Color GetColor(int2 pos)
    {
        if (GridHelper.InBound(pos, outputPixels.Sizes))
        {
            return outputPixels[pos];
        }
        return Color.clear;
    }
}

