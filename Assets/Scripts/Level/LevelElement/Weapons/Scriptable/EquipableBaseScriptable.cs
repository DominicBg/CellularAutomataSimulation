using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EquipableBaseScriptable : ScriptableObject
{
    [Header("Equipable")]
    public int2 equipedOffset = new int2(-2, -1);
    public SpriteSheet spriteSheet;
    public int framePerImage = 5;
    public int frameCooldown = 2;
}
