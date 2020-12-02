using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerElement : PhysicObject
{
    public PlayerControlSettings settings;

    WeaponBaseElement currentWeapon;

    public override Bound GetBound()
    {
        return new Bound(position, GetNativeSprite().sizes);
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplySprite(ref outputcolor, GetNativeSprite(), position);

        DebugAllPhysicBound(ref outputcolor);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(currentWeapon != null && Input.GetMouseButton(0))
        {
            currentWeapon.UseWeapon(position);
        }

        int2 direction = InputCommand.Direction;

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

        if (InputCommand.IsButtonHeld(KeyCode.Space))
        {
            physicData.velocity += new float2(0, settings.jetpackForce);
        }


        HandlePhysic();

    }

    public void EquipWeapon(WeaponBaseElement weapon)
    {
        if(currentWeapon != null)
        {
            currentWeapon.UnequipWeapon(position);
        }

        currentWeapon = weapon;
    }

    NativeSprite GetNativeSprite()
    {
        SpriteEnum sprite = currentWeapon != null ? SpriteEnum.astronaut_gun : SpriteEnum.astronaut;
        return SpriteRegistry.GetSprite(sprite);
    }
   
}
