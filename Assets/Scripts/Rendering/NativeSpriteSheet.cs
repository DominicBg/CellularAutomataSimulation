using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct NativeSpriteSheet : IDisposable
{
    public NativeGrid<Color32> pixels;
    public NativeArray<int> spritePerAnim;
    public int2 spriteSizes;
    public int2 colRows;

    public NativeSpriteSheet(SpriteSheet spriteSheet)
    {
        int rows = spriteSheet.spriteAnimations.Length;

        int cols = 0;

        for (int i = 0; i < rows; i++)
        {
            cols = math.max(cols, spriteSheet.spriteAnimations[i].sprites.Length);
        }

        var firstSprite = spriteSheet.spriteAnimations[0].sprites[0];
        spriteSizes = new int2(firstSprite.width, firstSprite.height);
        colRows = new int2(cols, rows);

        pixels = new NativeGrid<Color32>(spriteSizes * colRows, Allocator.Persistent);
        spritePerAnim = new NativeArray<int>(rows, Allocator.Persistent);

        for (int row = 0; row < rows; row++)
        {
            int currentCol = spriteSheet.spriteAnimations[row].sprites.Length;
            spritePerAnim[row] = currentCol;
            for (int col = 0; col < currentCol; col++)
            {
                //offset to have the first anim on top of the spritesheet
                int2 offset = new int2(col, row) * spriteSizes;
                Texture2D currentSprite = spriteSheet.spriteAnimations[row].sprites[col];
                StoreSprite(currentSprite, offset);
            }
        }
    }

    void StoreSprite(Texture2D texture, int2 offset)
    {
        Color32[] colors = texture.GetPixels32(0);

        int2 sizes = new int2(texture.width, texture.height);

        if(math.all(spriteSizes != sizes))
        {
            Debug.LogError("Unconsistend SpriteSheet sizes");
        }

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                pixels[x + offset.x, y + offset.y] = colors[y * sizes.x + x];
            }
        }
    }

    public void Dispose()
    {
        pixels.Dispose();
        spritePerAnim.Dispose();
    }

    [System.Serializable]
    public class SpriteSheet
    {
        public string spriteSheetName;
        public SpriteAnimation[] spriteAnimations;
    }

    [System.Serializable]
    public class SpriteAnimation
    {
        //to use in enum generator
        public string animationName;
        public Texture2D[] sprites;
    }
}
