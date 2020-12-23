using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FogElement;

[CreateAssetMenu(fileName = "PlayerControlSettings", menuName = "PlayerControlSettings", order = 1)]
public class PlayerControlSettings : ScriptableObject
{
    public float movementSpeed = 150;
    public float airAcceleration = 5;
    public float airMovementSpeed = 75;

    //move somewhere else
    public float jumpForce = 150;

    public SpriteSheet spriteSheet;
    public Texture2D collisionTexture;

    public LightSource lightSource;
}
