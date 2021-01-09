using Unity.Mathematics;
using UnityEngine;

public struct LightSource
{
    public enum Type { Directional, Point, Spot };

    Type type;
    float3 position;
    float3 direction;
    float radius;
    float angle;
    float intensity;
    float fadeoff;
    Color color;
    float resolution;

    public static LightSource DirectionalLight(float3 direction, float intensity, Color color, float resolution = 25)
    {
        LightSource light = new LightSource();
        light.type = Type.Directional;
        light.direction = math.normalize(direction);
        light.intensity = intensity;
        light.color = color;
        light.resolution = resolution;
        return light;
    }
    public static LightSource PointLight(float3 position, float radius, float fadeoff, float intensity, Color color, float resolution = 25)
    {
        LightSource light = new LightSource();
        light.type = Type.Point;
        light.position = position;
        light.radius = radius;
        light.fadeoff = fadeoff;
        light.intensity = intensity;
        light.color = color;
        light.resolution = resolution;
        return light;
    }
    public static LightSource SpotLight(float3 position, float3 direction, float radius, float angle, float intensity, Color color, float resolution = 25)
    {
        LightSource light = new LightSource();
        light.type = Type.Spot;
        light.position = position;
        light.direction = math.normalize(direction);
        light.angle = angle;
        light.radius = radius;
        light.intensity = intensity;
        light.color = color;
        light.resolution = resolution;
        return light;
    }

    public float GetLightIntensity(float2 position, float3 normal)
    {
        float3 pos3D = new float3(position.x, position.y, 0);
        float intensity = GetDistanceIntensity(pos3D) * GetLightSurfaceIntensity(pos3D, normal) * this.intensity;
        return intensity;
    }
    public float GetDistanceIntensity(float3 position)
    {
        switch (type)
        {
            case Type.Point:
                return PointLightDistanceIntensity(position);
            case Type.Directional:
                return DirectionalLightDistanceIntensity();
            case Type.Spot:
                return SpotLightDistanceIntensity(position);
        }
        return 0;
    }
    float GetLightSurfaceIntensity(float3 position, float3 normal)
    {
        float3 diff = position - this.position;
        float3 dir = math.normalize(diff);
        switch (type)
        {
            case Type.Point:
                return LightSurfaceIntensity(dir, normal);
            case Type.Directional:
                return LightSurfaceIntensity(direction, normal);
            case Type.Spot:
                return LightSurfaceIntensity(dir, normal);
        }
        return 0;
    }



    float LightSurfaceIntensity(float3 lightDirection, float3 normal)
    {
        return math.saturate(math.dot(lightDirection, normal));
    }


    float DirectionalLightDistanceIntensity()
    {
        return 1;
    }

    float PointLightDistanceIntensity(float3 position/*, float3 normal*/)
    {
        float3 diff = this.position - position;

        diff.z = 0;
        float lengthSq = math.lengthsq(diff);
        if (lengthSq > radius * radius)
            return 0;

        float length = math.sqrt(lengthSq);
        //float3 dir = diff / length;
        //float nDotDir = math.saturate(math.dot(dir, normal));
        float radiusIntensity = MathUtils.RemapSaturate(radius - fadeoff, radius, 1, 0, length);

        return radiusIntensity;// * nDotDir;
    }

    float SpotLightDistanceIntensity(float3 position)
    {
        //add angle falloff?

        float3 diff = this.position - position;
        float lengthSq = math.lengthsq(diff);
        if (lengthSq > radius)
            return 0;

        float length = math.sqrt(lengthSq);
        float3 dir = diff / length;
        float angleSpot = math.acos(math.dot(dir, direction));
        if (angleSpot < angle)
            return 0;

        return (1 - length / radius);
    }

    Color Blend(Color pixelColor, float intensity)
    {
        Color color = this.color;
        color.a *= intensity;
        return RenderingUtils.Blend(pixelColor, color, BlendingMode.Normal);
    }

    public Color Blend(int2 pixelPosition, Color pixelColor, BlendingMode blendingMode)
    {
        Color color = this.color;
        color.a *= intensity * GetDistanceIntensity(new float3(pixelPosition.x, pixelPosition.y, 0));
        color.a = math.saturate(color.a);
        color.a = MathUtils.ReduceResolution(color.a, resolution);
        return RenderingUtils.Blend(pixelColor, color, blendingMode);
    }
}
