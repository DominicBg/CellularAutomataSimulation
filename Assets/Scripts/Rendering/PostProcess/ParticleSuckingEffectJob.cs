using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ParticleSuckingEffectSettings
{
    //add bound
    public int sample;
    public int range;
    public float offset;
    public float speed;
    public float blendRatio;
    public BlendingMode blendingMode;
    public int fadeOff;
    public int shrink;
    public EaseXVII.Ease ease;
    public float sinOffsetFreq;
    public float sinOffsetAmp;
    public float2 sinOffset;
}

[BurstCompile]
public struct ParticleSuckingEffectJob : IJobParallelFor
{
    public ParticleSuckingEffectSettings settings;
    public int direction;

    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeArray<Color32> inputColors;

    public TickBlock tickBlock;
    public Bound bound;
    public float intensity;

    public void Execute(int index)
    {
        bound = Bound.CenterAligned(bound.center, bound.sizes - settings.shrink);
        int2 gridPos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
        int2 projectedPoint = bound.ProjectPointOnbound(gridPos);
        float distanceSqToClosestPoint = math.distancesq(gridPos, projectedPoint);

        if (distanceSqToClosestPoint > settings.fadeOff * settings.fadeOff) 
            return;

        float fadeOffAlpha = 1 - (math.sqrt(distanceSqToClosestPoint) / settings.fadeOff);

        Color color = outputColors[index];

        float sinPosOffset = direction * settings.sinOffset.x * gridPos.x + settings.sinOffset.y * gridPos.y;
        float sinOffset = settings.sinOffsetAmp * math.sin(tickBlock.tick * settings.sinOffsetFreq * 2 * math.PI + sinPosOffset);
        for (int i = 0; i < settings.sample; i++)
        {
            float t = math.frac(tickBlock.tick * settings.speed + i * settings.offset);
            int offset = (int)(direction * settings.range * intensity * EaseXVII.Evaluate(t, settings.ease));
            int2 samplePos = gridPos + new int2(offset, (int)sinOffset);
            if(GridHelper.InBound(samplePos, GameManager.RenderSizes))
            {
                int sampleIndex = ArrayHelper.PosToIndex(samplePos, GameManager.RenderSizes);
                Color sampleColor = inputColors[sampleIndex];
                sampleColor.a = settings.blendRatio * fadeOffAlpha * intensity;
                color = RenderingUtils.Blend(color, sampleColor, settings.blendingMode);
            }
        }

        outputColors[index] = color;
    }
}
