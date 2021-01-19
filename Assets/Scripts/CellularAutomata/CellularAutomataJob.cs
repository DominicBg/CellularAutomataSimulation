using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static ParticleBehaviour;

[BurstCompile]
public struct CellularAutomataJob : IJob
{
    public NativeArray<ParticleSpawner> nativeParticleSpawners;
    public TickBlock tickBlock;
    public Map map;
    public ParticleBehaviour behaviour;
    public PhysiXVIISetings settings;
    public float deltaTime;

    public Bound updateBound;

    //output event
    public NativeList<int2> particleSmokeEvent;

    public void Execute()
    {
        particleSmokeEvent.Clear();

        map.ClearDirtyGrid();
        map.UpdateParticleTick(updateBound);
        UpdateSimulation();

        SpawnParticles();
    }

    void SpawnParticles()
    {
        for (int i = 0; i < nativeParticleSpawners.Length; i++)
        {
            var spawner = nativeParticleSpawners[i];
            bool canEmit = spawner.particleSpawnCount != 0;

            if (canEmit && tickBlock.random.NextFloat() <= spawner.chanceSpawn && map.IsFreePosition(spawner.spawnPosition))
            {
                spawner.particleSpawnCount--;
                map.SetParticleType(spawner.spawnPosition, spawner.particleType);
                nativeParticleSpawners[i] = spawner;
            }
        }
    }

    void UpdateSimulation()
    {
        int2 start = updateBound.bottomLeft;
        int2 end = updateBound.topRight;

       // if (tickBlock.tick % 2 == 0)
        {
            for (int x = start.x; x < end.x; x++)
            {
                for (int y = start.y; y < end.y; y++)
                {
                    int2 pos = new int2(x, y);
                    if(map.InBound(pos))
                        UpdateParticleBehaviour(pos);
                }
            }
        }
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
            case ParticleType.Rock:
                break;
            case ParticleType.Rubble:
                UpdateRubbleParticle(particle, pos);
                break;
            case ParticleType.Fire:
                //todo
                break;
            case ParticleType.Cinder:
                UpdateCinderParticle(particle, pos);
                break;
            case ParticleType.Wood:
                UpdateWoodParticle(particle, pos);
                break;
            case ParticleType.String:
                UpdateStringParticle(particle, pos);
                break;

            //Others
            case ParticleType.TitleDisintegration:
                UpdateTitleDisintegrationPartaicle(particle, pos);
                break;
            case ParticleType.Player:
                //nada
                break;
            case ParticleType.Count:
                break;
        }
    }

    unsafe bool TryFreeFalling(Particle particle, int2 pos)
    {
        particle.velocity += settings.gravity * deltaTime;
        float2 desiredPosition = (pos + particle.fracPosition) + (particle.velocity * deltaTime);
        int2 desiredGridPosition = (int2)desiredPosition;

        bool samePosition = math.all(pos == desiredGridPosition);
        if(samePosition)
        {
            particle.fracPosition = math.frac(desiredPosition);

            //used to update the velocity
            map.SetParticle(pos, particle);
            return true;
        }

        int2 slidePosition = map.SimulateParticlePhysic(pos, desiredGridPosition, out bool hasCollision, out int2 collisionPosition);

        if (!math.all(pos == slidePosition))
        {
            //stop velocity on collision
            if (hasCollision)
            {
                if(map.InBound(collisionPosition))
                {
                    ref Particle p1 = ref particle;
                    Particle p2 = map.GetParticle(collisionPosition);
                    PhysiXVII.CalculateParticleCollisions(ref p1, ref p2, slidePosition, collisionPosition, in settings);

                    map.SetParticle(collisionPosition, p2, false);
                }
                else //WALL COLLISION
                {
                    Particle p1 = particle;
                    int wallMass = 1000;
                    PhysiXVII.ComputeElasticCollision(slidePosition, collisionPosition, p1.velocity, 0, 1, wallMass, out float2 outv1, out float2 outv2);
                    particle.velocity = outv1 * 0.1f;
                }

                //Had a collision, set frac position to middle of the gridcell
                particle.fracPosition = 0.5f;
            }
            else
            {
                particle.fracPosition = math.frac(desiredPosition);
            }

            map.MoveParticle(particle, pos, slidePosition);
            return true;
        }

        //reset velocity, its not free falling
        particle.velocity *= settings.frictions[(int)particle.type] * settings.absorbtion[(int)particle.type];
        map.SetParticle(pos, particle, false);

        return particle.InFreeFall();
    }

    bool TryFloatyFalling(Particle particle, int2 pos)
    {
        var floaty = behaviour.floaty;
        bool willFloat = tickBlock.random.NextFloat() < floaty.ratioFloat;

        int2 bottom = new int2(pos.x, pos.y - 1);
        int2 bottomLeft = new int2(pos.x - 1, pos.y - 1);
        int2 bottomRight = new int2(pos.x + 1, pos.y - 1);
        if (!map.IsFreePosition(bottom) && !map.IsFreePosition(bottomLeft) && !map.IsFreePosition(bottomRight))
            return false;

        if (willFloat)
        {
            float2 offset = pos * floaty.sinOffset;
            float sin = math.sin(tickBlock.tick * floaty.sinSpeed + offset.x + offset.y);
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
            return TryFreeFalling(particle, pos);
        }
        return false;
    }


    bool TryUpdateFluidParticle(Particle particle, int2 pos)
    {
        int2 left = new int2(pos.x - 1, pos.y);
        int2 right = new int2(pos.x + 1, pos.y);

        bool goingLeft = tickBlock.random.NextBool();

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
        bool goingLeft = tickBlock.random.NextBool();
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

    bool TryUpdateSticking(Particle particle, int2 pos, int stickingFlag)
    {
        return IsSurroundedBy(pos, stickingFlag);
    }


    void UpdateWaterParticle(Particle particle, int2 pos)
    {
        bool updateFalling = TryFreeFalling(particle, pos);
        if (updateFalling)
            return;
        bool updateFluid = TryUpdateFluidParticle(particle, pos);
        if (updateFluid)
            return;

        int sandMudCount = SurroundedByCount(pos, (int)ParticleType.Sand | (int)ParticleType.Mud);
        int waterCount = SurroundedByCount(pos, ParticleType.Water);
        //Unique water behaviour
        if (sandMudCount > waterCount + behaviour.water.diffWaterSandToDry)
        {
            //Dry up                
            map.SetParticleType(pos, ParticleType.None, setDirty: false);
        }    
    }

    void UpdateSandParticle(Particle particle, int2 pos)
    {
        bool falling = TryFreeFalling(particle, pos);
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

    void UpdateRubbleParticle(Particle particle, int2 pos)
    {
        bool falling = TryFreeFalling(particle, pos);
        if (falling)
            return;

        bool updatePiling = TryUpdatePilingUpParticle(particle, pos);
        if (updatePiling)
            return;
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
        bool falling = TryFreeFalling(particle, pos);
        if (falling)
            return;
        bool updateSinking = TryUpdateSinkingParticle(particle, pos);
        if (updateSinking)
            return;
        
        //Add melting
    }


    void UpdateMudParticle(Particle particle, int2 pos)
    {
        bool falling = TryFreeFalling(particle, pos);
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

    void UpdateWoodParticle(Particle particle, int2 pos)
    {
        var surrounding = GetSurroundingParticle(pos);
        for (int i = 0; i < surrounding.Length; i++)
        {
            if(surrounding[i].type == ParticleType.Cinder && surrounding[i].tickIdle > behaviour.woodBehaviour.tickBeforeTurnToCinder)
            {
                map.SetParticleType(pos, ParticleType.Cinder);
                particleSmokeEvent.Add(pos);
            }
        }
        surrounding.Dispose();
    }
    void UpdateStringParticle(Particle particle, int2 pos)
    {
        var surrounding = GetSurroundingParticle(pos);
        for (int i = 0; i < surrounding.Length; i++)
        {
            if (surrounding[i].type == ParticleType.Cinder && surrounding[i].tickIdle > behaviour.stringBehaviour.tickBeforeTurnToCinder)
            {
                map.SetParticleType(pos, ParticleType.Cinder);
                particleSmokeEvent.Add(pos);
            }
        }
        surrounding.Dispose();
    }


    void UpdateCinderParticle(Particle particle, int2 pos)
    {
        bool sticking = TryUpdateSticking(particle, pos, PhysiXVII.GetFlag(ParticleType.Wood));
        if (sticking)
            return;

        bool falling = TryFreeFalling(particle, pos);
        if (falling)
            return;

        bool updatePiling = TryUpdatePilingUpParticle(particle, pos);
        if (updatePiling)
            return;

        var cinderBehaviour = behaviour.cinderBehaviour;
        if (SurroundedByCount(pos, ParticleType.Cinder) < cinderBehaviour.minimumSurroundingCinder && particle.tickIdle > cinderBehaviour.tickBeforeDisapear)
        {
            map.SetParticleType(pos, ParticleType.None);
        }
    }


    void UpdateTitleDisintegrationPartaicle(Particle particle, int2 pos)
    {
        //todo put in behaviour
        bool willUpdate = tickBlock.random.NextFloat() < behaviour.titleDisentegrate.chanceMove;
        bool willDisapear = tickBlock.random.NextFloat() < behaviour.titleDisentegrate.chanceDespawn;

        if (willDisapear)
        {
            map.SetParticleType(pos, ParticleType.None);
            return;
        }

        int2 floatingDirection = new int2(-1, 0);
        int2 newPosition = pos + floatingDirection;
        if (willUpdate && map.IsFreePosition(newPosition))
        {
            map.MoveParticle(particle, pos, newPosition);
        }
    }

    NativeList<Particle> GetSurroundingParticle(int2 pos, int range = 1, Allocator allocator = Allocator.Temp)
    {
        NativeList<Particle> particles = new NativeList<Particle>(allocator);
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == y)
                    continue;

                int2 adjacentPos = pos + new int2(x, y);
                if (map.InBound(adjacentPos))
                {
                    particles.Add(map.GetParticle(adjacentPos));
                }
            }
        }
        return particles;
    }

    bool IsSurroundedBy(int2 pos, ParticleType particleType, int range = 1)
    {
        return IsSurroundedBy(pos, PhysiXVII.GetFlag(particleType), range);
    }

    bool IsSurroundedBy(int2 pos, int particleFlag, int range = 1)
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
                    ParticleType type = map.GetParticleType(adjacentPos);
                    if (PhysiXVII.IsInFlag(particleFlag, type))
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
        return SurroundedByCount(pos, PhysiXVII.GetFlag(type), range);
    }

    int SurroundedByCount(int2 pos, int flag, int range = 1)
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
                    ParticleType type = map.GetParticleType(adjacentPos);
                    if (PhysiXVII.IsInFlag(flag, type))
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
}
