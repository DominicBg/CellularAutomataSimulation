using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Animation/SpriteAnimation", order = 1)]
public class SpriteAnimationScriptable : ScriptableObject
{
    //to use in enum generator
    public Texture2D[] sprites;
    public Texture2D[] normals;
    public Texture2D[] reflections;

    [Header("Extra")]
    public int2[] offsets;
}