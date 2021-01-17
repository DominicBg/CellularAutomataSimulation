using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SpriteSheetObject : LevelObject
{
    [SerializeField] SpriteSheetScriptable spriteSheetScriptable = default;

    SpriteAnimator spriteAnimator;
    public bool inBackground;

    public override void OnInit()
    {
        spriteAnimator = new SpriteAnimator(spriteSheetScriptable.spriteSheet);
    }

    public override Bound GetBound()
    {
        return new Bound(position, spriteAnimator.nativeSpriteSheet.spriteSizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        spriteAnimator.Update();
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        if(!inBackground)
            spriteAnimator.Render(ref outputColor, renderPos, false);
    }
    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        if(inBackground)
            spriteAnimator.Render(ref outputColor, renderPos, false);
    }

    public override void Dispose()
    {
        base.Dispose();
        spriteAnimator.Dispose();
    }

    public void PlayAnimation(int animation)
    {
        spriteAnimator.SetAnimation(animation);
    }
}
