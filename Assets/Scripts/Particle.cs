using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public ParticleType type;
    //velocity?
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
    Player,
    //Misc
    TitleDisintegration

}