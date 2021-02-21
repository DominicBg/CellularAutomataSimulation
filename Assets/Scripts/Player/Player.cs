using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Player : CharacterController, ILightSource
{
    public PlayerControlSettings settings;
    public ItemInventory inventory;

    private int reflectionIndex;

    NativeSprite backPack;

    bool ghostMode;
    public float2 ViewDirection => InputCommand.HasInputDirection ? InputCommand.Get8Direction : (lookLeft ? new float2(-1, 0) : new float2(1, 0));

    public override void OnInit()
    {
        base.OnInit();
        inventory = new ItemInventory();
        backPack = new NativeSprite(settings.backpack.sprite, settings.backpack.normal);
        CheatManager.AddCheat("Ghost Mode", () => ghostMode = !ghostMode);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var sprite = spriteAnimator.GetCurrentSprite();
        var normals = spriteAnimator.GetCurrentNormals();

        //int2 backpackOffset = backPack.pixels.Sizes/2 + settings.backpack.offset * (lookLeft ? new int2(-1, 1) : new int2(1, 1));
        //GridRenderer.ApplyLitSprite(ref outputColors, backPack.pixels, backPack.normals, position + backpackOffset, renderPos + backpackOffset, info.lightSources, in settings.backpack.shadingInfo);
        GridRenderer.ApplyLitSprite(ref outputColors, sprite, normals, position, renderPos, info.lightSources, in settings.shadingInfo);
        
        reflectionIndex = info.GetReflectionIndex();
        GridRenderer.PrepareSpriteEnvironementReflection(sprite, renderPos, ref info, reflectionIndex);
    }

    public override void RenderReflection(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var sprite = spriteAnimator.GetCurrentSprite();
        var normals = spriteAnimator.GetCurrentNormals();
        var reflections = spriteAnimator.GetCurrentReflections();
        GridRenderer.ApplySpriteSkyboxReflection(ref outputColors, sprite, normals, reflections, renderPos, ref info, in settings.skyReflection);
        GridRenderer.ApplySpriteEnvironementReflection(ref outputColors, sprite, normals, reflections, renderPos, reflectionIndex, ref info, in settings.environementReflection);
    }


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if (ghostMode)
        {
            physicData.position += (InputCommand.Direction * 360 * GameManager.DeltaTime);
            position = (int2)physicData.position;
            return;
        }
        var underFeetPositionBeforePhysics = physicData.physicBound.GetUnderFeetCollisionBound(position).GetPositionsGrid();

        base.OnUpdate(ref tickBlock);

        var underFeetPositionAfterPhysics = physicData.physicBound.GetUnderFeetCollisionBound(position).GetPositionsGrid();

        //When you move, give force to particle beneath you
        for (int i = 0; i < underFeetPositionBeforePhysics.Length; i++)
        {
            bool found = false;
            int2 pos = underFeetPositionBeforePhysics[i];
            for (int j = 0; j < underFeetPositionAfterPhysics.Length; j++)
            {
                found |= math.all(pos == underFeetPositionAfterPhysics[j]);
            }
            if (!found)
            {
                if (map.CanPush(pos, GameManager.PhysiXVIISetings))
                {
                    Particle particle = map.GetParticle(pos);
                    particle.velocity += new float2(lookLeft ? settings.walkingForce.x : -settings.walkingForce.x, settings.walkingForce.y) * GameManager.DeltaTime;
                    map.SetParticle(pos, particle);
                }
            }
        }

        underFeetPositionBeforePhysics.Dispose();
        underFeetPositionAfterPhysics.Dispose();

        inventory.Update();

    }

    public override void Dispose()
    {
        base.Dispose();
        backPack.Dispose();
    }

    public LightSource GetLightSource(int tick)
    {
        return settings.lightSourceSettings.GetLightSource(GetBound().center, tick);
    }

    bool ILightSource.IsVisible() => isVisible;
}
