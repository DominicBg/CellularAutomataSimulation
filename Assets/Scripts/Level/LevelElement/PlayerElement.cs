using System.Collections;
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
    [HideInInspector] public EquipableElement currentEquipMouse;
    [HideInInspector] public EquipableElement currentEquipQ;

    int lookDirection;
    public bool lookLeft;


    public override Bound GetBound()
    {
        return physicData.physicBound.GetCollisionBound(position);
    }

    public override void OnInit()
    {
        spriteAnimator = new SpriteAnimator(settings.spriteSheet);
        IniPhysicData(settings.collisionTexture);
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        if(lookDirection != 0)
        {
            lookLeft = lookDirection == -1;
        }

        spriteAnimator.Render(ref outputcolor, position, lookLeft);
        DebugAllPhysicBound(ref outputcolor);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(currentEquipMouse != null && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            bool isAltButton = Input.GetMouseButton(1);
            currentEquipMouse.Use(position, isAltButton);
        }
        if (currentEquipQ != null && InputCommand.IsButtonDown(KeyCode.Q))
        {
            currentEquipQ.Use(position, false);
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
        HandlePhysic();
    }

    public void EquipMouse(EquipableElement equipable)
    {
        if(currentEquipMouse != null)
        {
            currentEquipMouse.Unequip(position);
        }

        currentEquipMouse = equipable;
    }
    public void EquipQ(EquipableElement equipable)
    {
        if (currentEquipQ != null)
        {
            currentEquipQ.Unequip(position);
        }

        currentEquipQ = equipable;
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

    public override void UpdateLevelMap(int2 newLevel, Map map, LevelContainer levelContainer)
    {
        base.UpdateLevelMap(newLevel, map, levelContainer);
        currentLevel = newLevel;
    }
}
