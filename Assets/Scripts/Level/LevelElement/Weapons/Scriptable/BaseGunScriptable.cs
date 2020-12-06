using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class BaseGunScriptable : ScriptableObject
{
    public int2 equipedOffset = new int2(-2, -1);
    public int2 shootOffset = new int2(-2, -1);
    public NativeSpriteSheet.SpriteSheet spriteSheet;
    public int framePerImage = 5;

    public int2 kickDirection = new int2(-1, 0);
}
