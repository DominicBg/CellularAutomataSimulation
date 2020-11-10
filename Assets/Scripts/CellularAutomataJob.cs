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
        map.ClearDirtyGrid();
        UpdateSimulation();
        SpawnParticles();
    }

    void SpawnParticles()
    {
        for (int i = 0; i < nativeParticleSpawners.Length; i++)
        {
            var spawner = nativeParticleSpawners[i];
            if (random.NextFloat() <= spawner.chanceSpawn)
            {
                map.SetParticleType(spawner.spawnPosition, spawner.particleType);
            }
        }
    }

    void UpdateSimulation()
    {
        //for (int x = 0; x < map.Sizes.x; x++)
        //{
        //    for (int y = 0; y < map.Sizes.y; y++)
        //    {
        //        int2 pos = new int2(x, y);
        //        UpdateParticleBehaviour(pos);
        //    }
        //}

        for (int x = map.Sizes.x - 1; x >= 0; x--)
        {
            for (int y = map.Sizes.y - 1; y >= 0; y--)
            {
                UpdateParticleBehaviour(new int2(x, y));
            }
        }
    }

    void UpdateParticleBehaviour(int2 pos)
    {
        if (map.IsParticleDirty(pos))
            return;

        Particle particle = map.GetParticle(pos);
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

            bool goingLeft = random.NextBool();

            int2 dir1 = (goingLeft) ? left : right;
            int2 dir2 = (goingLeft) ? right : left;
  
            if (map.IsFreePosition(dir1))
            {
                map.MoveParticle(particle, pos, dir1);
            }
            else if (map.IsFreePosition(dir2))
            {
                map.MoveParticle(particle, pos, dir2);
            }
            else if (SurroundedByCount(pos, (int)ParticleType.Sand | (int)ParticleType.Mud, 1) > SurroundedByCount(pos, ParticleType.Water, 1) + 2)
            {
                //Dry up                
                map.SetParticleType(pos, ParticleType.None, setDirty: false);
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
            {   
                //Sand is touching water, becomes mud
                map.SetParticleType(pos, ParticleType.Mud);
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
        else if(map.GetParticleType(bottom) == ParticleType.Water)
        {
            //Mud will sink
            map.SwapParticles(pos, bottom);
        }
        else if (!IsSurroundedBy(pos, ParticleType.Water, 1))
        {   
            //Mud is not touching water, becomes sand
            map.SetParticleType(pos, ParticleType.Sand);
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
                if (map.InBound(adjacentPos))
                {
                    if (type == map.GetParticleType(adjacentPos))
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
        return SurroundedByCount(pos, (int)type, range);
    }

    int SurroundedByCount(int2 pos, int type, int range = 1)
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
                    if ((type & (int)map.GetParticleType(adjacentPos)) != 0)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
}
