using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class SpriteAnimator : IDisposable
{
    public NativeSpriteSheet nativeSpriteSheet;

    private NativeGrid<Color32> currentSprite;
    private NativeGrid<float3> currentNormal;
    private NativeGrid<float> currentReflection;
    private int2 currentAnimOffset;

    public int currentAnim;
    public int currentFrame;

    public int framePerImage = 10;

    int currentTick;

    public bool returnToIdleAfterAnim = false;

    private bool useNormal;
    private bool useReflection;

    public void Update(bool isFlipped = false)
    {
        Update(new bool2(isFlipped, false));
    }

    public void Update(bool2 isFlipped)
    {
        currentTick++;
        if(currentTick == framePerImage)
        {
            currentTick = 0;
            currentFrame++;

            //Loop back animation
            int animFrameCount = nativeSpriteSheet.spritePerAnim[currentAnim];
            if (currentFrame == animFrameCount)
            {
                currentFrame = 0;
                if(returnToIdleAfterAnim)
                {
                    currentAnim = 0;
                }
            }
        }
        StoreCurrentSprite(isFlipped);
    }

    public void SetAnimation(int animation)
    {
        if (currentAnim == animation)
            return;

        currentTick = 0;
        currentFrame = 0;
        currentAnim = animation;
    }

    public SpriteAnimator(SpriteSheetScriptable spriteSheet)
    {
        nativeSpriteSheet = new NativeSpriteSheet(spriteSheet);
        currentSprite = new NativeGrid<Color32>(nativeSpriteSheet.spriteSizes, Allocator.Persistent);
        currentNormal = new NativeGrid<float3>(nativeSpriteSheet.spriteSizes, Allocator.Persistent);
        currentReflection = new NativeGrid<float>(nativeSpriteSheet.spriteSizes, Allocator.Persistent);
        useNormal = spriteSheet.spriteAnimations[0].normals != null && spriteSheet.spriteAnimations[0].normals.Length > 0;
        useReflection = spriteSheet.spriteAnimations[0].reflections != null && spriteSheet.spriteAnimations[0].reflections.Length > 0;

        StoreCurrentSprite(false);
    }

    private void StoreCurrentSprite(bool2 isFlipped)
    {
        int2 sizes = nativeSpriteSheet.spriteSizes;
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int xx = (!isFlipped.x) ? x : sizes.x - x - 1;
                int yy = (!isFlipped.y) ? y : sizes.y - y - 1;

                int2 pixelPos = new int2(xx, yy) + new int2(currentFrame, currentAnim) * nativeSpriteSheet.spriteSizes;

                currentSprite[x, y] = nativeSpriteSheet.pixels[pixelPos.x, pixelPos.y];
                if (useNormal)
                {
                    float3 normal = nativeSpriteSheet.normals[pixelPos.x, pixelPos.y];
                    normal.xy = math.select(normal.xy, -normal.xy, isFlipped);
                    currentNormal[x, y] = normal;
                }
                if (useReflection)
                    currentReflection[x, y] = nativeSpriteSheet.reflections[pixelPos.x, pixelPos.y];          
            }
        }
        currentAnimOffset = nativeSpriteSheet.offsets[currentFrame, currentAnim];
    }

    public NativeGrid<Color32> GetCurrentSprite()
    {
        return currentSprite;
    }
    public NativeGrid<float3> GetCurrentNormals()
    {
        return currentNormal;
    }
    public NativeGrid<float> GetCurrentReflections()
    {
        return currentReflection;
    }
    public int2 GetCurrentAnimOffset()
    {
        return currentAnimOffset;
    }

    public int2 GetSizes()
    {
        return nativeSpriteSheet.spriteSizes;
    }

    public void Render(ref NativeArray<Color32> outputColors, int2 position)
    {
        int2 sizes = nativeSpriteSheet.spriteSizes;
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int2 worldPos = new int2(x, y) + position;
                if (GridHelper.InBound(worldPos, GameManager.RenderSizes) && currentSprite[x, y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(worldPos, GameManager.RenderSizes.x);
                    outputColors[index] = currentSprite[x, y];
                }
            }
        }
    }

    //public void RenderWithLight(ref NativeArray<Color32> outputColors, int2 worldPos, int2 renderPos, bool2 isFlipped, NativeList<LightSource> lightSources, NativeSprite normalMap)
    //{
    //    int2 sheetOffset = new int2(currentFrame, currentAnim) * nativeSpriteSheet.spriteSizes;
    //    int2 sizes = nativeSpriteSheet.spriteSizes;

    //    Func<int2, int2, bool> canDrawPixel = (pixelPos, finalPos) => nativeSpriteSheet.pixels[sheetOffset + pixelPos].a != 0;
    //    Func<int2, int2, Color> getColor = (pixelPos, finalPos) => nativeSpriteSheet.pixels[sheetOffset + pixelPos];
    //    //We don't want to have flipped normal, reflip
    //    Func<int2, int2, Color> getNormal = (pixelPos, finalPos) => normalMap.pixels[pixelPos];
    //    Func<int2, int2, Color> getLightColor = (pixelPos, finalPos) => RenderingUtils.ApplyLightOnPixel(finalPos, pixelPos, lightSources, getColor, getNormal, 0, 0.5f, 25, isFlipped.x);

    //    GridRenderer.ApplyCustomRender(ref outputColors, renderPos, sizes, isFlipped, canDrawPixel, getLightColor, false);
    //}


    public void Dispose()
    {
        nativeSpriteSheet.Dispose();
        currentSprite.Dispose();
        currentNormal.Dispose();
        currentReflection.Dispose();
    }

}
