using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;

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
        float z = isInBackground ? -1 : 0;

        Func<int2, bool> canDrawPixel = pixelPos => nativeSprite.pixels[pixelPos].a != 0;
        Func<int2, Color> getColor = pixelPos => nativeSprite.pixels[pixelPos];
        Func<int2, Color> getNormal = pixelPos => nativeNormalMap.pixels[pixelPos];
        Func<int2, Color> getLightColor = pixelPos => RenderingUtils.ApplyLightOnPixel(position, pixelPos, lightsCopy, getColor, getNormal, z, minLightIntensity, lightResolution);

        GridRenderer.ApplyCustomRender(ref outputColors, renderPos, nativeSprite.sizes, isFlipped, canDrawPixel, getLightColor, true);           
    }


    public override void Dispose()
    {
        base.Dispose();
        nativeNormalMap.Dispose();
    }
}
