using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct RockRendering: IParticleRenderer
{
    public Color4Dither color4Dither;
    public Color borderColor;

    public float resolution;
    public float minclampHeight;
    public float maxclampHeight;
    public float noiseScale;

    [Header("debug")]
    public bool showNormal;
    public bool showIntensity;
    public bool showHeight;
    public bool showLightColor;
    public float minThresholdCrack;
    public bool highestIntensityOnly;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map, NativeArray<LightSource> lightSources)
    {
        if (HaveNonRockInSurrounding(position, ref map))
            return borderColor;
        return GetColor(position, lightSources, 0);
    }

    public Color32 GetColor(int2 position, NativeArray<LightSource> lightSources, int2 lightoffset)
    {
        float2 positionScaled = (float2)position * noiseScale;
     
        float heightAtPosition = GetHeightAtPosition(positionScaled);

        if (showHeight)
            return new Color(heightAtPosition, heightAtPosition, heightAtPosition, 1);

        if (heightAtPosition < minThresholdCrack)
            return color4Dither.veryDarkColor;

        float3 normal = GetNormalAtPosition(heightAtPosition, positionScaled);
        normal = math.floor(normal * resolution) / resolution;

        if (showNormal)
            return new Color(normal.x, normal.y, normal.z, 1);

        float intensity = 0;
        for (int i = 0; i < lightSources.Length; i++)
        {
            float currentIntensity = lightSources[i].GetLightIntensity(position + lightoffset, normal);

            if (highestIntensityOnly)
                intensity = math.max(intensity, currentIntensity);
            else
                intensity += currentIntensity;
        }

        intensity = math.saturate(intensity);

        if (showIntensity)
            return new Color(intensity, intensity, intensity, 1);

        return color4Dither.GetColorWitLightValue(intensity, position);
    }

    float GetHeightAtPosition(float2 position)
    {
        float heightAtPosition = 1 - (noise.cellular(position)).x;
        float clampHeight = math.remap(-1, 1, minclampHeight, maxclampHeight, noise.cnoise(position * 0.1f));

        //fast remap 0, 1
        return math.min(heightAtPosition, clampHeight) * (1f / clampHeight);
    }

    float3 GetNormalAtPosition(float baseHeight, float2 position)
    {
        const float epsilon = 0.0001f;

        float2 positionScaledDx = position + new float2(epsilon, 0);
        float2 positionScaledDy = position + new float2(0, epsilon);

        float dx = (baseHeight - GetHeightAtPosition(positionScaledDx)) / epsilon;
        float dy = (baseHeight - GetHeightAtPosition(positionScaledDy)) / epsilon;

        return math.normalize(new float3(dx, dy, 1));
    }

    public bool HaveNonRockInSurrounding(int2 position, ref Map map)
    {
        int2 p1 = position + new int2(1, 0);
        int2 p2 = position + new int2(-1, 0);
        int2 p3 = position + new int2(0, 1);
        int2 p4 = position + new int2(0, -1);
        if (map.InBound(p1) && map.GetParticleType(p1) != ParticleType.Rock) return true;
        if (map.InBound(p2) && map.GetParticleType(p2) != ParticleType.Rock) return true;
        if (map.InBound(p3) && map.GetParticleType(p3) != ParticleType.Rock) return true;
        if (map.InBound(p4) && map.GetParticleType(p4) != ParticleType.Rock) return true;

        return false;
    }

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        return Color.black;
    }
}