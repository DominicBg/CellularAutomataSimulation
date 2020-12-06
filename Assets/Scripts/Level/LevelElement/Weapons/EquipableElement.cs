using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class EquipableElement : LevelObject
{
    [HideInInspector] public PlayerElement player;

    public SpriteEnum spriteVisual;

    public bool isEquiped = false;
    protected bool isLateUnequip;
    protected int2 unequipPosition;

    public void OnValidate()
    {
        player = GetComponent<PlayerElement>();
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if (!isEquiped && GetBound().IntersectWith(player.GetBound()) && InputCommand.IsButtonDown(KeyCode.E))
        {
            isEquiped = true;
            //isVisible = false;
            player.Equip(this);
            OnEquip();
        }

        //This caused a bug where you could equip and unequip in the same frame lol
        //need an anim or something
        if (isLateUnequip)
        {
            isLateUnequip = false;
            isEquiped = false;
            //isVisible = true;
            position = unequipPosition;
        }
    }


    public void Unequip(int2 unequipPosition)
    {
        isLateUnequip = true;
        this.unequipPosition = unequipPosition;
    }

    public abstract void Use(int2 position);

    protected abstract void OnEquip();
    protected abstract void OnUnequip();


    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplySprite(ref outputcolor, SpriteRegistry.GetSprite(spriteVisual), position);
    }

    public override Bound GetBound()
    {
        return SpriteRegistry.GetSprite(spriteVisual).GetBound(position);
    }
}
