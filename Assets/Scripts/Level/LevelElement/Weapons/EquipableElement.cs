using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class EquipableElement : LevelObject
{
    public EquipableBaseScriptable baseSettings;

    protected SpriteAnimator spriteAnimator;

    protected int cooldown;
    protected bool isUsedThisFrame = false;
    protected int unUsedForTicks;

    UseRequest useRequest;


    public override void OnInit()
    {
       spriteAnimator = new SpriteAnimator(baseSettings.spriteSheet);
       spriteAnimator.framePerImage = baseSettings.framePerImage;

        CheatManager.AddCheat("Equip " + gameObject.name + " Main", () => player.inventory.Equip(this, ItemInventory.Slot.Main));
        CheatManager.AddCheat("Equip " + gameObject.name + " Secondary", () => player.inventory.Equip(this, ItemInventory.Slot.Secondary));
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        isUsedThisFrame = false;
        spriteAnimator.Update(player.lookLeft);

        if (useRequest.requestUse)
        {
            InternalUse(ref tickBlock);
        }
        else
        {
            unUsedForTicks++;
        }
        position = GetEquipOffset(baseSettings.equipedOffset);
        OnEquipableUpdate(ref tickBlock);
        cooldown = math.max(cooldown - 1, 0);
    }

    public abstract void OnEquipableUpdate(ref TickBlock tickBlock);

    public void UsePress(bool useAltButton)
    {
        useRequest = new UseRequest()
        {
            requestUse = true,
            useAltButton = useAltButton,
        };
    }
    public void UseHold(bool useAltButton)
    {
        if (baseSettings.needButtonPress)
            return;

        useRequest = new UseRequest()
        {
            requestUse = true,
            useAltButton = useAltButton,
        };
    }

    void InternalUse(ref TickBlock tickBlock)
    {
        useRequest.requestUse = false;
        if (cooldown != 0)
            return;

        unUsedForTicks = 0;
        isUsedThisFrame = true;
        cooldown = baseSettings.frameCooldown;

        OnUse(position, useRequest.useAltButton, ref tickBlock);
    }
    protected abstract void OnUse(int2 position, bool altButton, ref TickBlock tickBlock);

    protected virtual void OnEquip() { }
    protected virtual void OnUnequip() { }
    public void Equip()
    {
        isVisible = true;
        isEnable = true;
        OnEquip();
    }
    public void Unequip()
    {
        isVisible = false;
        isEnable = false;
        OnUnequip();
    }


    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        spriteAnimator.Render(ref outputcolor, renderPos);
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
        return player.GetBound().center + offset + player.AnimOffset;
    }
    protected int2 GetWorldPositionOffset(int2 offset)
    {
        if (player.lookLeft)
        {
            offset.x = -offset.x;
        }

        return player.GetBound().center + offset;
    }

    public override void Dispose()
    {
        base.Dispose();
        spriteAnimator?.Dispose();
    }

    struct UseRequest
    {
        public bool requestUse;
        public int2 requestUsePosition;
        public bool useAltButton;
    }
}

