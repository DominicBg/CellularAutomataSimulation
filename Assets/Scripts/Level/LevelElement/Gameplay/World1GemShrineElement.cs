using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class World1GemShrineElement : LevelObject
{
    [SerializeField] int2 sizes;
    [SerializeField] PlayerElement player;
    [SerializeField] TimeGemElement timeGem;

    private void OnValidate()
    {
    }

    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(InputCommand.IsButtonDown(KeyCode.E) && GetBound().IntersectWith(player.GetBound()))
        {
            //show gems
            //when taken...
            player.EquipQ(timeGem);
            timeGem.isEquiped = true;
            isEnable = false;
            Debug.Log("equip gem");
        }
    }


}
