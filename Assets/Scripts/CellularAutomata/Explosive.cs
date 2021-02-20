using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
public static class Explosive
{
    [System.Serializable]
    public struct Settings
    {
        public int radius;
        public float strength;
        public bool canDestroy;
    }

    public static void SetExplosive(int2 position, in Settings settings, Map map)
    {
        new ExplosiveJob() { position = position, map = map, settings = settings }.Run();
    }

    public struct ExplosiveJob : IJob
    {
        public Settings settings;
        public Map map;
        public int2 position;

        public void Execute()
        {
            var positions = GridHelper.GetCircleAtPosition(position, settings.radius, map.Sizes, Allocator.Temp);
            for (int i = 0; i < positions.Length; i++)
            {
                int2 pos = positions[i];
                Particle particle = map.GetParticle(pos);
                float2 diff = pos - position;
                particle.velocity += diff * settings.strength;
                
                if(settings.canDestroy)
                {
                    ParticleType newType = GetAfterExplosiveType(map.GetParticleType(pos));
                    particle.type = newType;
                }

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
