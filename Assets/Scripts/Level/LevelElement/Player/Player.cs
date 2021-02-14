using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Player : PhysicObject, ILightSource
{
    public PlayerControlSettings settings;

    public SpriteAnimator spriteAnimator;


    public ReflectionInfo skyRefl = new ReflectionInfo() { amount = .2f, blending = BlendingMode.AdditiveAlpha, distance = 20 };
    public ReflectionInfo envRefl = new ReflectionInfo() { amount = .5f, blending = BlendingMode.AdditiveAlpha, distance = 20 };

    public float minLight = 0.5f;
    public int blurRadius = 1;
    public float blurIntensity = 1;

    [HideInInspector] public bool lookLeft;
    [HideInInspector] public bool isDirectionLocked;

    ItemInventory inventory;

    private int reflectionIndex;

    int lookDirection;
    bool canJump;
    float inAirDuration;
    float pressJumpBufferDuration;

    bool ghostMode;

    public override Bound GetBound()
    {
        return physicData.physicBound.GetCollisionBound(position);
    }

    public override void OnInit()
    {
        spriteAnimator = new SpriteAnimator(settings.spriteSheet);
        InitPhysicData(settings.collisionTexture);
        inventory = new ItemInventory();
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var sprite = spriteAnimator.GetCurrentSprite();
        var normals = spriteAnimator.GetCurrentNormals();
        GridRenderer.ApplyLitSprite(ref outputColors, sprite, normals, position, renderPos, info.lightSources, minLight);
        
        reflectionIndex = info.GetReflectionIndex();
        GridRenderer.PrepareSpriteEnvironementReflection(sprite, renderPos, ref info, reflectionIndex);
    }

    public override void RenderReflection(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var sprite = spriteAnimator.GetCurrentSprite();
        var normals = spriteAnimator.GetCurrentNormals();
        var reflections = spriteAnimator.GetCurrentReflections();
        GridRenderer.ApplySpriteSkyboxReflection(ref outputColors, sprite, normals, reflections, renderPos, ref info, ref skyRefl);
        GridRenderer.ApplySpriteEnvironementReflection(ref outputColors, sprite, normals, reflections, renderPos, reflectionIndex, ref info, ref envRefl, blurRadius, blurIntensity);
    }


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if (InputCommand.IsButtonDown(KeyCode.F3))
            ghostMode = !ghostMode;

        if(ghostMode)
        {
            physicData.position += (InputCommand.Direction * 360 * GameManager.DeltaTime);
            position = (int2)physicData.position;
            return;
        }


        DebugEquip();
        inventory.Update();

        float2 direction = new float2(InputCommand.Direction.x, 0);
        
        if(!isDirectionLocked)
            lookDirection = (int)math.sign(direction.x);

        if (lookDirection != 0)
        {
            lookLeft = lookDirection == -1;
        }


        bool isGrounded = IsGrounded();
    
        UpdateAnimation(direction, isGrounded);

        UpdateMovement(direction);
        UpdateJump(isGrounded);

        if(isGrounded && InputCommand.IsButtonHeld(KeyCode.Space))
        {
            PhysiXVII.MoveUpFromPile(ref physicData, map, GameManager.PhysiXVIISetings);
            position = physicData.gridPosition;
        }

        HandlePhysic();
    }

    void DebugEquip()
    {
        if (InputCommand.IsButtonDown(KeyCode.Alpha1))
            inventory.Equip(FindObjectOfType<Shovel>(), ItemInventory.Slot.Main);
        if (InputCommand.IsButtonDown(KeyCode.Alpha2))
            inventory.Equip(FindObjectOfType<ParticleBlower>(), ItemInventory.Slot.Main);
        if (InputCommand.IsButtonDown(KeyCode.Alpha3))
            inventory.Equip(FindObjectOfType<StatisGun>(), ItemInventory.Slot.Secondary);
    }

    private void UpdateAnimation(float2 direction, bool isGrounded)
    {
        if (!isGrounded)
        {
            spriteAnimator.SetAnimation(2);
        }
        else if (direction.x == 0)
        {
            spriteAnimator.SetAnimation(0);
        }
        else
        {
            spriteAnimator.SetAnimation(1);
        }
        spriteAnimator.Update(lookLeft);
    }

    private void UpdateMovement(float2 direction)
    {
        physicData.velocity.x += direction.x * settings.acceleration * GameManager.DeltaTime;
        float damping;
        if (math.abs(direction.x) < 0.01f)
            damping = settings.stopMovingDamping;
        else if (math.sign(direction.x) != math.sign(physicData.velocity.x))
            damping = settings.turiningDamping;
        else
            damping = settings.movingDamping;

        physicData.velocity.x *= math.pow(1 - damping, GameManager.DeltaTime * settings.dampingForce);
    }

    private void UpdateJump(bool isGrounded)
    {
        if (isGrounded)
        {
            canJump = true;
            inAirDuration = 0;
        }
        else
        {
            inAirDuration += GameManager.DeltaTime;

            //Control jump height
            if(physicData.velocity.y > 0 && InputCommand.IsButtonUp(KeyCode.Space))
            {
                physicData.velocity.y *= settings.releaseJumpButtonCutoff;
            }

        }

        pressJumpBufferDuration -= GameManager.DeltaTime;
        if (InputCommand.IsButtonDown(KeyCode.Space))
        {
            pressJumpBufferDuration = settings.pressJumpBuffer;
        }

        if (canJump && inAirDuration < settings.inAirJumpThreshold && pressJumpBufferDuration > 0)
        {
            physicData.velocity = new float2(physicData.velocity.x, settings.jumpForce);
            canJump = false;
            pressJumpBufferDuration = 0;
        }
    }

    public override void Dispose()
    {
        spriteAnimator.Dispose();
    }

    public LightSource GetLightSource(int tick)
    {
        return settings.lightSourceSettings.GetLightSource(GetBound().center, tick);
    }

    bool ILightSource.IsVisible() => isVisible;
}
