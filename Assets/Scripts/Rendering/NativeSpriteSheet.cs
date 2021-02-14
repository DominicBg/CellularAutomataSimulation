using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct NativeSpriteSheet : IDisposable
{
    public NativeGrid<Color32> pixels;
    public NativeGrid<float3> normals;
    public NativeGrid<float> reflections;

    public NativeArray<int> spritePerAnim;
    public int2 spriteSizes;
    public int2 colRows;

    public NativeSpriteSheet(SpriteSheetScriptable spriteSheet)
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
        normals = new NativeGrid<float3>(spriteSizes * colRows, Allocator.Persistent);
        reflections = new NativeGrid<float>(spriteSizes * colRows, Allocator.Persistent);

        spritePerAnim = new NativeArray<int>(rows, Allocator.Persistent);

        bool useNormal = spriteSheet.spriteAnimations[0].normals != null && spriteSheet.spriteAnimations[0].normals.Length > 0;
        bool useReflection = spriteSheet.spriteAnimations[0].reflections != null && spriteSheet.spriteAnimations[0].reflections.Length > 0; ;

        for (int row = 0; row < rows; row++)
        {
            int currentCol = spriteSheet.spriteAnimations[row].sprites.Length;
            spritePerAnim[row] = currentCol;
            for (int col = 0; col < currentCol; col++)
            {
                //offset to have the first anim on top of the spritesheet
                int2 offset = new int2(col, row) * spriteSizes;
                Texture2D currentSprite = spriteSheet.spriteAnimations[row].sprites[col];
                Texture2D currentNormal = (useNormal) ? spriteSheet.spriteAnimations[row].normals[col] : null;
                Texture2D currentReflection = (useReflection) ? spriteSheet.spriteAnimations[row].reflections[col] : null;
                StoreSprite(currentSprite, currentNormal, currentReflection, offset);
            }
        }
    }

    void StoreSprite(Texture2D sprite, Texture2D normal, Texture2D reflection, int2 offset)
    {
        bool useNormal = normal != null;
        bool useReflection = reflection != null;

        Color32[] spriteColors = sprite.GetPixels32(0);
        Color32[] normalColors = useNormal ? normal.GetPixels32(0) : null;
        Color32[] reflectionColors = useReflection ? reflection.GetPixels32(0) : null;

        int2 sizes = new int2(sprite.width, sprite.height);

        if(math.all(spriteSizes != sizes))
        {
            Debug.LogError("Unconsistend SpriteSheet sizes");
        }

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                int i = y * sizes.x + x;
                pixels[x + offset.x, y + offset.y] = spriteColors[i];
                if(useNormal)
                    normals[x + offset.x, y + offset.y] = ((Color)normalColors[i]).ToNormal();
                
                //Reflections are black n white, so we can use r g or b
                if (useReflection)
                    reflections[x + offset.x, y + offset.y] = ((Color)reflectionColors[i]).r;
            }
        }
    }

    public void Dispose()
    {
        pixels.Dispose();
        normals.Dispose();
        reflections.Dispose();
        spritePerAnim.Dispose();
    }
}
