using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Jetpack : EquipableElement
{
    public JetpackScriptable settings;
    private int currentFuel;
    private int unUsedForTicks;

    private void Start()
    {
        currentFuel = settings.fuelCapacity;
    }

    public override void Use(int2 position)
    {
        if(currentFuel < settings.fuelUseRate)
            return;

        if(currentFuel == settings.fuelCapacity)
            player.physicData.velocity += settings.intialVelocity;
        else
            player.physicData.velocity += settings.jetpackVelocity;

        unUsedForTicks = 0;
        currentFuel -= settings.fuelUseRate;
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        base.OnUpdate(ref tickBlock);
        unUsedForTicks++;

        if(unUsedForTicks >= settings.refuelAfterXTick)
            currentFuel = math.min(currentFuel + settings.fuelRefillRate, settings.fuelCapacity);
    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    public override void OnRenderUI(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
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
