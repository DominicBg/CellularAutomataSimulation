using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteSheetScriptable", menuName = "Animation/SpriteSheetScriptable", order = 1)]
public class SpriteSheetScriptable : ScriptableObject
{
    public SpriteAnimationScriptable[] spriteAnimations;
}
