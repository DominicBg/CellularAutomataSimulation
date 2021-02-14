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
    public Texture2D reflectionMap;

    public float lightResolution = 25;
    public float minLightIntensity = 0.5f;
    public override void OnInit()
    {
        nativeSprite = new NativeSprite(texture, normalMap, reflectionMap);
        TrySetCollision();
    }

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (isInBackground)
            RenderPixels(ref outputColors, ref tickBlock, renderPos, ref info.lightSources, ref info);
    }
    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (!isInBackground)
            RenderPixels(ref outputColors, ref tickBlock, renderPos, ref info.lightSources, ref info);
    }

    void RenderPixels(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref NativeList<LightSource> lights, ref EnvironementInfo info)
    {
        var sprite = nativeSprite.pixels;
        var normals = nativeSprite.normals;
        var reflections = nativeSprite.reflections;

        if (nativeSprite.UseNormals)
        {
            GridRenderer.ApplyLitSprite(ref outputColors, sprite, normals, position, renderPos, lights, minLightIntensity, true);
        }
        else
        {
            GridRenderer.ApplySprite(ref outputColors, sprite, renderPos);
        }

        if (nativeSprite.UseNormals && nativeSprite.UseReflection)
        {
            GridRenderer.ApplySpriteSkyboxReflection(ref outputColors, sprite, normals, reflections, renderPos, info, ReflectionInfo.Default(), true);
            GridRenderer.ApplySpriteEnvironementReflection(ref outputColors, sprite, normals, reflections, renderPos, ReflectionInfo.Default(), 2, .5f, true);
        }


        //NativeList<LightSource> lightsCopy = lights;
        //float z = isInBackground ? -1 : 0;

        //Func<int2, int2, bool> canDrawPixel = (pixelPos, _) => nativeSprite.pixels[pixelPos].a != 0;
        //Func<int2, int2, Color> getColor = (pixelPos, _)=> nativeSprite.pixels[pixelPos];
        //Func<int2, int2, Color> getNormal = (pixelPos, _) => nativeNormalMap.pixels[pixelPos];
        //Func<int2, int2, Color> getLightColor = (pixelPos, finalPos) => RenderingUtils.ApplyLightOnPixel(finalPos, pixelPos, lightsCopy, getColor, getNormal, z, minLightIntensity, lightResolution);

        //GridRenderer.ApplyCustomRender(ref outputColors, renderPos, nativeSprite.sizes, isFlipped, canDrawPixel, getLightColor, true);           
    }


    public override void Dispose()
    {
        base.Dispose();
    }
}
