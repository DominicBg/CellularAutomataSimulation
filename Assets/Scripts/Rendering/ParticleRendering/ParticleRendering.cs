using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public struct ParticleRendering
{
    public Color noneColor;
    public WaterRendering waterRendering;
    public SandRendering sandRendering;
    public IceRendering iceRendering;
    public RockRendering rockRendering;
    public RubbleRendering rubbleColor;
    public FireRendering fireRendering;
    public Color mudColor;
    public Color snowColor;
    public Color titleDisintegration;
}

public interface IParticleRenderer
{
    Color32 GetColor(int2 position, ref TickBlock tickBlock);
}