using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CellularAutomataJob : IJob
{
    public NativeArray<ParticleSpawner> nativeParticleSpawners;

    public Map map;
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
                map[spawner.spawnPosition] = new Particle() { type = spawner.particleType };
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

        Particle particle = map[pos];
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
        if (map.IsFreePosition(bottom))
        {
            map.MoveParticle(particle, pos, bottom);
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
            if (map.IsFreePosition(dir1) && map.IsFreePosition(dir3))
            {
                map.MoveParticle(particle, pos, dir1);
            }
            else if (map.IsFreePosition(dir2) && map.IsFreePosition(dir4))
            {
                map.MoveParticle(particle, pos, dir2);
            }
            else if (map.IsFreePosition(dir3))
            {
                map.MoveParticle(particle, pos, dir3);
            }
            else if (map.IsFreePosition(dir4))
            {
                map.MoveParticle(particle, pos, dir4);
            }
            else if (SurroundedByCount2(pos, ParticleType.Sand, ParticleType.Mud, 1) > SurroundedByCount(pos, ParticleType.Water, 1) + 2)
            {
                //Dry up                
                particle.type = ParticleType.None;
                map[pos] = particle;
            }
        }
    }

    void UpdateSandParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (map.IsFreePosition(bottom))
        {
            map.MoveParticle(particle, pos, bottom);
        }
        else
        {
            //randomize gauche droite
            int2 bottomLeft = new int2(bottom.x - 1, bottom.y);
            int2 bottomRight = new int2(bottom.x + 1, bottom.y);
            bool goingLeft = random.NextBool();
            int2 firstDir = (goingLeft) ? bottomLeft : bottomRight;
            int2 secondDir = (goingLeft) ? bottomRight : bottomLeft;

            if (map.IsFreePosition(firstDir))
            {
                map.MoveParticle(particle, pos, firstDir);
            }
            else if (map.IsFreePosition(secondDir))
            {
                map.MoveParticle(particle, pos, secondDir);
            }
            else if (IsSurroundedBy(pos, ParticleType.Water, 1))
            {   //Sand is touching water, becomes mud
                particle.type = ParticleType.Mud;
                map[pos] = particle;
            }
        }
    }

    void UpdateMudParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (!map.InBound(bottom))
            return;

        if (map.IsFreePosition(bottom))
        {
            map.MoveParticle(particle, pos, bottom);
        }
        else if(map.ParticleTypeAtPosition(bottom) == ParticleType.Water)
        {
            //Mud will sink
            map.SwapParticles(pos, bottom);
        }
        else if (!IsSurroundedBy(pos, ParticleType.Water, 1))
        {   //Mud is not touching water, becomes sand
            particle.type = ParticleType.Sand;
            map[pos] = particle;
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
                    if(map[adjacentPos].type == type)
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
                    if (map[adjacentPos].type == type)
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
                    if (map[adjacentPos].type == type || map[adjacentPos].type == type2)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
}
