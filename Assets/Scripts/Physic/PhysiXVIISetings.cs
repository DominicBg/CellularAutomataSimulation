using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public unsafe struct PhysiXVIISetings
{
    public float2 gravity;
    public float friction;
    public int maxSlope;

    [EnumeratedArray(typeof(ParticleType))]
    public fixed bool canPush[(int)ParticleType.Count];

    [EnumeratedArray(typeof(ParticleType))]
    public fixed float frictions[(int)ParticleType.Count];

    [EnumeratedArray(typeof(ParticleType))]
    public fixed float mass[(int)ParticleType.Count];

    [EnumeratedArray(typeof(ParticleType))]
    public fixed float absorbtion[(int)ParticleType.Count];
}
