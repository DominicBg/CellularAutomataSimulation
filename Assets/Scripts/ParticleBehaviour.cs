using Unity.Mathematics;

//todo add scriptable

[System.Serializable]
public struct ParticleBehaviour
{
    public FloatyBehaviour floatyBehaviour;
    public WaterBehaviour waterBehaviour;
    
    [System.Serializable]
    public struct FloatyBehaviour
    {
        public float sinSpeed;
        public float2 sinOffset;
        public float ratioFloat;
    }

    [System.Serializable]
    public struct WaterBehaviour
    {
        public int diffWaterSandToDry;
    }

}
