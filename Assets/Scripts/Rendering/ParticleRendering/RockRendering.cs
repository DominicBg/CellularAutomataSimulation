using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct RockRendering: IParticleRenderer
{
    public Color lightColor;
    public Color mediumColor;
    public Color darkColor;
    public Color veryDarkColor;

    public Color borderColor;

    public float lightThreshold;
    public float mediumThreshold;
    public float darkThreshold;
    public float noiseScale;
  
    [Header("test")]
    public float3 lightPos;
    public bool showNormal;
    public bool showDot;
    public bool showHeight;
    public float minThresholdCrack;

    public float resolution;
    public float minclampHeight;
    public float maxclampHeight;
    public float ditherRange;

    public Color32 GetColor(int2 position, ref TickBlock tickBlock, ref Map map)
    {
        if (HaveNonRockInSurrounding(position, ref map))
            return borderColor;

        float2 positionScaled = (float2)position * noiseScale;
     
        float heightAtPosition = GetHeightAtPosition(positionScaled);

        if (showHeight)
            return new Color(heightAtPosition, heightAtPosition, heightAtPosition, 1);

        if (heightAtPosition < minThresholdCrack)
            return veryDarkColor;

        float3 normal = GetNormalAtPosition(heightAtPosition, positionScaled);
        normal = math.floor(normal * resolution) / resolution;

        if (showNormal)
            return new Color(normal.x, normal.y, normal.z, 1);

        float dot = math.dot(math.normalize(lightPos - new float3(position.x, position.y, 0)), normal);
        if (showDot)
            return new Color(dot, dot, dot, 1);

        return GetColorWithNoiseValue(dot, position);
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

    public Color32 GetColorWithNoiseValue(float noiseValue, int2 position)
    {
        bool checker = (position.x + position.y) % 2 == 0;
        if (noiseValue > lightThreshold)
        {
            return lightColor;
        }
        else if(noiseValue > lightThreshold - ditherRange)
        {
            return checker ? lightColor : mediumColor;
        }
        else if (noiseValue > mediumThreshold)
        {
            return mediumColor;
        }
        else if (noiseValue > mediumThreshold - ditherRange)
        {
            return checker ? mediumColor : darkColor;
        }
        else if (noiseValue > darkThreshold)
        {
            return darkColor;
        }
        else if (noiseValue > darkThreshold - ditherRange)
        {
            return checker ? darkColor : veryDarkColor;
        }
        else
        {
            return veryDarkColor;
        }
    }

    public Color32 GetColor(int2 position, ref TickBlock tickBlock)
    {
        throw new System.NotImplementedException();
    }
}