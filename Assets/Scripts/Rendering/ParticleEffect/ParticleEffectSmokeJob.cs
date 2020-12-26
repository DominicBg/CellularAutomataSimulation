using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct SmokeParticleSystemSettings
{
    public float noiseSpeed;
    public float moveAmplitude;
    public float fadeOffFactor;
    public float dispersionFactor;
    public float2 windForce;
}

[BurstCompile]
public struct ParticleEffectSmokeMoveJob : IJob
{
    public NativeArray<Color32> outputColors;
    public NativeList<SmokeParticle> smokeParticles;
    public SmokeParticleSystemSettings settings;
    public TickBlock tickBlock;
    public float dt;

    public void Execute()
    {
        for (int i = 0; i < smokeParticles.Length; i++)
        {
            SmokeParticle sp = smokeParticles[i];
            float t = math.saturate(tickBlock.DurationSinceTick(sp.startTick) / sp.duration);
            float2 noise = NoiseXVII.fbm4_2(sp.position + new float2(tickBlock.tick * settings.noiseSpeed + sp.startTick));
            sp.position += (int2)(settings.moveAmplitude * noise);

            sp.position += settings.windForce * dt;
            sp.position = math.clamp(sp.position, 0, GameManager.GridSizes - 1);

            int index = ArrayHelper.PosToIndex((int2)sp.position, GameManager.GridSizes);
            outputColors[index] = Color32.Lerp(sp.startColor, sp.endColor, t);

            smokeParticles[i] = sp;
        }
    }
}

[BurstCompile]
public struct ParticleEffectSmokeDispersionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> inputColors;
    public NativeArray<Color32> outputColors;
    public SmokeParticleSystemSettings settings;
    public TickBlock tickBlock;

    public void Execute(int index)
    {
        int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        Color32 input = inputColors[index];
        input = Color32.Lerp(input, Color.clear, settings.fadeOffFactor);

        int count = 0;
        float4 col = GetColor(pos, new int2(1, 0), ref count);
        col += GetColor(pos, new int2(-1, 0), ref count);
        col += GetColor(pos, new int2(0, 1), ref count);
        col += GetColor(pos, new int2(0, -1), ref count);
        col /= count;

        Color surrounding = new Color(col.x, col.y, col.z, col.w);
        outputColors[index] = Color.Lerp(input, surrounding, settings.dispersionFactor);
    }

    float4 GetColor(int2 pos, int2 offset, ref int count)
    {
        if (!GridHelper.InBound(pos + offset, GameManager.GridSizes))
            return 0;

        count++;
        int index = ArrayHelper.PosToIndex(pos + offset, GameManager.GridSizes);
        Color col = inputColors[index];
        return new float4(col.r, col.g, col.b, col.a);
    }
}

public struct SmokeParticle
{
    public float2 position;
    public Color32 startColor;
    public Color32 endColor;
    public int startTick;
    public float duration;
}