using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleEffectSystemScriptable", menuName = "Effects/ParticleSystem/ParticleEffectSystemScriptable", order = 1)]
public class ParticleEffectSystemScriptable : ScriptableObject
{
    public ParticleEffectSystemSettings settings;
}
