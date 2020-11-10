
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public unsafe struct Map
{
    [NativeDisableUnsafePtrRestriction]
    public void* particlesBuffer;
    public int2 sizes;
    public int ArrayLength => sizes.x * sizes.y;

    public Map(int2 sizes)
    {
        int size = (sizes.x * sizes.y) * sizeof(Particle);
        this.sizes = sizes;

        particlesBuffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<Particle>(), Allocator.Persistent);
        UnsafeUtility.MemClear(particlesBuffer, size);
    }

    public void Dispose()
    {
        UnsafeUtility.Free(particlesBuffer, Allocator.Persistent);
        particlesBuffer = null;
    }

    public unsafe Particle this[int index]
    {
        get
        {
            if (index < 0 || index >= ArrayLength)
                throw new ArgumentOutOfRangeException("Don't you ever try to write out of bound again, this is unsafe :@ at " + index);

            return UnsafeUtility.ReadArrayElement<Particle>(particlesBuffer, index);
        }
        set
        {
            if (index < 0 || index >= ArrayLength)
                throw new ArgumentOutOfRangeException("Don't you ever try to write out of bound again, this is unsafe :@ at " + index);

            UnsafeUtility.WriteArrayElement(particlesBuffer, index, value);
        }
    }


    public unsafe Particle this[int2 index2]
    {
        get
        {
            if (!InBound(index2))
                throw new ArgumentOutOfRangeException("Don't you ever try to read out of bound again, this is unsafe :@ " + index2);

            int index = ArrayHelper.PosToIndex(index2, sizes);
            return UnsafeUtility.ReadArrayElement<Particle>(particlesBuffer, index);
        }
        set
        {
            if (!InBound(index2))
                throw new ArgumentOutOfRangeException("Don't you ever try to write out of bound again, this is unsafe :@ at " + index2);

            int index = ArrayHelper.PosToIndex(index2, sizes);
            UnsafeUtility.WriteArrayElement(particlesBuffer, index, value);
        }
    }

    public void MoveParticle(Particle particle, int2 from, int2 to)
    {
        this[from] = new Particle() { type = ParticleType.None };
        this[to] = particle;
    }

    public void SwapParticles(int2 from, int2 to)
    {
        Particle temp = this[from];
        this[from] = this[to];
        this[to] = temp;
    }

    public bool IsFreePosition(int2 pos)
    {
        return InBound(pos) && this[pos].type == ParticleType.None;
    }

    public ParticleType ParticleTypeAtPosition(int2 pos)
    {
        return this[pos].type;
    }


    public void SetSpriteAtPosition(int2 previousPosition, int2 position, ref PixelSprite sprite)
    {
        //Cleanup old position
        for (int x = 0; x < sprite.sizes.x; x++)
        {
            for (int y = 0; y < sprite.sizes.y; y++)
            {
                int2 newPos = new int2(previousPosition.x + x, previousPosition.y + y);
                if (sprite.collisions[x, y])
                    this[newPos] = new Particle() { type = ParticleType.None };
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
                    ParticleType previousType = this[nextPosition].type;
                    if (previousType != ParticleType.None && previousType != ParticleType.Player && TryFindEmptyPosition(nextPosition, new int2(0, -1), out int2 newPosition))
                    {
                        this[newPosition] = new Particle() { type = previousType };
                    }

                    this[nextPosition] = new Particle() { type = ParticleType.Player };
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
                if (this[position].type == ParticleType.None)
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
        return pos.x >= 0 && pos.y >= 0 && pos.x < sizes.x && pos.y < sizes.y;
    }
}
