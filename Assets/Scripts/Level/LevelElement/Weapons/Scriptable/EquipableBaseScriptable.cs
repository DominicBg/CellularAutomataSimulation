using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipableBaseScriptable", menuName = "Equipable/EquipableBaseScriptable", order = 1)]
public class EquipableBaseScriptable : ScriptableObject
{
    [Header("Equipable")]
    public int2 equipedOffset = new int2(-2, -1);
    public SpriteSheetScriptable spriteSheet;
    public int framePerImage = 5;
    public int frameCooldown = 2;
    public bool needButtonPress;
}

