using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleGunScriptable", menuName = "Equipable/ParticleGunScriptable", order = 1)]
public class ParticleGunScriptable : BaseGunScriptable
{
    public float particleSpeed;
    public int2 particleOffset;
}
