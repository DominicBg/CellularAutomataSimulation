using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectionalLightSourceScriptable", menuName = "Environment/DirectionalLightSourceScriptable", order = 1)]
public class DirectionalLightSourceScriptable : LightSourceScriptable
{
    public float3 direction;
    public float intensity;
    public Color color;
    public float resolution;

    public override LightSource GetLightSource(int2 position, int tick)
    {
        return LightSource.DirectionalLight(direction, intensity, color, resolution);
    }
}
