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
        //eww
        var worldLevel = FindObjectOfType<WorldLevel>();
        if (!worldLevel.updatLevelElement)
        {
            return;
        }
  

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
        //int2 offset = GetEquipOffset(position, settings.lookingOffset);
        for (int y = 0; y < settings.shovelSize.y; y++)
        {
            if (ascOrder)
            {
                for (int x = 0; x < settings.shovelSize.x; x++)
                {
                    int2 localPos = new int2(x, y);
                    int2 pos = position + localPos;
                    ThrowParticle(pos, localPos,  dir, startOffset, ref tickBlock);
                }
            }
            else
            {
                for (int x = settings.shovelSize.x - 1; x >= 0; x--)
                {
                    int2 localPos = new int2(x, y);
                    int2 pos = position + localPos;
                    ThrowParticle(pos, localPos, dir, startOffset, ref tickBlock);
                }
            }
        }
        //try move up one position
        PhysicBound playerPhysicbound = player.physicData.physicBound;
        Bound playerOverBound = playerPhysicbound.GetTopCollisionBound(player.physicData.gridPosition + new int2(0, 1));
        Bound playerUnderBound = playerPhysicbound.GetBottomCollisionBound(player.physicData.gridPosition + new int2(0, 1));

        playerOverBound.GetPositionsGrid(out NativeArray<int2> headCollisions, Allocator.Temp);
        playerUnderBound.GetPositionsGrid(out NativeArray<int2> feetCollisions, Allocator.Temp);

        bool isBlocked = false;
        var physixSettings = GameManager.PhysiXVIISetings;
        for (int i = 0; i < headCollisions.Length; i++)
        {
            if (!(map.CanPush(headCollisions[i], in physixSettings) || map.IsFreePosition(headCollisions[i])))
            {
                isBlocked = true;
            }
        }
        if(!isBlocked)
        {
            map.RemoveSpriteAtPosition(player.physicData.gridPosition, ref playerPhysicbound);
            for (int i = 0; i < headCollisions.Length; i++)
            {
                map.SwapParticles(headCollisions[i], feetCollisions[i]);
            }
            player.physicData.position += new float2(0, 1);
            player.physicData.gridPosition = (int2)player.physicData.position;
            //map.SetSpriteAtPosition(player.physicData.gridPosition, ref playerPhysicbound);

        }
        headCollisions.Dispose();
        feetCollisions.Dispose();
    }

    void ThrowParticle(int2 pos, int2 localPos, float2 dir, int2 startOffset, ref TickBlock tickBlock)
    {
        if (!map.InBound(pos))
            return;

        debugPositions.Add(pos);
        debugPositions.Add(GetDesiredPosition(pos, localPos, startOffset, dir));

        if (map.CanPush(pos, GameManager.PhysiXVIISetings))
        {
            //add shovel premade flag?
            int ignoreFilter = PhysiXVII.GetFlag(ParticleType.Player, ParticleType.Sand, ParticleType.Cinder, ParticleType.Rubble);
            int2 desiredPos = GetDesiredPosition(pos, localPos, startOffset, dir);
            int2 finalPosition = map.FindParticlePosition(pos, desiredPos, ignoreFilter);

        
            Particle particle = map.GetParticle(pos);
            if (map.GetParticleType(finalPosition) != ParticleType.None)
            {
                //Debug.Log("eh what can ya do?");
                return;
            }
            float strength = tickBlock.random.NextFloat(settings.minThrowStrength, settings.maxThrowStrength);
            particle.velocity = dir * strength;
            map.MoveParticle(particle, pos, finalPosition);
        }
    }

    int2 GetDesiredPosition(int2 pos, int2 localPos, int2 startOffset, float2 dir)
    {
        int2 desiredPos = pos + startOffset + new int2((int)math.sign(dir.x) * (int)(settings.yshifting * localPos.y), 0);
        desiredPos = math.clamp(desiredPos, 0, map.Sizes - 1);

        return desiredPos;
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, int2 renderPos)
    {
        bool playAnim = cooldown > 0 && cooldown > settings.frameCooldown / 2;
        int2 animOffset = playAnim ? settings.animOffset :0;
        //int2 finalRenderPos = isEquiped ? GetEquipOffset(renderPos, renderOffset) : renderPos;
        bool2 flipped;
        flipped.x = isEquiped ? player.lookLeft : false;
        flipped.y = isEquiped && playAnim;
        spriteAnimator.Render(ref outputcolor, renderPos + animOffset, flipped);
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        for (int i = 0; i < debugPositions.Count; i++)
        {
            int index = ArrayHelper.PosToIndex(debugPositions[i], GameManager.GridSizes);
            outputColor[index] = Color.Lerp(outputColor[index], Color.red, 0.25f); 
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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerElement player = FindObjectOfType<PlayerElement>();
            position = player.position;
        }
    }
}
