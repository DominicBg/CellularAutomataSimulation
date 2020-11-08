using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public ParticleType type;
    //velocity?
}

public enum ParticleType
{
    None,
    Water,
    Sand,
    Mud,
    Player
}