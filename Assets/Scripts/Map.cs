
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public unsafe struct Map
{
    [NativeDisableUnsafePtrRestriction]
    public void* buffer;
    public int2 sizes;
    public int ArrayLenght => sizes.x * sizes.y;
    public Map(int2 sizes)
    {
        int size = (sizes.x * sizes.y) * sizeof(Particle);
        buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<Particle>(), Allocator.Persistent);
        UnsafeUtility.MemClear(buffer, size);
        this.sizes = sizes;
    }

    public void Dispose()
    {
        UnsafeUtility.Free(buffer, Allocator.Persistent);
        buffer = null;
    }

    public unsafe Particle this[int2 index2]
    {
        get
        {
            if(!InBound(index2))
                throw new ArgumentOutOfRangeException("Don't you ever try to go out of bound again, this is unsafe :@");

            int index = PosToIndex(index2) * sizeof(Particle);
            return UnsafeUtility.ReadArrayElement<Particle>(buffer, index);
        }
        set
        {
            if (!InBound(index2))
                throw new ArgumentOutOfRangeException("Don't you ever try to go out of bound again, this is unsafe :@");

            int index = PosToIndex(index2) * sizeof(Particle);
            UnsafeUtility.WriteArrayElement(buffer, index, value);
        }
    }
    
    public void MoveParticle(NativeArray<Particle> particles, Particle particle, int2 from, int2 to)
    {
        int fromI = PosToIndex(from);
        int toI = PosToIndex(to);

        particles[fromI] = new Particle() { type = ParticleType.None };
        particles[toI] = particle;
    }

    public void SwapParticles(NativeArray<Particle> particles, int2 from, int2 to)
    {
        int fromI = PosToIndex(from);
        int toI = PosToIndex(to);
        Particle temp = particles[fromI];
        particles[fromI] = particles[toI];
        particles[toI] = temp;
    }

    public bool IsFreePosition(NativeArray<Particle> particles, int2 pos)
    {
        int i = PosToIndex(pos);
        return InBound(pos) && particles[i].type == ParticleType.None;
    }

    public ParticleType ParticleTypeAtPosition(NativeArray<Particle> particles, int2 pos)
    {
        int i = PosToIndex(pos);
        return particles[i].type;
    }

    public bool InBound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < sizes.x && pos.y < sizes.y;
    }

    public void SetSpriteAtPosition(NativeArray<Particle> particles, int2 previousPosition, int2 position, ref PixelSprite sprite)
    {
        //Cleanup old position
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int i = PosToIndex(previousPosition.x + x, previousPosition.y + y);
                if(sprite.collisions[x, y])
                    particles[i] = new Particle() { type = ParticleType.None };
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
                    int i = PosToIndex(nextPosition);

                    //Throw particle in the air
                    ParticleType previousType = particles[i].type;
                    if (previousType != ParticleType.None && previousType != ParticleType.Player && TryFindEmptyPosition(particles, nextPosition, new int2(0, -1), out int2 newPosition))
                    { 
                        int newI = PosToIndex(newPosition);
                        particles[newI] = new Particle() { type = previousType };
                    }

                    particles[i] = new Particle() { type = ParticleType.Player };
                }
            }
        }
    }

    public bool TryFindEmptyPosition(NativeArray<Particle> particles, int2 position, int2 direction, out int2 newPosition)
    {
        for (int i = 0; i < 32; i++)
        {
            position += direction;
            int index = PosToIndex(position);
            if(InBound(position))
            {
                if(particles[index].type == ParticleType.None)
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
        newPosition =  -1;
        return false;
    }

    public int PosToIndex(int2 pos)
    {
        return pos.y * sizes.x + pos.x;
    }
    public int PosToIndex(int x, int y)
    {
        return y * sizes.x + x;
    }
}
