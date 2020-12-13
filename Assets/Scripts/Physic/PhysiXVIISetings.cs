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

    [EnumNamedArray(typeof(ParticleType))]
    public fixed float canPush[(int)ParticleType.Count];

    //TODO
    //[EnumNamedArray(typeof(ParticleType))]
    //public fixed float frictions[(int)ParticleType.Count];

    //[EnumNamedArray(typeof(ParticleType))]
    //public fixed float weigth[(int)ParticleType.Count];

    //[EnumNamedArray(typeof(ParticleType))]
    //public fixed float absorbtion[(int)ParticleType.Count];

}
