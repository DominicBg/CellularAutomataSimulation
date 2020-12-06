using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class WeaponBaseElement : EquipableElement
{
    protected abstract void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map);

    public override void Use(int2 usePosition)
    {
        int2 aimPosition = GridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        int2 startPosition = usePosition + new int2(9, 3);
        float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
        OnShoot(startPosition, aimDirection, map);
    }
}
