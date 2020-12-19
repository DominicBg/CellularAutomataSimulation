using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Shovel : EquipableElement
{
    public ShovelScriptable settings => (ShovelScriptable)baseSettings;

    protected override void OnUse(int2 position)
    {
        float2 throwVelocity = settings.velocity;
        if (player.lookLeft)
        {
            throwVelocity.x = -throwVelocity.x;
        }

        //todo stop loops if blocked
        int2 offset = GetEquipOffset(settings.lookingOffset);
        for (int x = 0; x < settings.shovelSize.x; x++)
        {
            for (int y = 0; y < settings.shovelSize.y; y++)
            {
                int2 pos = offset + new int2(x, y);
                int2 findPosDir = new int2((int)math.sign(throwVelocity.x), 1);
                int2 moveOffset = pos + new int2((int)math.sign(throwVelocity.x) * 5, 8);
                if (map.InBound(pos) && map.CanPush(pos, GameManager.PhysiXVIISetings) && map.TryFindEmptyPosition(moveOffset, findPosDir, out int2 newPos))
                {
                    Particle particle = map.GetParticle(pos);
                    particle.velocity += throwVelocity;
                    map.SetParticle(pos, particle);
                    map.MoveParticle(pos, newPos);                     
                }
            }
        }
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        bool playAnim = cooldown > 0 && cooldown > settings.frameCooldown / 2;
        int2 renderOffset = playAnim ? baseSettings.equipedOffset + settings.animOffset : baseSettings.equipedOffset;
        int2 renderPos = isEquiped ? GetEquipOffset(renderOffset) : position;
        bool2 flipped;
        flipped.x = isEquiped ? player.lookLeft : false;
        flipped.y = isEquiped && playAnim;
        //int2 animHeightOffset = cooldown > 0 ? GetAjustedOffset(settings.animOffset) : 0;
        spriteAnimator.Render(ref outputcolor, renderPos, flipped);

        //base.OnRender(ref outputcolor, ref tickBlock);

        if (settings.showDebug)
        {
            int2 offset = GetEquipOffset(settings.lookingOffset);
            for (int x = 0; x < settings.shovelSize.x; x++)
            {
                for (int y = 0; y < settings.shovelSize.y; y++)
                {
                    int2 pos = offset + new int2(x, y);
                    if (map.InBound(pos))
                        outputcolor[ArrayHelper.PosToIndex(pos, GameManager.GridSizes)] = Color.red;

                }
            }
        }
    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
    }
}
