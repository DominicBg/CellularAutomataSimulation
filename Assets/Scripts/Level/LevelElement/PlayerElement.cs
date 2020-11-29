﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerElement : LevelObject
{
    int jumpIndex = 0;
    bool wasGrounded;

    WeaponBaseElement currentWeapon;

    PhysicBound physicBound; //use physics object
    public Texture2D collisionTexture;

    int[] jumpHeight = {
        1, 1,
        1, 0, 1, 0,
        1, 0, 0,
        -1, 0, 0,
        -1, 0, -1, 0,
        -1, -1
        };

    public override void Init(GameLevelManager gameLevelManager, Map map)
    {
        base.Init(gameLevelManager, map);
        physicBound = new PhysicBound(collisionTexture);
    }

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

        bool isGrounded = IsGrounded(map, position);
        if (InputCommand.IsButtonDown(KeyCode.Space) && (wasGrounded || isGrounded))
        {
            jumpIndex = 0;
        }
        else
        {
            jumpIndex++;
        }
        wasGrounded = isGrounded;

        NativeReference<int2> positionRef = new NativeReference<int2>(Allocator.TempJob);
        positionRef.Value = position;

        var jumpArray = new NativeArray<int>(jumpHeight, Allocator.TempJob);
        new HandlePlayerJob()
        {
            direction = direction,
            map = map,
            physicBound = physicBound,
            jumpIndex = jumpIndex,
            jumpArray = jumpArray,
            positionRef = positionRef
        }.Run();

        position = positionRef.Value;
        jumpArray.Dispose();
        positionRef.Dispose();
    }


    public bool IsGrounded(Map map, int2 position)
    {
        Bound feetBound = physicBound.GetFeetCollisionBound(position);
        Bound underFeetBound = physicBound.GetUnderFeetCollisionBound(position);
        bool hasFeetCollision = map.HasCollision(ref feetBound);
        bool hasUnderFeetCollision = map.HasCollision(ref underFeetBound);
        bool atFloorLevel = position.y == 0;
        return hasFeetCollision || hasUnderFeetCollision || atFloorLevel;
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

    //DEBUG
    [Header("Debug")]
    public PhysicBound.BoundFlag debugBoundFlag;
    void DebugAllPhysicBound(ref NativeArray<Color32> outputColor)
    {
        PhysicBound physicbound = physicBound;

        if ((debugBoundFlag & PhysicBound.BoundFlag.All) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetCollisionBound(position), Color.magenta);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Feet) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetFeetCollisionBound(position), Color.yellow);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Left) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetLeftCollisionBound(position), Color.red);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Right) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetRightCollisionBound(position), Color.blue);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Top) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetTopCollisionBound(position), Color.cyan);
    }

    void DebugPhysicBound(ref NativeArray<Color32> outputColor, Bound bound, Color color)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }
        GridRenderer.ApplyPixels(ref outputColor, ref positions, ref colors);
        positions.Dispose();
        colors.Dispose();
    }
}
