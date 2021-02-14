using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//Is center aligned
public class SpriteStaticObject : LevelObject
{
    public Texture2D texture;

    protected NativeSprite nativeSprite;
    public bool hasCollision;
    public bool isInBackground;
    public bool2 isFlipped;

    public override void OnInit()
    {
        nativeSprite = new NativeSprite(texture);
        TrySetCollision();
    }

    protected void TrySetCollision()
    {
        if (hasCollision && GameManager.CurrentState != GameManager.GameStateEnum.LevelEditor)
        {
            for (int x = 0; x < nativeSprite.sizes.x; x++)
            {
                for (int y = 0; y < nativeSprite.sizes.y; y++)
                {
                    if (nativeSprite.pixels[x, y].a != 0)
                    {
                        //COLLISION MIGHT BREAK WITH FLIP
                        int2 mapPos = position + new int2(x, y) - (nativeSprite.sizes / 2);
                        map.SetParticleType(mapPos, ParticleType.Collision);
                    }
                }
            }
        }
    }

    public override Bound GetBound()
    {
        return Bound.CenterAligned(position, nativeSprite.sizes);
    }

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if(isInBackground)
            GridRenderer.ApplySprite(ref outputColors, nativeSprite, renderPos, isFlipped, true);
    }
    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (!isInBackground)
            GridRenderer.ApplySprite(ref outputColors, nativeSprite, renderPos, isFlipped, true);
    }

    public override void Dispose()
    {
        nativeSprite.Dispose();
    }
}
