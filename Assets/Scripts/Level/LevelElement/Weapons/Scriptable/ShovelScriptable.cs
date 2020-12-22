using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ShovelSettingsScriptable", menuName = "Equipable/ShovelSettingsScriptable", order = 1)]
public class ShovelScriptable : EquipableBaseScriptable
{
    [Header("Shovel")]
    public int2 shovelSize = 3;
    public int2 lookingOffset;
    public float minThrowStrength;
    public float maxThrowStrength;
    public int2 animOffset;
    public bool flipPhysics;
    public bool showDebug;
}
