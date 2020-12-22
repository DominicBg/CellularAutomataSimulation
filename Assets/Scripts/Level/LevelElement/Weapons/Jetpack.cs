using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Jetpack : EquipableElement
{
    public JetpackScriptable settings => (JetpackScriptable)baseSettings;
    private int currentFuel;

    public override void Init(Map map)
    {
        base.Init(map);
        currentFuel = settings.fuelCapacity;
        spriteAnimator = new SpriteAnimator(settings.spriteSheet);
    }

    protected override void OnUse(int2 position, ref TickBlock _)
    {
        if (currentFuel < settings.fuelUseRate)
        {
            spriteAnimator.SetAnimation(0);
            return;
        }
        if(currentFuel == settings.fuelCapacity)
            player.physicData.velocity += settings.intialVelocity;
        else
            player.physicData.velocity += settings.jetpackVelocity;

        unUsedForTicks = 0;
        currentFuel -= settings.fuelUseRate;

        spriteAnimator.SetAnimation(1);

    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
        spriteAnimator.Update();
        if (unUsedForTicks >= settings.refuelAfterXTick)
        {
            currentFuel = math.min(currentFuel + settings.fuelRefillRate, settings.fuelCapacity);
            spriteAnimator.SetAnimation(0);
        }
    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        if (isEquiped)
        {
            int2 offset = settings.equipedOffset;

            if (player.lookLeft)
                offset.x = -offset.x;

            offset -= spriteAnimator.nativeSpriteSheet.spriteSizes / 2;
            spriteAnimator.Render(ref outputcolor, player.GetBound().center + offset, false);
        }
        else
            spriteAnimator.Render(ref outputcolor, position, false);
    }

    public override void RenderUI(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        if(isEquiped && currentFuel != settings.fuelCapacity)
        {
            //dont hardcode player height

            const int playersHeight = 9;
            float ratio = currentFuel / (float)settings.fuelCapacity;
            int gridAmmount = (int)(ratio * playersHeight);
            for (int i = 0; i < gridAmmount; i++)
            {
                int index = ArrayHelper.PosToIndex(player.position + new int2(-2, i), GameManager.GridSizes);
                outputcolor[index] = Color.white;
            }
        }
    }
}
