using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "JetpackScriptable", menuName = "Equipable/JetpackScriptable", order = 1)]
public class JetpackScriptable : EquipableBaseScriptable
{
    [Header("Jetpack")]
    public float2 jetpackVelocity = new float2(0, 13);
    public float2 intialVelocity = new float2(0, 55);
    public int fuelCapacity = 60;
    public int fuelRefillRate = 1;
    public int fuelUseRate = 5;
    public int refuelAfterXTick = 5;
}
