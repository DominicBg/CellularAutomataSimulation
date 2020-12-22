using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class LevelObject : LevelElement
{
    public int2 position;

    public abstract Bound GetBound();
}
