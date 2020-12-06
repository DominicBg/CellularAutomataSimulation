using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerElement : PhysicObject
{
    public PlayerControlSettings settings;

    public SpriteAnimator spriteAnimator;
    EquipableElement currentEquip;
    int lookDirection;
    bool lookLeft;

    public override Bound GetBound()
    {
        return new Bound(position, GetNativeSprite().sizes);
    }

    public override void Init(GameLevelManager gameLevelManager, Map map)
    {
        base.Init(gameLevelManager, map);
        spriteAnimator = new SpriteAnimator(settings.spriteSheet);
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        if(lookDirection != 0)
        {
            lookLeft = lookDirection == -1;
        }

        //spriteAnimator.DebugRender(ref outputcolor, position);
        spriteAnimator.Render(ref outputcolor, position, lookLeft);
        //GridRenderer.ApplySprite(ref outputcolor, GetNativeSprite(), position, lookLeft);
        DebugAllPhysicBound(ref outputcolor);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(currentEquip != null && Input.GetMouseButton(0))
        {
            currentEquip.Use(position);
        }

        int2 direction = InputCommand.Direction;
        lookDirection = direction.x;

        if(direction.x == 0)
        {
            spriteAnimator.SetAnimation(0);
        }
        else
        {
            spriteAnimator.SetAnimation(1);
        }
        spriteAnimator.Update();

        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            physicData.controlledVelocity = (float2)direction * settings.movementSpeed;
        }
        else 
        {
            physicData.controlledVelocity = (float2)direction * settings.airMovementSpeed;
            physicData.velocity += (float2)direction * settings.airMovementSpeed;
        }

        if (isGrounded && InputCommand.IsButtonDown(KeyCode.Space))
        {
            physicData.velocity += new float2(0, settings.jumpForce);
        }


        HandlePhysic();
    }

    public void Equip(EquipableElement weapon)
    {
        if(currentEquip != null)
        {
            currentEquip.Unequip(position);
        }

        currentEquip = weapon;
    }

    NativeSprite GetNativeSprite()
    {
        SpriteEnum sprite = currentEquip != null ? SpriteEnum.astronaut_gun : SpriteEnum.astronaut;
        return SpriteRegistry.GetSprite(sprite);
    }

    public override void Dispose()
    {
        spriteAnimator.Dispose();
    }
}
