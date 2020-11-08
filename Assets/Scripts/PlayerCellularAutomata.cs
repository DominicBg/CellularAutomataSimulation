using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCellularAutomata : MonoBehaviour
{
    readonly int2[] directions = new int2[] { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1)};
    readonly KeyCode[] inputs = new KeyCode[] { KeyCode.D, KeyCode.A, KeyCode.W, KeyCode.S };

    public PixelSprite sprite;

    [SerializeField] Texture2D baseSprite;

    public void Init(int2 position, NativeArray<Particle> particles, Map map)
    {
        sprite = new PixelSprite(position, baseSprite);
        map.SetSpriteAtPosition(particles, sprite.position, position, ref sprite);
    }

    public bool TryUpdate(NativeArray<Particle> particles, Map map)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            if(Input.GetKey(inputs[i]))
            {
                int2 direction = directions[i];
                int2 previousPos = sprite.position;
                MovePlayer(map, direction);
                map.SetSpriteAtPosition(particles, previousPos, sprite.position, ref sprite);
                return true;
            }
        }

        return false;
    }

    void MovePlayer(Map map, int2 direction)
    {
        int2 newPosition = sprite.position + direction;
        if(IsPlayerInBound(map, newPosition))
        {
            sprite.position = newPosition;
        }
    }

    bool IsPlayerInBound(Map map, int2 newPosition)
    {
        int2 topLeftCorner = newPosition;
        int2 topRightCorner = newPosition + new int2(sprite.sizes.x, 0);
        int2 bottomLeftCorner = newPosition + new int2(0, sprite.sizes.y); ;
        int2 bottomRightCorner = newPosition + new int2(sprite.sizes.x, sprite.sizes.y);
        return map.InBound(topLeftCorner) && map.InBound(topRightCorner) && map.InBound(bottomLeftCorner) && map.InBound(bottomRightCorner);
    }
}
