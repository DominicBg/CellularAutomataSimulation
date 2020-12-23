using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SpriteSheetObject : LevelObject
{
    [SerializeField] SpriteSheetScriptable spriteSheetScriptable = default;

    SpriteAnimator spriteAnimator;

    public override void Init(Map map)
    {
        base.Init(map);
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

    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        base.PreRender(ref outputColor, ref tickBlock);
        spriteAnimator.Render(ref outputColor, position, false);
    }

    public override void Dispose()
    {
        base.Dispose();
        spriteAnimator.Dispose();
    }
}
