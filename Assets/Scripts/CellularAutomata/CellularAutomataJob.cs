using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CellularAutomataJob : IJob
{
    public NativeArray<ParticleSpawner> nativeParticleSpawners;

    public int tick;
    public Map map;
    public Random random;
    public ParticleBehaviour behaviour;
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
            bool canEmit = spawner.notTickBounded || (tick >= spawner.startTick && tick <= spawner.endTick);

            if (canEmit && random.NextFloat() <= spawner.chanceSpawn)
            {
                map.SetParticleType(spawner.spawnPosition, spawner.particleType);
            }
        }
    }

    void UpdateSimulation()
    {
        //if(tick % 2 == 0)
        //{
            for (int x = 0; x < map.Sizes.x; x++)
            {
                for (int y = 0; y < map.Sizes.y; y++)
                {
                    int2 pos = new int2(x, y);
                    UpdateParticleBehaviour(pos);
                }
            }
        //}
        //else
        //{
        //    for (int x = map.Sizes.x - 1; x >= 0; x--)
        //    {
        //        for (int y = map.Sizes.y - 1; y >= 0; y--)
        //        {
        //            UpdateParticleBehaviour(new int2(x, y));
        //        }
        //    }
        //}
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
            case ParticleType.Snow:
                UpdateSnowParticle(particle, pos);
                break;
            case ParticleType.Ice:
                UpdateIceParticle(particle, pos);
                break;
            case ParticleType.Player:
                //nada
                break;
            case ParticleType.Rock:
                break;
            case ParticleType.TitleDisintegration:
                UpdateTitleDisintegrationPartaicle(particle, pos);
                break;
        }
    }

    bool TryStraightFalling(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (map.IsFreePosition(bottom))
        {
            map.MoveParticle(particle, pos, bottom);
            return true;
        }
        return false;
    }

    bool TryFloatyFalling(Particle particle, int2 pos)
    {
        var floaty = behaviour.floatyBehaviour;
        bool willFloat = random.NextFloat() < floaty.ratioFloat;

        int2 bottom = new int2(pos.x, pos.y - 1);
        int2 bottomLeft = new int2(pos.x - 1, pos.y - 1);
        int2 bottomRight = new int2(pos.x + 1, pos.y - 1);
        if (!map.IsFreePosition(bottom) && !map.IsFreePosition(bottomLeft) && !map.IsFreePosition(bottomRight))
            return false;

        if (willFloat)
        {
            float2 offset = pos * floaty.sinOffset;
            float sin = math.sin(tick * floaty.sinSpeed + offset.x + offset.y);
            bool goingLeft = math.sign(sin) == -1;

            int2 left = new int2(pos.x - 1, pos.y);
            int2 right = new int2(pos.x + 1, pos.y);
            int2 dir = (goingLeft) ? left : right;

            if (map.IsFreePosition(dir))
            {
                map.MoveParticle(particle, pos, dir);
                return true;
            }
        }
        else 
        {
            return TryStraightFalling(particle, pos);
        }
        return false;
    }


    bool TryUpdateFluidParticle(Particle particle, int2 pos)
    {
        int2 left = new int2(pos.x - 1, pos.y);
        int2 right = new int2(pos.x + 1, pos.y);

        bool goingLeft = random.NextBool();

        int2 dir1 = (goingLeft) ? left : right;
        int2 dir2 = (goingLeft) ? right : left;

        if (map.IsFreePosition(dir1))
        {
            map.MoveParticle(particle, pos, dir1);
            return true;
        }
        else if (map.IsFreePosition(dir2))
        {
            map.MoveParticle(particle, pos, dir2);
            return true;
        }    
        return false;
    }

    bool TryUpdatePilingUpParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);

        //randomize gauche droite
        int2 bottomLeft = new int2(bottom.x - 1, bottom.y);
        int2 bottomRight = new int2(bottom.x + 1, bottom.y);
        bool goingLeft = random.NextBool();
        int2 firstDir = (goingLeft) ? bottomLeft : bottomRight;
        int2 secondDir = (goingLeft) ? bottomRight : bottomLeft;

        if (map.IsFreePosition(firstDir))
        {
            map.MoveParticle(particle, pos, firstDir);
            return true;
        }
        else if (map.IsFreePosition(secondDir))
        {
            map.MoveParticle(particle, pos, secondDir);
            return true;
        }
        return false;
    }

    bool TryUpdateSinkingParticle(Particle particle, int2 pos)
    {
        int2 bottom = new int2(pos.x, pos.y - 1);
        if (!map.InBound(bottom))
            return false;

        if (map.GetParticleType(bottom) == ParticleType.Water)
        {
            map.SwapParticles(pos, bottom);
            return true;
        }
        return false;
    }

    void UpdateWaterParticle(Particle particle, int2 pos)
    {
        bool updateFalling = TryStraightFalling(particle, pos);
        if (updateFalling)
            return;
        bool updateFluid = TryUpdateFluidParticle(particle, pos);
        if (updateFluid)
            return;

        int sandMudCount = SurroundedByCount(pos, (int)ParticleType.Sand | (int)ParticleType.Mud);
        int waterCount = SurroundedByCount(pos, ParticleType.Water);
        //Unique water behaviour
        if (sandMudCount > waterCount + behaviour.waterBehaviour.diffWaterSandToDry)
        {
            //Dry up                
            map.SetParticleType(pos, ParticleType.None, setDirty: false);
        }    
    }

    void UpdateSandParticle(Particle particle, int2 pos)
    {
        bool falling = TryStraightFalling(particle, pos);
        if (falling)
            return;
        bool updatePiling = TryUpdatePilingUpParticle(particle, pos);
        if (updatePiling)
            return;
        
        //Unique sand behaviour
        if (IsSurroundedBy(pos, ParticleType.Water))
        {
            //Sand is touching water, becomes mud
            map.SetParticleType(pos, ParticleType.Mud);
        }
    }

    void UpdateSnowParticle(Particle particle, int2 pos)
    {
        bool falling = TryFloatyFalling(particle, pos);
        if (falling)
            return;
        bool updatePiling = TryUpdatePilingUpParticle(particle, pos);
        if (updatePiling)
            return;

        //Unique Snow behaviour
        if (IsSurroundedBy(pos, ParticleType.Water))
        {
            map.SetParticleType(pos, ParticleType.Ice);
            return;
        }
    }
    void UpdateIceParticle(Particle particle, int2 pos)
    {
        bool falling = TryStraightFalling(particle, pos);
        if (falling)
            return;
        bool updateSinking = TryUpdateSinkingParticle(particle, pos);
        if (updateSinking)
            return;
        
        //Add melting
    }


    void UpdateMudParticle(Particle particle, int2 pos)
    {
        bool falling = TryStraightFalling(particle, pos);
        if (falling)
            return;
        bool updateSinking = TryUpdateSinkingParticle(particle, pos);
        if (updateSinking)
            return;

        //Unique mud behaviour
        if (!IsSurroundedBy(pos, ParticleType.Water, 1))
        {   
            //Mud is not touching water, becomes sand
            map.SetParticleType(pos, ParticleType.Sand);
        }       
    }

    void UpdateTitleDisintegrationPartaicle(Particle particle, int2 pos)
    {
        //todo put in behaviour
        bool willUpdate = random.NextFloat() < 0.1f;
        bool willDisapear = random.NextFloat() < 0.005f;

        if(willDisapear)
        {
            map.SetParticleType(pos, ParticleType.None);
            return;
        }

        int2 floatingDirection = new int2(-1, 0);
        int2 newPosition = pos + floatingDirection;
        if (willUpdate && map.IsFreePosition(newPosition))
        {
            map.MoveParticle(pos, newPosition);
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
