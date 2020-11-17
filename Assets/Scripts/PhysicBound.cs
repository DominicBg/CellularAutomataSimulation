using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct PhysicBound
{
    public Bound localCollisionBound;

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
    public Bound GetFeetCollisionBound(int2 position)
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
