using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct WaterRendering
{
    [Header("Bubble Sin")]
    public float bubbleSineAmplitude;
    public float bubbleSineSpeed;
    public float bubbleSineNoiseAmplitude;
    public float2 bubbleSineOffSynch;
    public float2 bubbleSineNoiseSpeed;

    [Header("Bubble Color")]
    public Color bubbleOuterColor;
    public Color bubbleInnerColor;
    public float bubbleInnerThreshold;
    public float bubbleOuterThreshold;

    [Header("Other")]
    public float scaling;
    public Color waterColor;
    public float2 speed;
}