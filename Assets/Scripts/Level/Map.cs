﻿
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

        particleGrid[pos] = new Particle() { type = type };
        if(setDirty)
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

    public void MoveParticle(Particle particle, int2 from, int2 to)
    {
        particleGrid[from] = new Particle() { type = ParticleType.None };
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

    public void SetSpriteAtPosition(int2 nextPosition, ref PixelSprite sprite)
    {
        int2 previousPosition = sprite.position;
        //Cleanup old position
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 newPos = previousPosition + new int2(x, y);
                if (sprite.collisions[x, y])
                    particleGrid[newPos] = new Particle() { type = ParticleType.None };
            }
        }

        //Place new pixels
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 pixelPosition = nextPosition + new int2(x, y);
                if (sprite.collisions[x, y])
                {
                    //Throw particle in the air
                    ParticleType previousType = particleGrid[pixelPosition].type;
                    if (previousType != ParticleType.None && previousType != ParticleType.Player && TryFindEmptyPosition(pixelPosition, new int2(0, 1), out int2 newPosition))
                    {
                        SetParticleType(newPosition, previousType);
                    }

                    SetParticleType(pixelPosition, ParticleType.Player);
                }
            }
        }

        sprite.position = nextPosition;
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
        //Didnt move
        if (math.all(from == to))
            return to;

        bool goingLeft = (from.x - to.x) == 1;

        Bound directionBound;
        if(goingLeft)
        {
            directionBound = physicBound.GetLeftCollisionBound(to);
        }
        else
        {
            directionBound = physicBound.GetRightCollisionBound(to);
        }

        int minY = directionBound.min.y;

        directionBound.GetPositionsGrid(out NativeArray<int2> directionPositions, allocator);

        int2 finalPosition = to;
        for (int i = 0; i < directionPositions.Length; i++)
        {
            int2 pos = directionPositions[i];
            ParticleType type = GetParticleType(pos);
            if (HasParticleCollision(type))
            {
                if (pos.y >= minY && pos.y <= minY + 2)
                {
                    //blocked from the bottom, walk ontop
                    int2 newPosition = new int2(to.x, pos.y + 1);
                    Bound headBound = physicBound.GetTopCollisionBound(newPosition);
                    if(!HasCollision(ref headBound, allocator))
                    {
                        finalPosition = newPosition;
                        break;
                    }
                }
                else
                {
                    //Slope too high, can't move
                    finalPosition = from;
                    break;
                }
            }
        }
        directionPositions.Dispose();
        return finalPosition;
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
        return ArrayHelper.InBound(pos, Sizes);
    }

    public bool InBound(Bound bound)
    {
        return ArrayHelper.InBound(bound, Sizes);
    }
}
