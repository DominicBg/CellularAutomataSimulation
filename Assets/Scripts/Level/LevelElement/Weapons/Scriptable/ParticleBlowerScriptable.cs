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
    public float fadeIn;
    public float fadeOut;
    public int cameraOffset;
    public EaseXVII.Ease fadeInCurve;
    public EaseXVII.Ease fadeOutCurve;

    public ParticleSuckingEffectSettings effects;
}
