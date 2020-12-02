using Unity.Mathematics;

[System.Serializable]
public struct PhysicData
{
    public float2 position;
    public float2 controlledVelocity;
    public float2 velocity;
    public PhysicBound physicBound;
    public int2 gridPosition;
}