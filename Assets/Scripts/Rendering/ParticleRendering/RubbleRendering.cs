using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct RubbleRendering: IParticleRenderer
{
    public Color rockColor;
    public Color crackColor;

    public Color4Dither color4Dither;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        uint seed = (uint)(position.x + position.y * 100);
        Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex(seed);

        float3 normal = math.normalize(random.NextFloat3(-1, 1));
        float lightIntensity = lightSources.CalculateLight(position, normal);
        return color4Dither.GetColorWitLightValue(lightIntensity, position);
    }
}