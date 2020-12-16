using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LevelContainer))]
public abstract class LevelObject : LevelElement
{
    public int2 position;

    public abstract Bound GetBound();
}
