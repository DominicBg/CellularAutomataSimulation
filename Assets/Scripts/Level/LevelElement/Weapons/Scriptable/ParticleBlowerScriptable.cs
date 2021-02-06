using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleBlowerScriptable", menuName = "Equipable/ParticleBlowerScriptable", order = 1)]
public class ParticleBlowerScriptable : EquipableBaseScriptable
{
    public int capacity = 25;
    public int attractionRadius = 15; 
    public int2 absorbBound = 3;

    public int2 absorbOffset;
    public int2 attractionOffset;
    public float suckVelocity;
    public float suckY;
    public float vortexVelocity;
    public float blowVelocity;
    public int maxParticlePerFrame = 3;
    public float fadeIn;
    public float fadeOut;
    public int cameraOffset;
    public EaseXVII.Ease fadeInCurve;
    public EaseXVII.Ease fadeOutCurve;
    public float shakeIntensity;
    public float shakeFrequency;

    public ParticleSuckingEffectSettings effects;
}
