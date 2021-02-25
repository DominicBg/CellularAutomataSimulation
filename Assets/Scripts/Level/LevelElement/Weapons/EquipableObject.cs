using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EquipableObject : LevelObject
{
    //maybe add sprite sheet?
    [SerializeField] Texture2D visualTexture;

    [SerializeField] EquipableElement equipable;
    //Add event to plug context stuff?

    NativeSprite sprite;
    public override void OnInit()
    {
        base.OnInit();
        sprite = new NativeSprite(visualTexture);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        base.OnUpdate(ref tickBlock);

        if(CollideWith(player))
        {
            //play audio
            //fire event for potential context switch
            player.inventory.Equip(equipable, ItemInventory.Slot.Secondary);
            isEnable = false;
            isVisible = false;
        }
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        GridRenderer.DrawSprite(ref outputColors, in sprite.pixels, renderPos, true);
    }



    public override Bound GetBound()
    {
        return sprite.GetBound(position);
    }

    public override void Dispose()
    {
        base.Dispose();
        sprite.Dispose();
    }
}
