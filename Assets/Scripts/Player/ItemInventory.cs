using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventory
{
    public enum Slot { Main, Secondary}
    EquipableElement[] equipableElements;

    public ItemInventory()
    {
        equipableElements = new EquipableElement[System.Enum.GetNames(typeof(Slot)).Length];
    }

    public void Equip(EquipableElement equipableElement, Slot slot)
    {
        //todo refactor unequip
        int index = (int)slot;
        equipableElements[index]?.Unequip();

        equipableElements[index] = equipableElement;
        equipableElements[index].Equip();
    }

    public void Update()
    {
        //todo bind each action to a key

        if (InputCommand.IsButtonHeld(ButtonType.Action1))
        {
            equipableElements[0]?.Use(false);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action1Alt))
        {
            equipableElements[0]?.Use(true);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action2))
        {
            equipableElements[1]?.Use(false);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action2Alt))
        {
            equipableElements[1]?.Use(true);
        }
    }
}
