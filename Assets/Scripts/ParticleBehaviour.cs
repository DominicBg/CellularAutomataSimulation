using Unity.Mathematics;

[System.Serializable]
public struct ParticleBehaviour
{
    public FloatyBehaviour floatyBehaviour;
    public WaterBehaviour waterBehaviour;
    public TitleDisentegrateBehaviour titleDisentegrateBehaviour;
    
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

    [System.Serializable]
    public struct TitleDisentegrateBehaviour
    {
        public float chanceMove;
        public float chanceDespawn;
    }
}
