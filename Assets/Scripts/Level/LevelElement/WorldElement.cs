using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class WorldObject : LevelObject
{
    public int2 currentLevel;

    public virtual void UpdateLevelMap(int2 newLevel, Map map, LevelContainer levelContainer)
    {
        this.map = map;
        this.levelContainer = levelContainer;
    }

    //only update if in current level
}
