
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

    public void SetSpriteAtPosition(int2 previousPosition, ref PixelSprite sprite)
    {
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
                int2 nextPosition = sprite.position + new int2(x, y);
                if (sprite.collisions[x, y])
                {
                    //Throw particle in the air
                    ParticleType previousType = particleGrid[nextPosition].type;
                    if (previousType != ParticleType.None && previousType != ParticleType.Player && TryFindEmptyPosition(nextPosition, new int2(0, -1), out int2 newPosition))
                    {
                        particleGrid[newPosition] = new Particle() { type = previousType };
                        dirtyGrid[newPosition] = true;

                    }

                    particleGrid[nextPosition] = new Particle() { type = ParticleType.Player };
                    dirtyGrid[nextPosition] = true;

                }
            }
        }
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

    public bool InBound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Sizes.x && pos.y < Sizes.y;
    }
}
