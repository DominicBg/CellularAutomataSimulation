using Unity.Mathematics;

[System.Serializable]
public struct ParticleBehaviour
{
    public GravityBehaviour gravity;
    public FloatyBehaviour floaty;
    public WaterBehaviour water;
    public TitleDisentegrateBehaviour titleDisentegrate;


    [System.Serializable]
    public struct GravityBehaviour
    {
        public float2 accelerationPerFrame;
    }

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
