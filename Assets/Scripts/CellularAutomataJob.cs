using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CellularAutomataJob : IJob
{
    public NativeArray<ParticleSpawner> nativeParticleSpawners;

    public Map map;
    public NativeArray<Particle> particles;
    public Random random;

    public void Execute()
    {
        SpawnParticles();
        UpdateSimulation();
    }

    void SpawnParticles()
    {
        for (int i = 0; i < nativeParticleSpawners.Length; i++)
        {
            var spawner = nativeParticleSpawners[i];
            if (random.NextFloat() <= spawner.chanceSpawn)
            {
                int index = map.PosToIndex(spawner.spawnPosition);
                particles[index] = new Particle() { type = spawner.particleType };
            }
        }
    }

    void UpdateSimulation()
    {
        for (int x = 0; x < map.sizes.x; x++)
        {
            for (int y = 0; y < map.sizes.y; y++)
            {
                int2 pos = new int2(x, y);
                UpdateParticleBehaviour(pos);
            }
        }

        //for (int x = map.sizes.x - 1; x >= 0; x--)
        //{
        //    for (int y = map.sizes.y - 1; y >= 0; y--)
        //    {
        //        int2 pos = new int2(x, y);
        //        UpdateParticleBehaviour(pos);
        //    }
        //}
    }

    void UpdateParticleBehaviour(int2 pos)
    {
        int index = map.PosToIndex(pos);
        Particle particle = particles[index];
        switch (particle.type)
        {
            case ParticleType.None:
                break;
            case ParticleType.Water:
                UpdateWaterParticle(particle, pos);
                break;
            case ParticleType.Sand:
                UpdateSandParticle(particle, pos);
                break;
            case ParticleType.Mud:
                UpdateMudParticle(particle, pos);
                break;
            case ParticleType.Player:
                //nada
                break;
        }
    }

    void UpdateWaterParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (map.IsFreePosition(particles, bottom))
        {
            map.MoveParticle(particles, particle, pos, bottom);
        }
        else
        {
            int2 left = new int2(pos.x - 1, pos.y);
            int2 right = new int2(pos.x + 1, pos.y);
            int2 left2 = new int2(pos.x - 2, pos.y);
            int2 right2 = new int2(pos.x + 2, pos.y);

            bool goingLeft = random.NextBool();

            int2 dir1 = (goingLeft) ? left2 : right2;
            int2 dir2 = (goingLeft) ? right2 : left2;
            int2 dir3 = (goingLeft) ? left : right;
            int2 dir4 = (goingLeft) ? right : left;

            //Try go 2 steps only if its not blocked by 1 step
            if (map.IsFreePosition(particles, dir1) && map.IsFreePosition(particles, dir3))
            {
                map.MoveParticle(particles, particle, pos, dir1);
            }
            else if (map.IsFreePosition(particles, dir2) && map.IsFreePosition(particles, dir4))
            {
                map.MoveParticle(particles, particle, pos, dir2);
            }
            else if (map.IsFreePosition(particles, dir3))
            {
                map.MoveParticle(particles, particle, pos, dir3);
            }
            else if (map.IsFreePosition(particles, dir4))
            {
                map.MoveParticle(particles, particle, pos, dir4);
            }
            else if (SurroundedByCount2(pos, ParticleType.Sand, ParticleType.Mud, 1) > SurroundedByCount(pos, ParticleType.Water, 1) + 2)
            {
                //Dry up
                int index = map.PosToIndex(pos);
                particle.type = ParticleType.None;
                particles[index] = particle;
            }
        }
    }

    void UpdateSandParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (map.IsFreePosition(particles, bottom))
        {
            map.MoveParticle(particles, particle, pos, bottom);
        }
        else
        {
            //randomize gauche droite
            int2 bottomLeft = new int2(bottom.x - 1, bottom.y);
            int2 bottomRight = new int2(bottom.x + 1, bottom.y);
            bool goingLeft = random.NextBool();
            int2 firstDir = (goingLeft) ? bottomLeft : bottomRight;
            int2 secondDir = (goingLeft) ? bottomRight : bottomLeft;

            if (map.IsFreePosition(particles, firstDir))
            {
                map.MoveParticle(particles, particle, pos, firstDir);
            }
            else if (map.IsFreePosition(particles, secondDir))
            {
                map.MoveParticle(particles, particle, pos, secondDir);
            }
            else if (IsSurroundedBy(pos, ParticleType.Water, 1))
            {   //Sand is touching water, becomes mud
                int index = map.PosToIndex(pos);
                particle.type = ParticleType.Mud;
                particles[index] = particle;
            }
        }
    }

    void UpdateMudParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (!map.InBound(bottom))
            return;

        if (map.IsFreePosition(particles, bottom))
        {
            map.MoveParticle(particles, particle, pos, bottom);
        }
        else if(map.ParticleTypeAtPosition(particles, bottom) == ParticleType.Water)
        {
            //Mud will sink
            map.SwapParticles(particles, pos, bottom);
        }
        else if (!IsSurroundedBy(pos, ParticleType.Water, 1))
        {   //Mud is not touching water, becomes sand
            int index = map.PosToIndex(pos);
            particle.type = ParticleType.Sand;
            particles[index] = particle;
        }
      
    }

    bool IsSurroundedBy(int2 pos, ParticleType type, int range = 1)
    {
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == y)
                    continue;

                int2 adjacentPos = pos + new int2(x, y);
                if(map.InBound(adjacentPos))
                {
                    int index = map.PosToIndex(adjacentPos);
                    if (particles[index].type == type)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    int SurroundedByCount(int2 pos, ParticleType type, int range = 1)
    {
        int count = 0;
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == y)
                    continue;

                int2 adjacentPos = pos + new int2(x, y);
                if (map.InBound(adjacentPos))
                {
                    int index = map.PosToIndex(adjacentPos);
                    if (particles[index].type == type)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    //eww lol
    int SurroundedByCount2(int2 pos, ParticleType type, ParticleType type2, int range = 1)
    {
        int count = 0;
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == y)
                    continue;

                int2 adjacentPos = pos + new int2(x, y);
                if (map.InBound(adjacentPos))
                {
                    int index = map.PosToIndex(adjacentPos);
                    if (particles[index].type == type || particles[index].type == type2)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
}
