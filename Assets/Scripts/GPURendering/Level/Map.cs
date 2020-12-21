
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public unsafe struct Map
{
    public int2 Sizes => particleGrid.m_sizes;
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
        if (!InBound(pos))
            return;

        particleGrid[pos] = new Particle() { type = type, velocity = 0 };
        if(setDirty)
            dirtyGrid[pos] = true;
    }


    public bool IsParticleDirty(int2 pos)
    {
        return dirtyGrid[pos];
    }

    public void SetParticle(int2 pos, Particle particle, bool setDirty = true)
    {
        particleGrid[pos] = particle;
        if (setDirty)
            dirtyGrid[pos] = true;
    }

    public Particle GetParticle(int2 pos)
    {
        return particleGrid[pos];
    }
    
    public int2 SlideParticle(int2 from, int2 to, out bool hasCollision, out int2 collisionPos)
    {
        to = math.clamp(to, -1, GameManager.GridSizes);

        int2 diff = to - from;
        int distance = math.abs(diff.x) + math.abs(diff.y);

        int2 currentPosition = from;
        for (int i = 0; i < distance; i++)
        {
            diff = to - currentPosition;
            int2 step = math.clamp(diff, -1, 1);

            //Make sure we dont step in diagonals
            if(math.all(math.abs(step) == 1))
            {
                if (i % 2 == 0)
                    step.y = 0;
                else
                    step.x = 0;
            }

            int2 nextPosition = currentPosition + step;
            if (!InBound(nextPosition) || HasCollision(nextPosition))
            {
                hasCollision = true;
                collisionPos = nextPosition;
                return currentPosition;
            }
            currentPosition = nextPosition;
        }
        hasCollision = false;
        collisionPos = currentPosition;
        return currentPosition;
    }

    public void MoveParticle(int2 from, int2 to)
    {
        Particle temp = particleGrid[from];
        particleGrid[from] = new Particle() { type = ParticleType.None, velocity = 0 };
        particleGrid[to] = temp;
        dirtyGrid[to] = true;
    }

    public void MoveParticle(Particle particle, int2 from, int2 to)
    {
        particleGrid[from] = new Particle() { type = ParticleType.None, velocity = 0 };
        particleGrid[to] = particle;
        dirtyGrid[to] = true;
    }

    public void SwapParticles(int2 from, int2 to)
    {
        Particle temp = particleGrid[from];
        particleGrid[from] = particleGrid[to];
        particleGrid[to] = temp;
        dirtyGrid[to] = true;
    }

    public bool IsFreePosition(int2 pos)
    {
        return InBound(pos) && particleGrid[pos].type == ParticleType.None;
    }

    public ParticleType GetParticleType(int2 pos)
    {
        return particleGrid[pos].type;
    }

    public void RemoveSpriteAtPosition(int2 position, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(position);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            SetParticleType(positions[i], ParticleType.None);
        }
        positions.Dispose();
    }

    public void SetPlayerAtPosition(int2 nextPosition, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(nextPosition);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            SetParticleType(positions[i], ParticleType.Player);
        }
        positions.Dispose();
    }



    public void SetSpriteAtPosition(int2 nextPosition, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(nextPosition);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
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
                bool canIgnoreType = ((int)type & ignoreTypeFlag) != 0;

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

    public bool HasCollision(int2 position)
    {
        return InBound(position) && HasParticleCollision(GetParticleType(position));
    }

    public bool HasParticleCollision(ParticleType type)
    {
        return type != ParticleType.None;
        //switch (type)
        //{
        //    case ParticleType.None:
        //    case ParticleType.Player:
        //        return false;
        //}
        //return true;
    }

    public bool CanPush(int2 position, in PhysiXVIISetings settings)
    {
        return CanPush(GetParticleType(position), in settings);
    }
    public bool CanPush(ParticleType type, in PhysiXVIISetings settings)
    {
        return settings.canPush[(int)type];
    }

    public bool HasCollision(ref Bound bound, int ignoreFlag = 0, Allocator allocator = Allocator.Temp)
    {
        return CountCollision(ref bound, ignoreFlag, allocator) > 0;
    }

    public int CountCollision(ref Bound bound, int ignoreFlag = 0, Allocator allocator = Allocator.Temp)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, allocator);
        int count = CountCollision(ref positions, ignoreFlag);
        positions.Dispose();
        return count;
    }

    public bool HasCollision(ref NativeArray<int2> positions, int ignoreFlag = 0)
    {
        return CountCollision(ref positions, ignoreFlag) > 0;
    }

    public int CountCollision(ref NativeArray<int2> positions, int ignoreFlag = 0)
    {
        int count = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            if (!InBound(positions[i]))
                continue;

            ParticleType particleType = GetParticleType(positions[i]);
            bool ignoreCollision = ((int)particleType & ignoreFlag) != 0;
            if (!ignoreCollision && HasParticleCollision(particleType))
            {
                count++;
            }
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
