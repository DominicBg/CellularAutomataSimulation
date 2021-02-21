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
        //lol
        if (InputCommand.IsButtonDown(ButtonType.Action1))
        {
            equipableElements[0]?.UsePress(false);
        }
        else if (InputCommand.IsButtonDown(ButtonType.Action1Alt))
        {
            equipableElements[0]?.UsePress(true);
        }
        else if (InputCommand.IsButtonDown(ButtonType.Action2))
        {
            equipableElements[1]?.UsePress(false);
        }
        else if (InputCommand.IsButtonDown(ButtonType.Action2Alt))
        {
            equipableElements[1]?.UsePress(true);
        }

        //todo bind each action to a key
        if (InputCommand.IsButtonHeld(ButtonType.Action1))
        {
            equipableElements[0]?.UseHold(false);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action1Alt))
        {
            equipableElements[0]?.UseHold(true);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action2))
        {
            equipableElements[1]?.UseHold(false);
        }
        else if (InputCommand.IsButtonHeld(ButtonType.Action2Alt))
        {
            equipableElements[1]?.UseHold(true);
        }
    }
}
