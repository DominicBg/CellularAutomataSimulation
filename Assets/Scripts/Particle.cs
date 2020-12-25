using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Particle
{
    public ParticleType type;
    public float2 velocity;
    public int tickIdle;
    public float2 fracPosition;
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
    Cinder,
    Wood,
    Player,

    //Misc
    TitleDisintegration,
    Count


}