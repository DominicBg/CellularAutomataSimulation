using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class World1GemShrineElement : SpriteSheetObject
{
    [SerializeField] PlayerElement player = default;
    [SerializeField] TimeGemElement timeGem = default;
    [SerializeField] World1GemContext context;

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        base.OnUpdate(ref tickBlock);
        if(InputCommand.IsButtonDown(KeyCode.E) && GetBound().IntersectWith(player.GetBound()))
        {
            context.OnEndCallBack = ContextCallBack;
            GameManager.Instance.SetContext(context);
        }
    }

    void ContextCallBack()
    {
        //show gems
        //when taken...
        player.EquipQ(timeGem);
        timeGem.isEquiped = true;
        isEnable = false;
        Debug.Log("equip gem");
        PlayAnimation(1);
    }

}
