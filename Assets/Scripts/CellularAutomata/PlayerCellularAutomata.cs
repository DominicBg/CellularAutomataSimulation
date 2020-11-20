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

    InputCommand input;

    int[] jumpHeight = {
        1, 1, 1,
        1, 0, 1, 0, 1, 0,
        1, 0, 0, 1, 0, 0,
        -1, 0, 0, -1, 0, 0,
        -1, 0, -1, 0, -1, 0,
        -1, -1, -1
        };

    int jumpIndex = 0;
    bool wasGrounded;

    NativeArray<int> nativeJumpArray;

    private void OnDestroy()
    {
        nativeJumpArray.Dispose();
    }

    public void Init(ref PixelSprite sprite, Map map)
    {
        nativeJumpArray = new NativeArray<int>(jumpHeight, Allocator.Persistent);
        //todo beautify this
        //physicBound = new PhysicBound(new Bound(new int2(1, 0), new int2(7, 8)));
        physicBound = new PhysicBound(collisionTexture);
        map.SetSpriteAtPosition(sprite.position, ref sprite);
    }

    public void OnUpdate(ref PixelSprite sprite, Map map)
    {
        input.Update();

        int2 direction = input.direction;

        bool isGrounded = IsGrounded(map, sprite.position);
        if (input.spaceInput.IsButtonDown() && (wasGrounded || isGrounded))
        {
            jumpIndex = 0;
        }
        else
        {
            jumpIndex++;
        }
        wasGrounded = isGrounded;

        NativeReference<int2> positionOutput = new NativeReference<int2>(Allocator.TempJob);
        new HandlePlayerJob()
        {
            direction = direction,
            sprite = sprite,
            map = map,
            physicBound = physicBound,
            jumpIndex = jumpIndex,
            jumpArray = nativeJumpArray,
            output = positionOutput
        }.Run();
        sprite.position = positionOutput.Value;
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
                map.SetSpriteAtPosition(nextPosition, ref sprite);
            }
            output.Value = sprite.position;
        }
    }
}
