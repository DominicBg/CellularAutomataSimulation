using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class NativeArrayEx
{
    public static void TryDispose<T>(this NativeArray<T> nativeArray) where T : struct
    {
        if (nativeArray.IsCreated)
            nativeArray.Dispose();
    }
    public static void TryDispose<T>(this NativeList<T> nativeList) where T : struct
    {
        if (nativeList.IsCreated)
            nativeList.Dispose();
    }
}
