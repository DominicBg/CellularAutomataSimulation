using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
public static class Explosive
{
    [System.Serializable]
    public struct ExplosiveSettings
    {
        public int radius;
        public float strength;
    }

    public static void SetExplosive(int2 position, ref ExplosiveSettings settings, Map map)
    {
        new ExplosiveJob() { position = position, map = map, settings = settings }.Run();
    }

    public struct ExplosiveJob : IJob
    {
        public ExplosiveSettings settings;
        public Map map;
        public int2 position;

        public void Execute()
        {
            var positions = GridHelper.GetCircleAtPosition(position, settings.radius, map.Sizes, Allocator.Temp);
            for (int i = 0; i < positions.Length; i++)
            {
                int2 pos = positions[i];
                ParticleType newType = GetAfterExplosiveType(map.GetParticleType(pos));
                Particle particle = map.GetParticle(pos);
                float2 diff = pos - position;
                particle.velocity += diff * settings.strength;
                particle.type = newType;
                map.SetParticle(pos, particle);
            }
        }

        ParticleType GetAfterExplosiveType(ParticleType type)
        {
            switch(type)
            {
                case ParticleType.Rock: return ParticleType.Rubble;
            }
            return type;
        }
    }

}
