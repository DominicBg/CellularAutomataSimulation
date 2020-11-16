using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SandRendering
{
    public float shimmerThreshold;
    public float waveThreshold;
    public float2 waveScale;
    public float2 waveSpeed;
    public float2 waveScrollSpeed;
    public Color sandColor;
    public Color shimmerColor;
    public Color waveColor;
}