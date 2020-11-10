using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public unsafe struct NativeGrid<T> : IDisposable where T : struct
{
    [NativeDisableUnsafePtrRestriction]
    public void* particlesBuffer;
    public int2 sizes;

    Allocator m_Allocator;
    internal AtomicSafetyHandle m_Safety;

    [NativeSetClassTypeToNullOnSchedule]
    internal DisposeSentinel m_DisposeSentinel;

    private static int s_staticSafetyId;

    public NativeGrid(int2 sizes, Allocator allocator)
    {
        int size = (sizes.x * sizes.y) * UnsafeUtility.SizeOf<T>();
        this.sizes = sizes;
        m_Allocator = allocator;

        particlesBuffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
        UnsafeUtility.MemClear(particlesBuffer, size);

        //Copy pasted stuff from NativeArray
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
        if (s_staticSafetyId == 0)
        {
            s_staticSafetyId = AtomicSafetyHandle.NewStaticSafetyId<NativeArray<T>>();
        }
        AtomicSafetyHandle.SetStaticSafetyId(ref m_Safety, s_staticSafetyId);
    }

    public void Dispose()
    {
        if (!UnsafeUtility.IsValidAllocator(m_Allocator))
        {
            throw new InvalidOperationException("The NativeArray can not be Disposed because it was not allocated with a valid allocator.");
        }
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
        UnsafeUtility.Free(particlesBuffer, m_Allocator);
        particlesBuffer = null;
    }

    public unsafe Particle this[int x, int y]
    {
        get
        {
            return this[new int2(x, y)];     
        }
        set
        {
            this[new int2(x, y)] = value;
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

    public bool InBound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < sizes.x && pos.y < sizes.y;
    }
}
