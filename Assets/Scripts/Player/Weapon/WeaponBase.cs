using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class WeaponBase
{
    public abstract void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map);
}
