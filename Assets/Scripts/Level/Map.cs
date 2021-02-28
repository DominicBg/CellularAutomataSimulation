
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public unsafe struct Map
{
    public int2 Sizes => particleGrid.Sizes;
    public int ArrayLength => Sizes.x * Sizes.y;

    NativeGrid<Particle> particleGrid;
    NativeGrid<bool> dirtyGrid;

    public Map(ParticleType[,] grid, int2 sizes)
    {
        particleGrid = new NativeGrid<Particle>(sizes, Allocator.Persistent);
        dirtyGrid = new NativeGrid<bool>(sizes, Allocator.Persistent);
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int2 pos = new int2(x, y);
                SetParticleType(pos, grid[x, y]);
            }
        }
    }

    public Map(int2 sizes)
    {
        particleGrid = new NativeGrid<Particle>(sizes, Allocator.Persistent);
        dirtyGrid = new NativeGrid<bool>(sizes, Allocator.Persistent);
    }

    public void Dispose()
    {
        particleGrid.Dispose();
        dirtyGrid.Dispose();
    }

    public void ClearDirtyGrid()
    {
        dirtyGrid.Clear();
    }

    public void SetParticleType(int2 pos, ParticleType type, bool setDirty = true)
    {
        SetParticleType(pos, type, 0.5f, setDirty);
    }

    public void SetParticleType(int2 pos, ParticleType type, float2 fracPos, bool setDirty = true)
    {
        if (!InBound(pos))
            return;

        particleGrid[pos] = new Particle() { type = type, velocity = 0, tickIdle = 0, fracPosition = fracPos };
        if(setDirty)
            dirtyGrid[pos] = true;
    }

    public void SetParticle(int2 pos, Particle particle, bool setDirty = true, bool resetTick = false)
    {
        if(resetTick)
            particle.tickIdle = 0;

        particleGrid[pos] = particle;
        if (setDirty)
            dirtyGrid[pos] = true;
    }

    public void MoveParticle(Particle particle, int2 from, int2 to)
    {
        if (math.all(from == to))
            return;

        particle.tickIdle = 0;
        particleGrid[from] = new Particle() { type = ParticleType.None, velocity = 0, tickIdle = 0, fracPosition = 0.5f };
        particleGrid[to] = particle;
        dirtyGrid[to] = true;
    }

    public void SwapParticles(int2 from, int2 to)
    {
        Particle p1 = particleGrid[from];
        Particle p2 = particleGrid[to];
        p1.tickIdle = 0;
        p2.tickIdle = 0;
        particleGrid[from] = p2;
        particleGrid[to] = p1;
        dirtyGrid[to] = true;
    }


    public bool IsParticleDirty(int2 pos)
    {
        return dirtyGrid[pos];
    }

    public Particle GetParticle(int2 pos)
    {
        return particleGrid[pos];
    }
    
    public void UpdateParticleTick(Bound updateBound)
    {
        int2 min = updateBound.bottomLeft;
        int2 max = updateBound.topRight;
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                if(InBound(new int2(x ,y)))
                {
                    Particle p = particleGrid[x, y];
                    p.tickIdle++;
                    particleGrid[x, y] = p;
                }
            }
        }
    }

    public int2 SimulateParticlePhysic(int2 from, int2 to, out bool hasCollision, out int2 collisionPos, in PhysiXVIISetings physiXVIISetings)
    {
        to = math.clamp(to, -1, Sizes);

        int2 diff = to - from;
        int maxSteps = math.abs(diff.x) + math.abs(diff.y);

        int2 currentPosition = from;
        int2 safePosition = from;
        bool useSafety = false;
        float steps = 1f / (maxSteps == 0 ? 1 : maxSteps);
        for (int i = 0; i <= maxSteps; i++)
        {
            int2 nextPosition = (int2)math.lerp(from, to, i * steps);
            if (math.all(currentPosition == nextPosition))
                continue;

            if (!InBound(nextPosition))
            {
                hasCollision = true;
                collisionPos = nextPosition;
                return useSafety ? safePosition : currentPosition;
            }

            if (HasCollision(nextPosition))
            {
                bool skipCollision = GetParticle(nextPosition).InFreeFall() && CanPush(nextPosition, in physiXVIISetings);
                if (skipCollision)
                {
                    useSafety = true;
                    currentPosition = nextPosition;
                    continue;
                }

                hasCollision = true;
                collisionPos = nextPosition;
                //return currentPosition;
                return useSafety ? safePosition : currentPosition;
            }
            safePosition = currentPosition;
            currentPosition = nextPosition;
            useSafety = false;
        }

        hasCollision = false;
        collisionPos = -1;
        return useSafety ? safePosition : currentPosition;
    }

    public int2 FindParticlePosition(int2 from, int2 to, int ignoreFlag = 0)
    {
        if (math.all(from == to))
            return from;

        int2 diff = to - from;
        int maxSteps = math.abs(diff.x) + math.abs(diff.y);

        int2 currentPosition = from;
        int2 safePosition = from;

        float steps = 1f / maxSteps;
        for (int i = 0; i <= maxSteps; i++)
        {
            int2 nextPosition = (int2)math.lerp(from, to, i * steps);
            if (math.all(currentPosition == nextPosition))
                continue;

            ParticleType particleType = GetParticleType(nextPosition);
            if(particleType == ParticleType.None)
            {
                safePosition = currentPosition;
            }
            else if (HasCollision(nextPosition, ignoreFlag))
            {
                return safePosition;
            }

            currentPosition = nextPosition;
        }
        return safePosition;
    }


  

    public bool IsFreePosition(int2 pos)
    {
        return InBound(pos) && particleGrid[pos].type == ParticleType.None;
    }

    public ParticleType GetParticleType(int2 pos)
    {
        if (!InBound(pos))
            return ParticleType.None;
        return particleGrid[pos].type;
    }

    public void RemoveSpriteAtPosition(int2 position, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(position);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            //if we overlap free fall particles, dont erase them
            if (GetParticleType(positions[i]) == ParticleType.Player)
                SetParticleType(positions[i], ParticleType.None);
        }
        positions.Dispose();
    }


    public void SetSpriteAtPosition(int2 nextPosition, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(nextPosition);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            //if we overlap free fall particles, dont erase them
            if (GetParticleType(positions[i]) == ParticleType.None)
                SetParticleType(positions[i], ParticleType.Player);
        }
        positions.Dispose();
    }

    public bool TryFindEmptyPosition(int2 position, int2 direction, out int2 newPosition, int maxStep = 5, int ignoreTypeFlag = 0)
    {
        for (int i = 0; i < maxStep; i++)
        {
            position += direction;
            if (InBound(position))
            {
                ParticleType type = particleGrid[position].type;
                bool canIgnoreType = PhysiXVII.IsInFlag(ignoreTypeFlag, type);

                if (type == ParticleType.None)
                {
                    newPosition = position;
                    return true;
                }
                else if(!canIgnoreType)
                {
                    newPosition = -1;
                    return false;
                }
            }
            else
            {
                break;
            }
        }
        newPosition = -1;
        return false;
    }

    public bool HasCollision(int2 position, int ignoreFlag = 0)
    {
        return InBound(position) && HasParticleCollision(GetParticleType(position), ignoreFlag);
    }

    public bool HasParticleCollision(ParticleType type, int ignoreFlag = 0)
    {
        //can remove  type != ParticleType.None?
        return type != ParticleType.None && !PhysiXVII.IsInFlag(ignoreFlag, type);
    }

    public bool CanPush(int2 position, in PhysiXVIISetings settings)
    {
        ParticleType type = GetParticleType(position);
        return CanPush(type, in settings);
    }
    public bool CanPush(ParticleType type, in PhysiXVIISetings settings)
    {
        int index = (int)type;
        return settings.canPush[index];
    }

    public bool HasCollision(ref Bound bound, int ignoreFlag = 0, Allocator allocator = Allocator.Temp)
    {
        return CountCollision(ref bound, ignoreFlag, allocator) > 0;
    }

    public bool HasCollisionIgnoreFreeFall(ref Bound bound, int ignoreFlag = 0, Allocator allocator = Allocator.Temp)
    {
        return CountCollision(ref bound, ignoreFlag, allocator, ignoreFreeFall : true) > 0;
    }

    public int CountCollision(ref Bound bound, int ignoreFlag = 0, Allocator allocator = Allocator.Temp, bool ignoreFreeFall = false)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, allocator);
        int count = CountCollision(ref positions, ignoreFlag, ignoreFreeFall);
        positions.Dispose();
        return count;
    }

    public int CountCollision(ref NativeArray<int2> positions, int ignoreFlag = 0, bool ignoreFreeFall = false)
    {
        int count = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            if (!InBound(positions[i]))
                continue;

            ParticleType particleType = GetParticleType(positions[i]);
            bool inFreeFall = GetParticle(positions[i]).InFreeFall();
            if (inFreeFall && ignoreFreeFall)
                continue;

            if (HasParticleCollision(particleType, ignoreFlag))
                count++;
        }
        return count;
    }

    public bool InBound(int2 pos)
    {
        return GridHelper.InBound(pos, Sizes);
    }

    public bool InBound(Bound bound)
    {
        return GridHelper.InBound(bound, Sizes);
    }
}
