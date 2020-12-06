using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Particle
{
    public ParticleType type;
    public float2 velocity;
    //turn idle?
}

public enum ParticleType
{
    None,
    Water,
    Sand,
    Mud,
    Snow,
    Ice,
    Rock,
    Rubble,
    Fire,
    Player,

    //Misc
    TitleDisintegration,
    Count


}