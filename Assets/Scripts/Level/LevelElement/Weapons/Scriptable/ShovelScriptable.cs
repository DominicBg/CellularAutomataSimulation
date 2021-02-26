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
    public float throwObjectStrength;

    public float2 throwDirVelocity;
    public int2 animOffset;

    public bool flipPhysics;

    public int2 throwStartOffset;
    public float yshifting;
}
