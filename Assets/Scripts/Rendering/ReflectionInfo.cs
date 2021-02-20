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

[System.Serializable]
public struct EnvironementReflectionInfo
{
    public static EnvironementReflectionInfo Default()
    {
        return new EnvironementReflectionInfo() {
            amount = .2f,
            blending = BlendingMode.AdditiveAlpha,
            distance = 20,
            blurRadius = 2,
            blurIntensity = .75f,
            mirrorReflectionThreshold = 0.9f
        };
    }

    public float amount;
    public float distance;
    public BlendingMode blending;
    public int blurRadius;
    public float blurIntensity;

    //Under this, it will be mirrored
    public float mirrorReflectionThreshold;
}

[System.Serializable]
public struct ShadingLitInfo
{
    public static ShadingLitInfo Default()
    {
        return new ShadingLitInfo()
        {
            minLightIntensity = 0.5f,
            lightResolution = 25,
            z = 0,
        };
    }

    public float minLightIntensity;
    public float lightResolution;
    public float z;
}

