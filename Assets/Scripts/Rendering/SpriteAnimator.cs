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

    public int currentAnim;
    public int currentFrame;

    public int framePerImage = 10;

    int currentTick;

    public bool returnToIdleAfterAnim = false;

    public void Update()
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
    }

    public void SetAnimation(int animation)
    {
        if (currentAnim == animation)
            return;

        currentTick = 0;
        currentFrame = 0;
        currentAnim = animation;
    }

    public SpriteAnimator(SpriteSheet spriteSheet)
    {
        nativeSpriteSheet = new NativeSpriteSheet(spriteSheet);
    }

    public void Render(ref NativeArray<Color32> outputColors, int2 position, bool isFlipped)
    {
        Render(ref outputColors, position, new bool2(isFlipped, false));
    }
    public void Render(ref NativeArray<Color32> outputColors, int2 position, bool2 isFlipped)
    {
        int2 sizes = nativeSpriteSheet.spriteSizes;
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int xx = (!isFlipped.x) ? x : sizes.x - x - 1;
                int yy = (!isFlipped.y) ? y : sizes.y - y - 1;

                int2 texturePos = new int2(x, y) + position;
                int2 pixelPos = new int2(xx, yy) + new int2(currentFrame, currentAnim) * nativeSpriteSheet.spriteSizes;
                if (GridHelper.InBound(texturePos, GameManager.GridSizes) && nativeSpriteSheet.pixels[pixelPos.x, pixelPos.y].a != 0)
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.GridSizes.x);
                    outputColors[index] = nativeSpriteSheet.pixels[pixelPos.x, pixelPos.y];
                }
            }
        }
    }

    public void DebugRender(ref NativeArray<Color32> outputColors, int2 position)
    {
        int2 sizes = nativeSpriteSheet.pixels.m_sizes;
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int2 texturePos = new int2(x, y) + position;
                if (GridHelper.InBound(texturePos, GameManager.GridSizes))
                {
                    int index = ArrayHelper.PosToIndex(texturePos, GameManager.GridSizes.x);
                    outputColors[index] = nativeSpriteSheet.pixels[x, y];
                }
            }
        }
    }

    public void Dispose()
    {
        nativeSpriteSheet.Dispose();
    }

}
