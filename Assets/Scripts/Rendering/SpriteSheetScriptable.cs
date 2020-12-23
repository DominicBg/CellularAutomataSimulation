using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteSheetScriptable", menuName = "SpriteSheetScriptable", order = 1)]
public class SpriteSheetScriptable : ScriptableObject
{
    public SpriteSheet spriteSheet;
}

[System.Serializable]
public struct SpriteSheet
{
    public string spriteSheetName;
    public SpriteAnimation[] spriteAnimations;

    [System.Serializable]
    public class SpriteAnimation
    {
        //to use in enum generator
        public string animationName;
        public Texture2D[] sprites;
    }
}
