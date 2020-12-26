using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SmokeParticleSystemScriptable", menuName = "XVII/ParticleSystem/SmokeParticleSystemScriptable", order = 1)]
public class SmokeParticleSystemScriptable : ScriptableObject
{
    public SmokeParticleSystemEmitter emitter;
    public SmokeParticleSystemSettings settings;
}
