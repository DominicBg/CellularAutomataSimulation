using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct PhysicBound
{
    [System.Flags]
    public enum BoundFlag
    {
        Feet = 1 << 0,
        UnderFeet = 1 << 1,
        Top = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
        All = 1 << 5
    }

    public Bound localCollisionBound;

    public PhysicBound(Texture2D collisionTexture)
    {
        int width = collisionTexture.width;
        int height = collisionTexture.height;

        int2 min = new int2(width, height);
        int2 max = new int2(0, 0);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(collisionTexture.GetPixel(x, y).a > 0.5f)
                {
                    min.x = math.min(min.x, x);
                    min.y = math.min(min.y, y);
                    max.x = math.max(max.x, x);
                    max.y = math.max(max.y, y);
                }
            }
        }
        Debug.Log($"min {min}, max {max}");
        localCollisionBound = new Bound(min, max - min + 1);
    }
    public PhysicBound(Bound localCollisionBound)
    {
        this.localCollisionBound = localCollisionBound;
    }

    public Bound GetCollisionBound(int2 position)
    {
        return new Bound(position + localCollisionBound.position, localCollisionBound.sizes);
    }

    public Bound GetTopCollisionBound(int2 position)
    {
        Bound globalBound = GetCollisionBound(position);
        return new Bound(globalBound.topLeft, new int2(globalBound.sizes.x, 1));
    }
    public Bound GetBottomCollisionBound(int2 position)
    {
        Bound globalBound = GetCollisionBound(position);
        return new Bound(globalBound.bottomLeft, new int2(globalBound.sizes.x, 1));
    }

    public Bound GetUnderFeetCollisionBound(int2 position)
    {
        Bound globalBound = GetCollisionBound(position);
        return new Bound(globalBound.bottomLeft + new int2(0, -1), new int2(globalBound.sizes.x, 1));
    }

    public Bound GetLeftCollisionBound(int2 position)
    {
        Bound globalBound = GetCollisionBound(position);
        return new Bound(globalBound.bottomLeft, new int2(1, globalBound.sizes.y));
    }
    public Bound GetRightCollisionBound(int2 position)
    {
        Bound globalBound = GetCollisionBound(position);
        return new Bound(globalBound.bottomRight, new int2(1, globalBound.sizes.y));
    }

}
