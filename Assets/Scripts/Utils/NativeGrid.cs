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
    public void* m_buffer;
    public int2 m_sizes;

    Allocator m_Allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    internal AtomicSafetyHandle m_Safety;

    [NativeSetClassTypeToNullOnSchedule]
    internal DisposeSentinel m_DisposeSentinel;

    //private static int s_staticSafetyId;
#endif

    private int m_binarySize;

    public NativeGrid(int2 sizes, Allocator allocator)
    {
        m_binarySize = (sizes.x * sizes.y) * UnsafeUtility.SizeOf<T>();
        this.m_sizes = sizes;
        m_Allocator = allocator;

        m_buffer = UnsafeUtility.Malloc(m_binarySize, UnsafeUtility.AlignOf<T>(), allocator);
        UnsafeUtility.MemClear(m_buffer, m_binarySize);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        //Copy pasted stuff from NativeArray
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
        //if (s_staticSafetyId == 0)
        //{
        //    s_staticSafetyId = AtomicSafetyHandle.NewStaticSafetyId<NativeGrid<T>>();
        //}
        //AtomicSafetyHandle.SetStaticSafetyId(ref m_Safety, s_staticSafetyId);
#endif
    }

    public void Clear()
    {
        UnsafeUtility.MemClear(m_buffer, m_binarySize);
    }

    public void Dispose()
    {
        if (m_buffer == null)
            return;

        m_buffer = null;
        UnsafeUtility.Free(m_buffer, m_Allocator);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        //if (!UnsafeUtility.IsValidAllocator(m_Allocator))
        //{
        //    throw new InvalidOperationException("The NativeArray can not be Disposed because it was not allocated with a valid allocator.");
        //}
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

    }

    public unsafe T this[int x, int y]
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

    public unsafe T this[int2 index2]
    {
        get
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            if (!InBound(index2))
                throw new ArgumentOutOfRangeException($"Don't you ever try to read out of bound again, this is unsafe :@ {index2}, max {m_sizes}");
#endif

            int index = ArrayHelper.PosToIndex(index2, m_sizes);
            return UnsafeUtility.ReadArrayElement<T>(m_buffer, index);
        }
        set
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            if (!InBound(index2))
                throw new ArgumentOutOfRangeException($"Don't you ever try to write out of bound again, this is unsafe :@ at {index2}, max {m_sizes}");
#endif

            int index = ArrayHelper.PosToIndex(index2, m_sizes);
            UnsafeUtility.WriteArrayElement(m_buffer, index, value);
        }
    }

    public bool InBound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < m_sizes.x && pos.y < m_sizes.y;
    }
}
