using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[SerializeField]
public class SmokeParticleSystem
{
    NativeArray<Color32> smokeColors;
    NativeList<SmokeParticle> smokeParticles;

    public SmokeParticleSystem()
    {
        smokeColors = new NativeArray<Color32>(GameManager.GridLength, Allocator.Persistent);
        smokeParticles = new NativeList<SmokeParticle>(Allocator.Persistent);
    }

    public void EmitParticle(int2 position,ref SmokeParticleSystemEmitter emitter, ref TickBlock tickBlock)
    {
        smokeParticles.Add(emitter.GetSmokeParticle(position, ref tickBlock));
    }

    public void Update(ref SmokeParticleSystemSettings settings, ref TickBlock tickBlock)
    {
        NativeArray<Color32> input = new NativeArray<Color32>(smokeColors, Allocator.TempJob);
        new ParticleEffectSmokeDispersionJob()
        {
            inputColors = input,
            outputColors = smokeColors,
            tickBlock = tickBlock,
            settings = settings,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        input.Dispose();

        new ParticleEffectSmokeMoveJob()
        {
            outputColors = smokeColors,
            settings = settings,
            smokeParticles = smokeParticles,
            tickBlock = tickBlock,
            dt = GameManager.DeltaTime
        }.Run();

        for (int i = smokeParticles.Length - 1; i >= 0; i--)
        {
            if (tickBlock.DurationSinceTick(smokeParticles[i].startTick) > smokeParticles[i].duration)
                smokeParticles.RemoveAt(i);
        }
    }

    public void Render(ref NativeArray<Color32> outputColor, BlendingMode blending = BlendingMode.Transparency)
    {
        GridRenderer.ApplyTexture(ref outputColor, ref smokeColors, blending);
    }

    public void Dispose()
    {
        smokeColors.Dispose();
        smokeParticles.Dispose();
    }
}
