using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Shovel : EquipableElement
{
    public ShovelScriptable settings => (ShovelScriptable)baseSettings;

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        float2 dir = settings.throwDir;
        if (player.lookLeft)
            dir.x = -dir.x;
        if (altButton)
            dir.x = -dir.x;

        bool ascOrder = player.lookLeft;
        if (settings.flipPhysics)
            ascOrder = !ascOrder;

        //todo stop loops if blocked?
        int2 offset = GetEquipOffset(settings.lookingOffset);
        for (int y = 0; y < settings.shovelSize.y; y++)
        {
            if(ascOrder)
            {
                for (int x = 0; x < settings.shovelSize.x; x++)
                {
                    int2 pos = offset + new int2(x, y);
                    ThrowParticle(pos, dir, ref tickBlock);
                }
            }
            else
            {
                for (int x = settings.shovelSize.x - 1; x >= 0; x--)
                {
                    int2 pos = offset + new int2(x, y);
                    ThrowParticle(pos, dir, ref tickBlock);
                }
            }
        }
    }

    void ThrowParticle(int2 pos, float2 dir, ref TickBlock tickBlock)
    {
        if (map.InBound(pos) && map.CanPush(pos, GameManager.PhysiXVIISetings))
        {
            int2 findPosDir = (int2)math.sign(dir);

            //Create a flag somewhere for pushable?
            int ignoreFilter = (int)(ParticleType.Player | ParticleType.Sand);
            if (map.TryFindEmptyPosition(pos, findPosDir, out int2 newPos, 15, ignoreFilter))
            {
                Particle particle = map.GetParticle(pos);
                float strength = math.lerp(settings.minThrowStrength, settings.maxThrowStrength, tickBlock.random.NextFloat());
                particle.velocity = dir * strength;
                map.SetParticle(pos, particle);

                //might move one twice?
                map.MoveParticle(pos, newPos);
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
        spriteAnimator.Render(ref outputcolor, renderPos, flipped);
    }

    public override void OnRenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {       
        int2 offset = GetEquipOffset(settings.lookingOffset);
        for (int x = 0; x < settings.shovelSize.x; x++)
        {
            for (int y = 0; y < settings.shovelSize.y; y++)
            {
                int2 pos = offset + new int2(x, y);
                if (map.InBound(pos))
                    outputColor[ArrayHelper.PosToIndex(pos, GameManager.GridSizes)] = Color.red;
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
