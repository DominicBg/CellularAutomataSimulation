using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class WorldObject : LevelObject
{
    public int2 currentLevel;

    public virtual void UpdateLevelMap(int2 newLevel, Map map)
    {
        this.map = map;
    }

    //only update if in current level
}
