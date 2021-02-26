using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public struct ParticleRendering
{
    public EmptyRendering emptyRendering;
    public WaterRendering waterRendering;
    public SandRendering sandRendering;
    public IceRendering iceRendering;
    public RockRendering rockRendering;
    public RubbleRendering rubbleColor;
    public FireRendering fireRendering;
    public WoodRendering woodRendering;
    public StringRendering stringRendering;
    public CinderRendering cinderRendering;
    public Color mudColor;
    public Color snowColor;
    public Color titleDisintegration;
}

public interface IParticleRenderer
{
    //Soon add map + light sources
    Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources);
}