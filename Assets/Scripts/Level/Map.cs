
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

    public void AddParticleVelocity(int2 pos, float2 velocity, bool setDirty = true)
    {
        if (!InBound(pos))
            return;

        var particle = particleGrid[pos];
        particle.velocity += velocity;
        particleGrid[pos] = particle;

        if (setDirty)
            dirtyGrid[pos] = true;
    }
    public void SetParticleVelocity(int2 pos, float2 velocity, bool setDirty = true)
    {
        if (!InBound(pos))
            return;

        var particle = particleGrid[pos];
        particle.velocity = velocity;
        particleGrid[pos] = particle;

        if (setDirty)
            dirtyGrid[pos] = true;
    }


    public bool IsParticleDirty(int2 pos)
    {
        return dirtyGrid[pos];
    }

    public Particle GetParticle(int2 pos)
    {
        return particleGrid[pos];
    }
    
    public int2 SlideParticle(int2 from, int2 to)
    {
        to = math.clamp(to, 0, GameManager.GridSizes);

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
            if (HasCollision(nextPosition))
            {
                return currentPosition;
            }
            currentPosition = nextPosition;
        }
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

    public void RemoveSpriteAtPosition(ref PixelSprite sprite, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(sprite.position);
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            SetParticleType(positions[i], ParticleType.None);
        }
        positions.Dispose();
    }

    public void SetSpriteAtPosition(int2 nextPosition, ref PixelSprite sprite, ref PhysicBound physicBound)
    {
        Bound boundPosition = physicBound.GetCollisionBound(nextPosition);

        sprite.position = nextPosition;
        boundPosition.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            SetParticleType(positions[i], ParticleType.Player);
        }
        positions.Dispose();
    }

    public bool TryFindEmptyPosition(int2 position, int2 direction, out int2 newPosition)
    {
        for (int i = 0; i < 32; i++)
        {
            position += direction;
            if (InBound(position))
            {
                if (particleGrid[position].type == ParticleType.None)
                {
                    newPosition = position;
                    return true;
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


    public int2 ApplyGravity(ref PhysicBound physicBound, int2 position, Allocator allocator = Allocator.Temp)
    {
        Bound feetBound = physicBound.GetFeetCollisionBound(position);
        Bound underfeetBound = physicBound.GetUnderFeetCollisionBound(position);
       
        int countAtFeet = CountCollision(ref feetBound, allocator);
        int countUnderFeet = CountCollision(ref underfeetBound, allocator);
   
        //todo dont hardcode
        if (countAtFeet >= 2)
        {
            //Apply ground normal force
            return position + new int2(0, 1);
        }
        else if(countUnderFeet > 0)
        {
            //Stays
            return position;
        }
        else if(countAtFeet == 0 && countUnderFeet == 0 && feetBound.min.y != 0)
        {
            //Apply gravity
            return position - new int2(0, 1);
        }

        return position;
    }

    public int2 Jump(ref PhysicBound physicBound, int2 position, Allocator allocator = Allocator.Temp)
    {
        int2 jumpPosition = position + new int2(0, 1);
        Bound headBound = physicBound.GetTopCollisionBound(jumpPosition);
        if (!HasCollision(ref headBound, allocator))
        {
            return jumpPosition;
        }
        return position;
    }

    public int2 HandlePhysics(ref PhysicBound physicBound, int2 from, int2 to, Allocator allocator = Allocator.Temp)
    {
        int2 desiredPosition = FindDesiredMovePosition(ref physicBound, from, to, allocator);
        if (TryGoPosition(ref physicBound, from, desiredPosition))
        {
            return desiredPosition;
        }

        return from;
    }

    int2 FindDesiredMovePosition(ref PhysicBound physicBound, int2 from, int2 to, Allocator allocator)
    {
        int direction = (to.x - from.x);
        bool goingLeft = direction == -1;

        Bound directionBound;
        if (goingLeft)
        {
            directionBound = physicBound.GetLeftCollisionBound(to);
        }
        else
        {
            directionBound = physicBound.GetRightCollisionBound(to);
        }

        int minY = directionBound.min.y;
        directionBound.GetPositionsGrid(out NativeArray<int2> directionPositions, allocator);
        int2 desiredPosition = to;

        int slopeLimit = 2;
        bool canClimb = false;
        int highestClimbY = 0;
        for (int i = 0; i < directionPositions.Length; i++)
        {
            int2 pos = directionPositions[i];
            if (HasCollision(pos))
            {
                if (pos.y >= minY && pos.y <= minY + slopeLimit)
                {
                    canClimb = true;
                    highestClimbY = math.max(highestClimbY, pos.y + 1);
                }
            }
        }

        if (canClimb)
        {
            desiredPosition.y = highestClimbY;
        }
        return desiredPosition;
    }

    bool TryGoPosition(ref PhysicBound physicBound, int2 from, int2 to)
    {
        int2 pushDirection = math.clamp(to - from, -1, 1);
        Bound bound = physicBound.GetCollisionBound(to);
        //Add push particles

        NativeList<int2> pushedParticlePositions = new NativeList<int2>(Allocator.Temp);
        bool isBlocked = false;
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            int2 position = positions[i];
            int2 pusedPosition = position + pushDirection;

            if(HasCollision(positions[i]))
            {
                bool canPush = CanPush(positions[i]) && IsFreePosition(pusedPosition);
                if (canPush)
                {
                    pushedParticlePositions.Add(positions[i]);
                }
                else 
                {
                    isBlocked = true;
                    break;
                }
            }
        }

        if(!isBlocked)
        {
            for (int i = 0; i < pushedParticlePositions.Length; i++)
            {
                int2 position = pushedParticlePositions[i];
                int2 pusedPosition = position + pushDirection;
                MoveParticle(position, pusedPosition);
            }
        }

        positions.Dispose();
        pushedParticlePositions.Dispose();
        return !isBlocked;
    }

    public bool HasCollision(int2 position)
    {
        return InBound(position) && HasParticleCollision(GetParticleType(position));
    }

    public bool HasParticleCollision(ParticleType type)
    {
        switch (type)
        {
            case ParticleType.None:
                return false;

            case ParticleType.Water:
            case ParticleType.Sand:
            case ParticleType.Snow:
            case ParticleType.Mud:
            case ParticleType.Ice:
            case ParticleType.Rock:
                return true;
 
        }
        return false;
    }

    public bool CanPush(int2 position)
    {
        return CanPush(GetParticleType(position));
    }
    public bool CanPush(ParticleType type)
    {
        switch (type)
        {
            case ParticleType.None:
            case ParticleType.Sand:
            case ParticleType.Snow:
            case ParticleType.Water:
                return true;

            case ParticleType.Mud:
            case ParticleType.Rock:
            case ParticleType.Ice:
                return false;
        }
        return true;
    }

    public bool HasCollision(ref Bound bound, Allocator allocator = Allocator.Temp)
    {
        return CountCollision(ref bound, allocator) > 0;
    }

    public int CountCollision(ref Bound bound, Allocator allocator = Allocator.Temp)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, allocator);
        int count = CountCollision(ref positions);
        positions.Dispose();
        return count;
    }

    public bool HasCollision(ref NativeArray<int2> positions)
    {
        return CountCollision(ref positions) > 0;
    }

    public int CountCollision(ref NativeArray<int2> positions)
    {
        int count = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            if (InBound(positions[i]) && HasParticleCollision(GetParticleType(positions[i])))
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
