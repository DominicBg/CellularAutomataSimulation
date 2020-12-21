﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class EquipableElement : LevelObject
{
    [HideInInspector] public PlayerElement player;
    public EquipableBaseScriptable baseSettings;

    public bool isEquiped = false;
    protected bool isLateUnequip;
    protected int2 unequipPosition;
    protected SpriteAnimator spriteAnimator;

    protected int cooldown;
    protected bool isUsedThisFrame = false;
    protected int unUsedForTicks;

    bool requestUse;
    int2 requestUsePosition;

    public void OnValidate()
    {
        player = GetLevelElement<PlayerElement>();
    }

    public override void Init(Map map)
    {
        base.Init(map);
        spriteAnimator = new SpriteAnimator(baseSettings.spriteSheet);
        spriteAnimator.framePerImage = baseSettings.framePerImage;
    }

    public sealed override void OnUpdate(ref TickBlock tickBlock)
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

        if(isEquiped)
        {
            if(requestUse)
            {
                InternalUse(requestUsePosition, ref tickBlock);
                isUsedThisFrame = true;
                unUsedForTicks = 0;
                requestUse = false;
            }
            else
            {
                unUsedForTicks++;
            }

            OnEquipableUpdate(ref tickBlock);

            isUsedThisFrame = false;
            cooldown = math.max(cooldown - 1, 0);
        }
    }

    public abstract void OnEquipableUpdate(ref TickBlock tickBlock);

    public void Unequip(int2 unequipPosition)
    {
        isLateUnequip = true;
        this.unequipPosition = unequipPosition;
    }

    public void Use(int2 pos)
    {
        requestUse = true;
        requestUsePosition = pos;
    }

    void InternalUse(int2 position, ref TickBlock tickBlock)
    {
        if (cooldown != 0)
            return;

        isUsedThisFrame = true;
        cooldown = baseSettings.frameCooldown;
        OnUse(position, ref tickBlock);
    }
    protected abstract void OnUse(int2 position, ref TickBlock tickBlock);

    protected abstract void OnEquip();
    protected abstract void OnUnequip();


    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        int2 renderPos = isEquiped ? GetEquipOffset(baseSettings.equipedOffset) : position;
        bool isFlipped = isEquiped ? player.lookLeft : false;
        spriteAnimator.Render(ref outputcolor, renderPos, isFlipped);
    }

    public override Bound GetBound()
    {
        return new Bound(position, spriteAnimator.nativeSpriteSheet.spriteSizes);
    }

    protected int2 GetEquipOffset(int2 offset)
    {
        if (player.lookLeft)
            offset.x = -offset.x;

        offset -= spriteAnimator.nativeSpriteSheet.spriteSizes / 2;
        return player.GetBound().center + offset;
    }
    protected int2 GetAjustedOffset(int2 offset)
    {
        if (player.lookLeft)
        {
            offset.x = spriteAnimator.nativeSpriteSheet.spriteSizes.x - offset.x - 1;
        }

        return offset;
    }
}
