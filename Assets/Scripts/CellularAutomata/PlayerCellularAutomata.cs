using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour
{
    readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1)};
    readonly KeyCode[] inputs = new KeyCode[] { KeyCode.D, KeyCode.A, KeyCode.W, KeyCode.S };

    //public PixelSprite sprite;

    public void Init(ref PixelSprite sprite, Map map)
    {
        //sprite = new PixelSprite(position, baseSprite);
        map.SetSpriteAtPosition(sprite.position, ref sprite);
    }

    public void OnUpdate(ref PixelSprite sprite, Map map)
    {
        int2 previousPos = sprite.position;
        //sprite.position = map.ApplyGravity(ref sprite);
        int2 nextPosition = map.ApplyGravity(ref sprite);
        for (int i = 0; i < inputs.Length; i++)
        {
            if(Input.GetKey(inputs[i]))
            {
                int2 direction = directions[i];
                nextPosition = nextPosition + direction;
                if (IsPlayerInBound(ref sprite, map, nextPosition))
                {
                    nextPosition = map.HandlePhysics(ref sprite, sprite.position, nextPosition);
                }
            }
        }

        if (math.any(previousPos != nextPosition))
        { 
            map.SetSpriteAtPosition(nextPosition, ref sprite);
        }
    }


    bool IsPlayerInBound(ref PixelSprite sprite, Map map, int2 newPosition)
    {
        Bound newBound = sprite.MovingBound(newPosition);
        return map.InBound(newBound);
        //int2 topLeftCorner = newPosition;
        //int2 topRightCorner = newPosition + new int2(sprite.sizes.x, 0);
        //int2 bottomLeftCorner = newPosition + new int2(0, sprite.sizes.y); ;
        //int2 bottomRightCorner = newPosition + new int2(sprite.sizes.x, sprite.sizes.y);
        //return map.InBound(topLeftCorner) && map.InBound(topRightCorner) && map.InBound(bottomLeftCorner) && map.InBound(bottomRightCorner);
        //return map.InBound(sprite.TopLeft) && map.InBound(sprite.TopRight) && map.InBound(sprite.BottomLeft) && map.InBound(sprite.BottomRight);
    }
}
