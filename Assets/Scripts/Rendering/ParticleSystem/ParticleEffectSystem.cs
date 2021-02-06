using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

[System.Serializable]
public class ParticleEffectSystem
{
    public struct Particle
    {
        public float2 position;
        public int startTick;
        public Color color;
        public int radius;
        public uint id;
        public float lifeTime;
    }

    public Bound bound { get; private set; }
    ParticleEffectSystemScriptable settings;
    NativeList<Particle> particles;
    uint idCount = 0;

    public ParticleEffectSystem(ParticleEffectSystemScriptable settings)
    {
        this.settings = settings;
        particles = new NativeList<Particle>(Allocator.Persistent);
    }

    public void EmitParticle(int2 position, ref TickBlock tickBlock)
    {
        Unity.Mathematics.Random rng = Unity.Mathematics.Random.CreateFromIndex(idCount);
        int2 range = settings.settings.emitter.emitterSizes;
        int2 particlePosition = position + rng.NextInt2(-range / 2, range / 2);
        particles.Add(GenerateParticle(particlePosition, tickBlock.tick));
    }

    public void EmitParticleAtPosition(int2 position, ref TickBlock tickBlock)
    {
        particles.Add(GenerateParticle(position, tickBlock.tick));
    }

    private Particle GenerateParticle(int2 position, int tick)
    {
        Unity.Mathematics.Random rng = Unity.Mathematics.Random.CreateFromIndex(idCount);
        float duration = math.lerp(settings.settings.lifeTime.minDuration, settings.settings.lifeTime.maxDuration, rng.NextFloat());
        Particle particle = new Particle() { id = idCount, startTick = tick, position = position, lifeTime = duration };
        idCount++;
        return particle;
    }

    public void Update(ref TickBlock tickBlock)
    {
        new ParticleEffectSystemUpdateJob()
        {
            dt = GameManager.DeltaTime,
            particles = particles.AsArray(),
            settings = settings.settings,
            tickBlock = tickBlock
        }.Schedule(particles.Length, 8).Complete();

        for (int i = particles.Length - 1; i >= 0; i--)
        {
            if (tickBlock.DurationSinceTick(particles[i].startTick) > particles[i].lifeTime)
                particles.RemoveAt(i);
        }

        NativeReference<Bound> outputBound = new NativeReference<Bound>(Allocator.TempJob);
        new ComputeBound()
        {
            bound = outputBound,
            particles = particles
        }.Run();
        bound = outputBound.Value;
        outputBound.Dispose();
    }

    public void Render(ref NativeArray<Color32> outputColor, PixelCamera pixelCamera)
    {
        new ParticleEffectSystemRenderJob()
        {
            cameraHandle = pixelCamera.GetHandle(),
            outputColors = outputColor,
            particles = particles,
            settings = settings.settings
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public void Dispose()
    {
        particles.TryDispose();
    }


    [BurstCompile]
    public struct ComputeBound : IJob
    {
        public NativeList<Particle> particles;
        public NativeReference<Bound> bound;

        public void Execute()
        {
            if(particles.Length == 0)
            {
                bound.Value = new Bound(0, 1);
                return;
            }

            int2 min = (int2)particles[0].position;
            int2 max = (int2)particles[0].position;
            for (int i = 0; i < particles.Length; i++)
            {
                min = math.min(min, (int2)particles[i].position - particles[i].radius);
                max = math.max(max, (int2)particles[i].position + particles[i].radius);
            }

            bound.Value = Bound.MinMax(min, max);
        }
    }
}

