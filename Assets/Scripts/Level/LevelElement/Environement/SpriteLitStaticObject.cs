using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//Is center aligned
public class SpriteLitStaticObject : SpriteStaticObject
{
    public Texture2D normalMap;

    NativeSprite nativeNormalMap;
    public float lightResolution = 25;
    public float minLightIntensity = 0.5f;
    public override void OnInit()
    {
        base.OnInit();
        nativeNormalMap = new NativeSprite(normalMap);
        if (!math.all(nativeSprite.sizes == nativeNormalMap.sizes))
            Debug.LogError(this.name + " is having a normal map of a different size");
    }

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {

        //override so SpriteStaticObject doesnt render
    }
    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        //override so SpriteStaticObject doesnt render
    }


    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        if (isInBackground)
            RenderPixels(ref outputColors, ref tickBlock, renderPos, ref lights);
    }
    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        if (!isInBackground)
            RenderPixels(ref outputColors, ref tickBlock, renderPos, ref lights);
    }

    void RenderPixels(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights)
    {
        NativeList<LightSource> lightsCopy = lights;
        GridRenderer.ApplyCustomRender(
            ref outputColors, renderPos, nativeSprite.sizes, isFlipped,
            (pixelPos => nativeSprite.pixels[pixelPos.x, pixelPos.y].a != 0),
            (pixelPos => ApplyLightOnPixel(position, pixelPos, lightsCopy)),
            true);           
    }

    Color ApplyLightOnPixel(int2 position, int2 pixelPos, NativeList<LightSource> lights)
    {
        Color color = nativeSprite.pixels[pixelPos.x, pixelPos.y];
        float3 normal = ((Color)nativeNormalMap.pixels[pixelPos.x, pixelPos.y]).ToNormal();

        float z = isInBackground ? -1 : 0;
        float3 pos3D = new float3(position.x, position.y, z);
        float lightIntensity = lights.CalculateLight(pos3D, normal);
        lightIntensity = MathUtils.ReduceResolution(lightIntensity, lightResolution);
        lightIntensity = math.remap(0, 1, minLightIntensity, 1, lightIntensity);

        return color * lightIntensity;
    }

    public override void Dispose()
    {
        base.Dispose();
        nativeNormalMap.Dispose();
    }
}
