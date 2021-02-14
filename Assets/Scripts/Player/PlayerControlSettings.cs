using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControlSettings", menuName = "PlayerControlSettings", order = 1)]
public class PlayerControlSettings : ScriptableObject
{
    [Header("Movement")]
    public float acceleration = 175;
    [Range(0, 1)] public float stopMovingDamping = .9f;
    [Range(0, 1)] public float turiningDamping = .3f;
    [Range(0, 1)] public float movingDamping = .3f;
    public float dampingForce = 10;


    [Header("Jump")]
    public float pressJumpBuffer = 0.2f;
    public float jumpForce = 150;
    public float inAirJumpThreshold = 0.2f;
    public float releaseJumpButtonCutoff = 0.5f;

    [Header("Other")]
    public SpriteSheetScriptable spriteSheet;
    public Texture2D collisionTexture;

    public GlowingLightSourceScriptable lightSourceSettings;
}
