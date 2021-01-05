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
    Color color;


    public static LightSource Directional(float3 direction, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Directional;
        light.direction = direction;
        light.intensity = intensity;
        light.color = color;
        return light;
    }
    public static LightSource Point(float3 position, float radius, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Point;
        light.position = position;
        light.radius = radius;
        light.intensity = intensity;
        light.color = color;
        return light;
    }
    public static LightSource Spot(float3 position, float3 direction, float radius, float angle, float intensity, Color color)
    {
        LightSource light = new LightSource();
        light.type = Type.Spot;
        light.position = position;
        light.direction = direction;
        light.angle = angle;
        light.radius = radius;
        light.intensity = intensity;
        light.color = color;
        return light;
    }

    public float GetLightAtPosition(float2 position, out Color ligthColor)
    {
        float3 pos3D = new float3(position.x, position.y, 0);
        ligthColor = color;
        switch (type)
        {
            case Type.Directional:
                return DirectionalLight();
            case Type.Point:
                return PointLight(pos3D);
            case Type.Spot:
                return SpotLight(pos3D);
        }
        return 0;
    }
    public float GetLightAtPosition(float2 position, float3 normal, out Color ligthColor)
    {
        float3 pos3D = new float3(position.x, position.y, 0);
        ligthColor = color;
        switch (type)
        {
            case Type.Point:
                float3 dir = math.normalize(this.position - pos3D);
                return intensity * PointLight(pos3D) * math.saturate(math.dot(dir, normal));      
        }
        return 0;
    }

    float DirectionalLight()
    {
        //lol maybe do it for 2D and 3D? 
        return intensity;
    }

    float PointLight(float3 position)
    {
        float3 diff = this.position - position;
        diff.z = 0;
        float lengthSq = math.lengthsq(diff);
        if (lengthSq > radius * radius)
            return 0;

        float length = math.sqrt(lengthSq);
        return 1 - length / radius;
    }

    float SpotLight(float3 position)
    {
        float3 diff = this.position - position;
        float lengthSq = math.lengthsq(diff);
        if (lengthSq > radius)
            return 0;

        float length = math.sqrt(lengthSq);
        float3 dir = diff / length;
        float angleSpot = math.acos(math.dot(dir, direction));
        if (angleSpot < angle)
            return 0;

        return 1 - length / radius;
    }
}
