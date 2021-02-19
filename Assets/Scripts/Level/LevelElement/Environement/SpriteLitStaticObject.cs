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
    
    int reflectionIndex;

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
        if (nativeSprite.UseNormals)
        {
            ShadingLitInfo defaultLit = ShadingLitInfo.Default();
            GridRenderer.ApplyLitSprite(ref outputColors, sprite, normals, position, renderPos, lights, in defaultLit, true);
        }
        else
        {
            GridRenderer.ApplySprite(ref outputColors, sprite, renderPos);
        }

        if (nativeSprite.UseNormals && nativeSprite.UseReflection)
        {
            reflectionIndex = info.GetReflectionIndex();
            GridRenderer.PrepareSpriteEnvironementReflection(sprite, renderPos, ref info, reflectionIndex, true);
        }
    }


    public override void RenderReflection(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var sprite = nativeSprite.pixels;
        var normals = nativeSprite.normals;
        var reflections = nativeSprite.reflections;
        if (nativeSprite.UseNormals && nativeSprite.UseReflection)
        {
            var defaultReflection = ReflectionInfo.Default();
            var defaultEnvReflection = EnvironementReflectionInfo.Default();
            GridRenderer.ApplySpriteSkyboxReflection(ref outputColors, sprite, normals, reflections, renderPos, ref info, in defaultReflection, true);
            GridRenderer.ApplySpriteEnvironementReflection(ref outputColors, sprite, normals, reflections, renderPos, reflectionIndex, ref info, in defaultEnvReflection, true);
        }
    }


    public override void Dispose()
    {
        base.Dispose();
    }
}
