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
    public bool2 isFlipped;

    public override void OnInit()
    {
        spriteAnimator = new SpriteAnimator(spriteSheetScriptable);
    }

    public override Bound GetBound()
    {
        return new Bound(position, spriteAnimator.nativeSpriteSheet.spriteSizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        spriteAnimator.Update(isFlipped);
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if(!inBackground)
            spriteAnimator.Render(ref outputColor, renderPos);
    }
    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if(inBackground)
            spriteAnimator.Render(ref outputColor, renderPos);
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
