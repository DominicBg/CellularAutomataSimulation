using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControlSettings", menuName = "PlayerControlSettings", order = 1)]
public class PlayerControlSettings : ScriptableObject
{
    [Header("Walk friction effect")]
    public float2 walkingForce = new float2(1, 0.5f);

    [Header("Reflections/Shading")]
    public ShadingLitInfo shadingInfo = ShadingLitInfo.Default();
    public ReflectionInfo skyReflection = ReflectionInfo.Default();
    public EnvironementReflectionInfo environementReflection = EnvironementReflectionInfo.Default();

    public GlowingLightSourceScriptable lightSourceSettings;
    public Backpack backpack;

    [System.Serializable]
    public struct Backpack
    {
        public int2 offset;
        public Texture2D sprite;
        public Texture2D normal;
        public ShadingLitInfo shadingInfo;
    }
}
