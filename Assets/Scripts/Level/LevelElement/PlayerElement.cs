﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static FogElement;

public class PlayerElement : PhysicObject, ILightSource
{
    public PlayerControlSettings settings;

    public SpriteAnimator spriteAnimator;
    [HideInInspector] public EquipableElement currentEquip;
    int lookDirection;
    public bool lookLeft;

    public Explosive.Settings explosiveSettings;
    public PostProcessManager.ShakeSettings shakeSettings;
    public PostProcessManager.ScreenFlashSettings flashSettings;
    public ShockwaveSettings shockwaveSettings;
    public BlackholeSettings blackholeSettings;
    public IllusionEffectSettings illusionEffect;

    public override Bound GetBound()
    {
        return physicData.physicBound.GetCollisionBound(position);
    }

    public override void Init(Map map)
    {
        base.Init(map);
        spriteAnimator = new SpriteAnimator(settings.spriteSheet);
        IniPhysicData(settings.collisionTexture);
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
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
        if(currentEquip != null && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            bool isAltButton = Input.GetMouseButton(1);
            currentEquip.Use(position, isAltButton);
        }

        int2 direction = new int2(InputCommand.Direction.x, 0);
        lookDirection = direction.x;

        bool isGrounded = IsGrounded();

        if(!isGrounded)
        {
            spriteAnimator.SetAnimation(2);
        }
        else  if (direction.x == 0)
        {
            spriteAnimator.SetAnimation(0);
        }
        else
        {
            spriteAnimator.SetAnimation(1);
        }
        spriteAnimator.Update();

        if (isGrounded)
        {
            physicData.controlledVelocity = (float2)direction * settings.movementSpeed * GameManager.DeltaTime;
        }
        else 
        {
            physicData.controlledVelocity = (float2)direction * settings.airMovementSpeed * GameManager.DeltaTime;
            physicData.velocity += (float2)direction * settings.airMovementSpeed * GameManager.DeltaTime;
        }

        if (isGrounded && InputCommand.IsButtonDown(KeyCode.Space))
        {
            if (physicData.velocity.y < 0)
                physicData.velocity.y = 0;

            physicData.velocity += new float2(0, settings.jumpForce);
        }

        if (InputCommand.IsButtonDown(KeyCode.X))
            Explosive.SetExplosive(GridPicker.GetGridPosition(), ref explosiveSettings, map);
        if (InputCommand.IsButtonDown(KeyCode.V))
            PostProcessManager.EnqueueScreenFlash(in flashSettings, tickBlock.tick);
        if (InputCommand.IsButtonDown(KeyCode.C))
            PostProcessManager.EnqueueShake(in shakeSettings, tickBlock.tick);
        if (InputCommand.IsButtonDown(KeyCode.B))
        {
            shockwaveSettings.centerPoint = GridPicker.GetGridPosition();
            PostProcessManager.EnqueueShockwave(in shockwaveSettings, tickBlock.tick);
        }
        if (InputCommand.IsButtonDown(KeyCode.N))
        {
            blackholeSettings.centerPoint = GridPicker.GetGridPosition();
            PostProcessManager.EnqueueBlackHole(in blackholeSettings, tickBlock.tick);
        }
        if (InputCommand.IsButtonDown(KeyCode.M))
        {
            PostProcessManager.EnqueuIllusion(in illusionEffect, tickBlock.tick);
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

    public override void Dispose()
    {
        spriteAnimator.Dispose();
    }

    public LightSource GetLightSource(out int2 position)
    {
        position = GetBound().center;
        return settings.lightSourceSettings.lightSource;
    }
}
