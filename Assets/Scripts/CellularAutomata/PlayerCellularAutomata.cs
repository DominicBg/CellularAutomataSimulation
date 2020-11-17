using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour
{
    readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1) };
    readonly KeyCode[] inputs = new KeyCode[] { KeyCode.D, KeyCode.A, KeyCode.W, KeyCode.S };
    public PhysicBound physicBound;
    public Texture2D collisionTexture;

    public int jumpHeight = 3;

    int jumpLeft = 0;

    public void Init(ref PixelSprite sprite, Map map)
    {
        //todo beautify this
        //physicBound = new PhysicBound(new Bound(new int2(1, 0), new int2(7, 8)));
        physicBound = new PhysicBound(collisionTexture);
        map.SetSpriteAtPosition(sprite.position, ref sprite);
    }

    public void OnUpdate(ref PixelSprite sprite, Map map)
    {
        int2 direction = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (Input.GetKey(inputs[i]))
            {
                direction = directions[i];
                break;
            }
        }
        if(Input.GetKey(KeyCode.Space) && IsGrounded(map, sprite.position))
        {
            jumpLeft = jumpHeight;
        }
        else
        {
            jumpLeft = math.max(jumpLeft - 1, 0);
        }

        NativeReference<int2> positionOutput = new NativeReference<int2>(Allocator.TempJob);
        new HandlePlayerJob()
        {
            direction = direction,
            sprite = sprite,
            map = map,
            physicBound = physicBound,
            jumpLeft = jumpLeft,
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

    public struct HandlePlayerJob : IJob
    {
        public PixelSprite sprite;
        public Map map;
        public PhysicBound physicBound;
        public int2 direction;
        public int jumpLeft;

        public NativeReference<int2> output;
        public void Execute()
        {
            int2 previousPos = sprite.position;
            int2 nextPosition;

            if (jumpLeft > 0)
            {
                nextPosition = map.Jump(ref physicBound, sprite.position);
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
