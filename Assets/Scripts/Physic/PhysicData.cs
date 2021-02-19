using Unity.Mathematics;

[System.Serializable]
public struct PhysicData
{
    public float2 position;
    public float2 displacement;
    public float2 velocity;
    public PhysicBound physicBound;
    public int2 gridPosition;
    public float mass;
    public int inclinaison;
    public bool isGrounded;

    //debug data
    public int2 debugSafePosition;
    public int2 debugCollisionNormal;
    public bool2 debugAxisBlocked;
}