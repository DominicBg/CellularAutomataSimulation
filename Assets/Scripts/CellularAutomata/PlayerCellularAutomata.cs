using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour
{
    readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1)};
    readonly KeyCode[] inputs = new KeyCode[] { KeyCode.D, KeyCode.A, KeyCode.W, KeyCode.S };
    public PhysicBound physicBound;

    public void Init(ref PixelSprite sprite, Map map)
    {
        //todo beautify this
        physicBound = new PhysicBound(new Bound(new int2(1, 0), new int2(8, 9)));
        map.SetSpriteAtPosition(sprite.position, ref sprite);
    }

    public void OnUpdate(ref PixelSprite sprite, Map map)
    {
        int2 previousPos = sprite.position;
        int2 nextPosition = map.ApplyGravity(sprite.position, ref physicBound);
        for (int i = 0; i < inputs.Length; i++)
        {
            if(Input.GetKey(inputs[i]))
            {
                int2 direction = directions[i];
                nextPosition = map.HandlePhysics(ref physicBound, sprite.position, nextPosition + direction);
            }
        }

        if (math.any(previousPos != nextPosition))
        { 
            map.SetSpriteAtPosition(nextPosition, ref sprite);
        }
    }
}
