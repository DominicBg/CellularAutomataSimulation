using Unity.Mathematics;

[System.Serializable]
public struct ParticleBehaviour
{
    public GravityBehaviour gravity;
    public FloatyBehaviour floaty;
    public WaterBehaviour water;
    public TitleDisentegrateBehaviour titleDisentegrate;
    public WoodBehaviour woodBehaviour;
    public CinderBehaviour cinderBehaviour;

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
    public struct WoodBehaviour
    {
        public int tickBeforeTurnToCinder;
    }

    [System.Serializable]
    public struct CinderBehaviour
    {
        public int minimumSurroundingCinder;
        public int tickBeforeDisapear;
    }


    [System.Serializable]
    public struct TitleDisentegrateBehaviour
    {
        public float chanceMove;
        public float chanceDespawn;
    }




}
