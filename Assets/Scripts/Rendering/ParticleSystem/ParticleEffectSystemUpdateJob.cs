using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ParticleEffectSystemUpdateJob : IJobParallelFor
{
    public NativeArray<ParticleEffectSystem.Particle> particles;
    public ParticleEffectSystemSettings settings;
    public TickBlock tickBlock;
    public float dt;

    public void Execute(int index)
    {
        ParticleEffectSystem.Particle particle = particles[index];
        Unity.Mathematics.Random rng = Unity.Mathematics.Random.CreateFromIndex(particle.id);

        //LifeTime
        float currentDuration = tickBlock.DurationSinceTick(particle.startTick);
        float lifeTimeRatio = math.saturate(currentDuration / particle.lifeTime);

        //Movement
        if(settings.movement.useNoise)
        {
            float2 noise = NoiseXVII.fbm4_2(particle.position + new float2(tickBlock.tick * settings.movement.noiseSpeed + particle.startTick));
            particle.position += (int2)(settings.movement.noiseAmplitude * noise * dt);
        }

        particle.position += settings.movement.windForce * dt;

        //Color
        Color startColor = Color.Lerp(settings.colors.colorStartMin, settings.colors.colorStartMax, rng.NextFloat());
        Color endColor = Color.Lerp(settings.colors.colorEndMin, settings.colors.colorEndMax, rng.NextFloat());
        particle.color = Color.Lerp(startColor, endColor, lifeTimeRatio).ReduceResolution(settings.colors.resolution);

        //Sizes
        float startRadius = math.lerp(settings.emitter.minStartRadius, settings.emitter.maxStartRadius, rng.NextFloat());
        float endRadius = math.lerp(settings.emitter.minEndRadius, settings.emitter.maxEndRadius, rng.NextFloat());
        particle.radius = (int)math.lerp(startRadius, endRadius, lifeTimeRatio);

        particles[index] = particle;
    }
}