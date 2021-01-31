using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleBlowerScriptable", menuName = "Equipable/ParticleBlowerScriptable", order = 1)]
public class ParticleBlowerScriptable : EquipableBaseScriptable
{
    public int capacity = 25;
    public int attractionRadius = 15; 
    public int absorbBound = 3;

    public int2 absorbOffset;
    public int2 attractionOffset;
    public float suckVelocity;
    public float blowVelocity;
}
