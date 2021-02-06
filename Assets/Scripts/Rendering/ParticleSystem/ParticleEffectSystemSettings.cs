using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ParticleEffectSystemSettings
{
    public Colors colors;
    public Emitter emitter;
    public LifeTime lifeTime;
    public Movement movement;

    [System.Serializable]
    public struct Colors
    {
        public Color colorStartMin;
        public Color colorStartMax;
        public Color colorEndMin;
        public Color colorEndMax;
        public int resolution;
        public BlendingMode blendingMode;
    }

    [System.Serializable]
    public struct Emitter
    {
        public int2 emitterSizes;
        public int minStartRadius;
        public int maxStartRadius;
        public int minEndRadius;
        public int maxEndRadius;
    }

    [System.Serializable]
    public struct LifeTime
    {
        public float minDuration;
        public float maxDuration;
    }

    [System.Serializable]
    public struct Movement
    {
        public float2 windForce;
        public bool useNoise;
        public float noiseSpeed;
        public float noiseAmplitude;
    }
}
