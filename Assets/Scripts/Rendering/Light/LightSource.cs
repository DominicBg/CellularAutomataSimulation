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


    public static LightSource DirectionalLight(float3 direction, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Directional;
        light.direction = math.normalize(direction);
        light.intensity = intensity;
        light.color = color;
        return light;
    }
    public static LightSource PointLight(float3 position, float radius, float fadeoff, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Point;
        light.position = position;
        light.radius = radius;
        light.fadeoff = fadeoff;
        light.intensity = intensity;
        light.color = color;
        return light;
    }
    public static LightSource SpotLight(float3 position, float3 direction, float radius, float angle, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Spot;
        light.position = position;
        light.direction = math.normalize(direction);
        light.angle = angle;
        light.radius = radius;
        light.intensity = intensity;
        light.color = color;
        return light;
    }

 
    public float GetLightAtPosition(float2 position, float3 normal, Color pixelColor, out Color ligthColor)
    {
        float3 pos3D = new float3(position.x, position.y, 0);
        ligthColor = color;
        switch (type)
        {
            case Type.Point:
                return PointLightIntensity(pos3D, normal, pixelColor, out ligthColor);
            case Type.Directional:
                return DirectionalLightIntensity(normal, pixelColor, out ligthColor);
            case Type.Spot:
                return SpotLightIntensity(pos3D, normal, pixelColor, out ligthColor);
        }
        return 0;
    }

    float DirectionalLightIntensity(float3 normal, Color pixelColor, out Color ligthColor)
    {
        float nDotDir = math.saturate(math.dot(direction, normal));
        ligthColor = Blend(pixelColor, this.intensity);
        return nDotDir * intensity;
    }

    float PointLightIntensity(float3 position, float3 normal, Color pixelColor, out Color ligthColor)
    {
        ligthColor = pixelColor;

        float3 diff = this.position - position;
        
        diff.z = 0;
        float lengthSq = math.lengthsq(diff);
        if (lengthSq > radius * radius)
            return 0;

        float length = math.sqrt(lengthSq);
        float3 dir = diff / length;
        float nDotDir = math.saturate(math.dot(dir, normal));
        float radiusIntensity = MathUtils.RemapSaturate(radius - fadeoff, radius, 1, 0, length);

        ligthColor = Blend(pixelColor, radiusIntensity);
        return radiusIntensity * intensity * nDotDir;
    }

    float SpotLightIntensity(float3 position, float3 normal, Color pixelColor, out Color ligthColor)
    {
        ligthColor = pixelColor;

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

        float nDotDir = math.saturate(math.dot(dir, normal));
        return (1 - length / radius) * intensity * nDotDir;
    }

    Color Blend(Color pixelColor, float intensity)
    {
        Color color = this.color;
        color.a *= intensity;
        return RenderingUtils.Blend(pixelColor, color, BlendingMode.Normal);
    }
}
