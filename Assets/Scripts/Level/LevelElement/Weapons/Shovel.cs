using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Shovel : EquipableElement
{
    public ShovelScriptable settings => (ShovelScriptable)baseSettings;

    List<int2> debugPositions = new List<int2>();

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        debugPositions.Clear();

        float2 dir = math.normalize(settings.throwDirVelocity);
        int2 startOffset = settings.throwStartOffset;

        if (player.lookLeft)
        {
            dir.x = -dir.x;
            startOffset.x = -startOffset.x;
        }
        if (altButton)
        {
            dir.x = -dir.x;
            startOffset.x = -startOffset.x;
        }

        bool ascOrder = player.lookLeft;
        if (settings.flipPhysics)
            ascOrder = !ascOrder;

        //todo stop loops if blocked?
        int2 offset = GetEquipOffset(settings.lookingOffset);
        for (int y = 0; y < settings.shovelSize.y; y++)
        {
            if (ascOrder)
            {
                for (int x = 0; x < settings.shovelSize.x; x++)
                {
                    int2 localPos = new int2(x, y);
                    int2 pos = offset + localPos;
                    ThrowParticle(pos, localPos,  dir, startOffset, ref tickBlock);
                }
            }
            else
            {
                for (int x = settings.shovelSize.x - 1; x >= 0; x--)
                {
                    int2 localPos = new int2(x, y);
                    int2 pos = offset + localPos;
                    ThrowParticle(pos, localPos, dir, startOffset, ref tickBlock);
                }
            }
        }
    }

    //void ThrowParticle(int2 pos, float2 dir, ref TickBlock tickBlock)
    //{
    //    if (map.InBound(pos) && map.CanPush(pos, GameManager.PhysiXVIISetings))
    //    {
    //        int2 findPosDir = (int2)math.sign(dir);

    //        //Create a flag somewhere for pushable?
    //        int ignoreFilter = PhysiXVII.GetFlag(ParticleType.Player, ParticleType.Sand, ParticleType.Cinder, ParticleType.Rubble);
    //        if (map.TryFindEmptyPosition(pos, findPosDir, out int2 newPos, 15, ignoreFilter))
    //        {
    //            Particle particle = map.GetParticle(pos);
    //            float strength = math.lerp(settings.minThrowStrength, settings.maxThrowStrength, tickBlock.random.NextFloat());
    //            particle.velocity = dir * strength;
    //            map.SetParticle(pos, particle);

    //            //might move one twice?
    //            map.MoveParticle(pos, newPos);
    //        }
    //    }
    //}

    void ThrowParticle(int2 pos, int2 localPos, float2 dir, int2 startOffset, ref TickBlock tickBlock)
    {
        pos = math.clamp(pos, 0, GameManager.GridSizes - 1);

        debugPositions.Add(pos);
        debugPositions.Add(GetFinalPosition(pos, localPos, startOffset, dir));

        if (map.InBound(pos) && map.CanPush(pos, GameManager.PhysiXVIISetings))
        {
            //int ignoreFilter = PhysiXVII.GetFlag(ParticleType.Player, ParticleType.Sand, ParticleType.Cinder, ParticleType.Rubble);

            //int2 desiredPos = pos + startOffset + new int2((int)math.sign(dir.x) * settings.yshifting * localPos.y, 0);
            //desiredPos = math.clamp(desiredPos, 0, GameManager.GridSizes - 1);

            //int2 finalPos = map.FindParticlePosition(pos, desiredPos, ignoreFilter);

            int2 finalPosition = GetFinalPosition(pos, localPos, startOffset, dir);
            Particle particle = map.GetParticle(pos);
            float strength = tickBlock.random.NextFloat(settings.minThrowStrength, settings.maxThrowStrength);
            particle.velocity = dir * strength;
            map.MoveParticle(particle, pos, finalPosition);
            //map.SetParticle(pos, particle);
            //map.Set(pos, finalPosition);
        }
    }

    int2 GetFinalPosition(int2 pos, int2 localPos, int2 startOffset, float2 dir)
    {
        int ignoreFilter = PhysiXVII.GetFlag(ParticleType.Player, ParticleType.Sand, ParticleType.Cinder, ParticleType.Rubble);

        int2 desiredPos = pos + startOffset + new int2((int)math.sign(dir.x) * settings.yshifting * localPos.y, 0);
        desiredPos = math.clamp(desiredPos, 0, GameManager.GridSizes - 1);

        return map.FindParticlePosition(pos, desiredPos, ignoreFilter);
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
        for (int i = 0; i < debugPositions.Count; i++)
        {
            int index = ArrayHelper.PosToIndex(debugPositions[i], GameManager.GridSizes);
            outputColor[index] = Color.Lerp(outputColor[index], Color.red, 0.25f); 
        }
        //int2 offset = GetEquipOffset(settings.lookingOffset);
        //for (int x = 0; x < settings.shovelSize.x; x++)
        //{
        //    for (int y = 0; y < settings.shovelSize.y; y++)
        //    {
        //        int2 localPos = new int2(x, y);
        //        int2 pos = offset + localPos;
        //        pos = math.clamp(pos, 0, GameManager.GridSizes - 1);

        //        int index = ArrayHelper.PosToIndex(pos, GameManager.GridSizes);
        //        if (map.InBound(pos))
        //            outputColor[index] = Color.Lerp(outputColor[index], Color.red, 0.5f);

        //        int2 throwOffset = settings.throwStartOffset;
        //        int2 throwOffset2 = settings.throwStartOffset;
        //        throwOffset2.x = -throwOffset2.x;

        //        int2 desiredPos1 = pos + throwOffset + new int2(settings.yshifting * localPos.y, 0);
        //        int2 desiredPos2 = pos + throwOffset2 - new int2(settings.yshifting * localPos.y, 0);
        //        if (map.InBound(desiredPos1))
        //        {
        //            int index1 = ArrayHelper.PosToIndex(desiredPos1, GameManager.GridSizes);
        //            outputColor[index1] = Color.Lerp(outputColor[index1], Color.red, 0.25f);
        //        }
        //        if (map.InBound(desiredPos2))
        //        {
        //            int index2 = ArrayHelper.PosToIndex(desiredPos2, GameManager.GridSizes);
        //            outputColor[index2] = Color.Lerp(outputColor[index2], Color.red, 0.25f); 
        //        }
        //    }
        //}
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
