﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControlSettings", menuName = "PlayerControlSettings", order = 1)]
public class PlayerControlSettings : ScriptableObject
{
    public float movementSpeed = 150;
    public float airAcceleration = 5;
    public float airMovementSpeed = 75;

    //move somewhere else
    public float jetpackForce = 150;

}
