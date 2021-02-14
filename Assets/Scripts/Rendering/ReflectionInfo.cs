using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ReflectionInfo
{
    public static ReflectionInfo Default()
    {
        return new ReflectionInfo() { amount = .2f, blending = BlendingMode.AdditiveAlpha, distance = 20 };
    }

    public float amount;
    public float distance;
    public BlendingMode blending;
}
