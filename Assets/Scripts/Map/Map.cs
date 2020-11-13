
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


    public int2 ApplyGravity(ref PixelSprite sprite)
    {
        Bound bound = sprite.Bound;
        //same for left right
        int y = bound.bottomLeft.y;
        int xMin = bound.bottomLeft.x;
        int xMax = bound.bottomRight.x;
        for (int x = xMin; x <= xMax; x++)
        {
            int2 position = new int2(x, y - 1);
            if(InBound(position))
            {
                //At least one collision
                if(HasParticleCollision(GetParticleType(position)))
                {
                    return sprite.position;
                }
            }
            else 
            {
                //bottom of the map
                //return normal position
                return sprite.position;
            }
        }
        //Drop one pixel
        return sprite.position - new int2(0, 1);
    }

    public int2 HandlePhysics(ref PixelSprite sprite, int2 from, int2 to)
    {
        Bound bound = sprite.MovingBound(to);

        bool goingLeft = (from.x - to.x) == 1;

        //same for left or right
        int minY = bound.bottomLeft.y;
        int maxY = bound.topLeft.y;
        int x;
        if(goingLeft)
        {
            x = bound.bottomLeft.x;
        }
        else
        {
            x = bound.bottomRight.x;
        }

        //Scan from top to bottom
        for (int y = maxY; y >= minY; y--)
        {
            int2 pos = new int2(x, y);
            ParticleType type = GetParticleType(pos);
            if (HasParticleCollision(type))
            {
                //if(y == minY || y == minY + 1)
                if(y >= minY && y <= minY + 2)
                {
                    //blocked from the bottom, walk ontop
                    return new int2(to.x, y + 1);
                }
                else
                {
                    //Blocked from top
                    return from;
                }
            }
        }
        return to;
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
            case ParticleType.Ice:
                return false;
        }
        return true;
    }

    public bool InBound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Sizes.x && pos.y < Sizes.y;
    }

    public bool InBound(Bound bound)
    {
        return InBound(bound.topLeft) && InBound(bound.topRight) && InBound(bound.bottomLeft) && InBound(bound.bottomRight);
    }
}
