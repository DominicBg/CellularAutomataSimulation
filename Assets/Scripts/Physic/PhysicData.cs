using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PhysicData
{
    [Header("Settings")]
    public PhysicBound physicBound;
    public float mass;
    public bool applyFriction;

    [Header("Runtime")]
    public float2 position;
    public float2 displacement;
    public float2 velocity;
    public int2 gridPosition;

    public int inclinaison;
    public bool isGrounded;

    public bool hasCollision;
    public float2 collisionNormalNormalized;
    public int2 collisionNormal;

    [Header("Debug")]

    //debug data
    public int2 debugSafePosition;
    public int2 debugCollisionNormal;
    public bool2 debugAxisBlocked;
}