using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "GlowingLightSourceScriptable", menuName = "Environment/GlowingLightSourceScriptable", order = 1)]
public class GlowingLightSourceScriptable : LightSourceScriptable
{
    public Glowing glowingIntensity;
    public Glowing glowingRadius;
    public Color color;
    public float z;

    public override LightSource GetLightSource(int2 position, int tick)
    {
        return LightSource.Point(new float3(position.x, position.y, z), glowingRadius.EvaluateGlow(tick), glowingIntensity.EvaluateGlow(tick), color);
    }
}
