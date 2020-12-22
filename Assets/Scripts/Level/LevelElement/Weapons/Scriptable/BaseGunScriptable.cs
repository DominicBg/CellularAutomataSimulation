using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class BaseGunScriptable : EquipableBaseScriptable
{
    public int2 shootOffset = new int2(-2, -1);
    public int2 kickDirection = new int2(-1, 0);
}
