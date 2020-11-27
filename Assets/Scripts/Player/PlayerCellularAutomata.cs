using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour, IRenderableAnimated
{
    public PhysicBound physicBound;
    public Texture2D collisionTexture;

    public int2 position;
    //float2 position;
    //public int2 SpritePosition => (int2)(position / GameManager.GridScale);


    InputCommand input = new InputCommand();

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


    public void Init(int2 startPosition, Map map)
    {
        //todo beautify this
        physicBound = new PhysicBound(collisionTexture);
        map.SetSpriteAtPosition(startPosition, ref physicBound);

        input.CreateInput(KeyCode.Space);
    }

    public void OnUpdate(Map map)
    {
        input.Update();

        int2 direction = input.direction;

        bool isGrounded = IsGrounded(map, position);
        if (input.IsButtonDown(KeyCode.Space) && (wasGrounded || isGrounded))
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

    public void OnEnd(Map map)
    {
        map.RemoveSpriteAtPosition(position, ref physicBound);
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

    public void Render(ref NativeArray<Color32> colorArray, int tick)
    {
        GridRenderer.ApplySprite(ref colorArray, SpriteRegistry.GetSprite(SpriteEnum.astronaut), position);
    }

    [BurstCompile]
    public struct HandlePlayerJob : IJob
    {
        public Map map;
        public PhysicBound physicBound;
        public int2 direction;
        public int jumpIndex;
        public NativeArray<int> jumpArray;

        public NativeReference<int2> positionRef;
        public void Execute()
        {
            int2 position = positionRef.Value;
            int2 nextPosition = position;

            if(jumpIndex < jumpArray.Length)
            {
                int jumpDirection = jumpArray[jumpIndex];
                if (jumpDirection == 1)
                {
                    nextPosition = map.Jump(ref physicBound, position);
                }
                else if(jumpDirection == -1)
                {
                    nextPosition = map.ApplyGravity(ref physicBound, position);
                }
            }
            else
            {
                nextPosition = map.ApplyGravity(ref physicBound, position);
            }

            nextPosition = map.HandlePhysics(ref physicBound, nextPosition, nextPosition + direction);
            if (math.any(position != nextPosition))
            {
                map.RemoveSpriteAtPosition(position, ref physicBound);
                map.SetPlayerAtPosition(nextPosition, ref physicBound);
            }
            positionRef.Value = nextPosition;
        }
    }
}
