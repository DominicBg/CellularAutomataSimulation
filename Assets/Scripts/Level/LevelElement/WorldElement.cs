using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class WorldElement : LevelElement
{
    public int2 currentLevel;

    //only update if in current level
}
