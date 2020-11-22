using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour
{

    public PhysicBound physicBound;
    public Texture2D collisionTexture;

    InputCommand input = new InputCommand();

    //int[] jumpHeight = {
    //    1, 1, 1,
    //    1, 0, 1, 0, 1, 0,
    //    1, 0, 0, 1, 0, 0,
    //    -1, 0, 0, -1, 0, 0,
    //    -1, 0, -1, 0, -1, 0,
    //    -1, -1, -1
    //    };

    int[] jumpHeight = {
        1, 1,
        1, 0, 1, 0, 
        1, 0, 0, 
        -1, 0, 0, 
        -1, 0, -1, 0, 
        -1, -1
        };

    int jumpIndex = 0;
    bool wasGrounded;


    public void Init(ref PixelSprite sprite, Map map)
    {
        //todo beautify this
        physicBound = new PhysicBound(collisionTexture);
        map.SetSpriteAtPosition(sprite.position, ref sprite, ref physicBound);

        input.CreateInput(KeyCode.Space);
    }

    public void OnUpdate(ref PixelSprite sprite, Map map)
    {
        input.Update();

        int2 direction = input.direction;

        bool isGrounded = IsGrounded(map, sprite.position);
        if (input.IsButtonDown(KeyCode.Space) && (wasGrounded || isGrounded))
        {
            jumpIndex = 0;
        }
        else
        {
            jumpIndex++;
        }
        wasGrounded = isGrounded;

        NativeReference<int2> positionOutput = new NativeReference<int2>(Allocator.TempJob);
        var jumpArray = new NativeArray<int>(jumpHeight, Allocator.TempJob);
        new HandlePlayerJob()
        {
            direction = direction,
            sprite = sprite,
            map = map,
            physicBound = physicBound,
            jumpIndex = jumpIndex,
            jumpArray = jumpArray,
            output = positionOutput
        }.Run();

        sprite.position = positionOutput.Value;
        jumpArray.Dispose();
        positionOutput.Dispose();
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


    [BurstCompile]
    public struct HandlePlayerJob : IJob
    {
        public PixelSprite sprite;
        public Map map;
        public PhysicBound physicBound;
        public int2 direction;
        public int jumpIndex;
        public NativeArray<int> jumpArray;

        public NativeReference<int2> output;
        public void Execute()
        {
            int2 previousPos = sprite.position;
            int2 nextPosition = previousPos;

            if(jumpIndex < jumpArray.Length)
            {
                int jumpDirection = jumpArray[jumpIndex];
                if (jumpDirection == 1)
                {
                    nextPosition = map.Jump(ref physicBound, sprite.position);
                }
                else if(jumpDirection == -1)
                {
                    nextPosition = map.ApplyGravity(ref physicBound, sprite.position);
                }
            }
            else
            {
                nextPosition = map.ApplyGravity(ref physicBound, sprite.position);
            }

            nextPosition = map.HandlePhysics(ref physicBound, nextPosition, nextPosition + direction);
            if (math.any(previousPos != nextPosition))
            {
                map.RemoveSpriteAtPosition(ref sprite, ref physicBound);
                map.SetSpriteAtPosition(nextPosition, ref sprite, ref physicBound);
            }
            output.Value = sprite.position;
        }
    }
}
