using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class WeaponBaseElement : LevelObject
{
    public bool isEquiped = false;
    [HideInInspector] public PlayerElement player;

    public SpriteEnum spriteVisual;

    bool isLateUnequip;
    int2 unequipPosition;

    public void OnValidate()
    {
        player = GetComponent<PlayerElement>();
    }

    protected abstract void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map);
    protected abstract void OnWeaponEquip();
    protected abstract void OnWeaponUnequip();

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(!isEquiped && GetBound().IntersectWith(player.GetBound()) && InputCommand.IsButtonDown(KeyCode.E))
        {
            isEquiped = true;
            isVisible = false;
            player.EquipWeapon(this);
            OnWeaponEquip();
        }

        //This caused a bug where you could equip and unequip in the same frame lol
        //need an anim or something
        if(isLateUnequip)
        {
            isLateUnequip = false;
            isEquiped = false;
            isVisible = true;
            position = unequipPosition;
        }

    }

    public void UnequipWeapon(int2 unequipPosition)
    {
        isLateUnequip = true;
        this.unequipPosition = unequipPosition;
    }

    public void UseWeapon(int2 usePosition)
    {
        int2 aimPosition = GridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        int2 startPosition = usePosition + new int2(9, 3);
        float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
        OnShoot(startPosition, aimDirection, map);
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplySprite(ref outputcolor, SpriteRegistry.GetSprite(spriteVisual), position);
    }

    public override Bound GetBound()
    {
        return SpriteRegistry.GetSprite(spriteVisual).GetBound(position);
    }
}
