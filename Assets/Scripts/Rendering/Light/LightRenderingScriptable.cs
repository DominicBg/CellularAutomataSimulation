using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LightRenderingScriptable", menuName = "Light/LightRenderingScriptable", order = 1)]
public class LightRenderingScriptable : ScriptableObject
{
    public LightRenderingSettings settings;
}

[System.Serializable]
public struct LightRenderingSettings
{
    [Range(0, 1)] public float additiveRatio;
    public BlendingMode lightBlending;
}