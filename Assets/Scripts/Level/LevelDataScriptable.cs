using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LevelDataScriptable : ScriptableObject
{
    [SerializeField] LevelData savedLevelData;
    [HideInInspector, SerializeField] ParticleType[] grid;

    public void SaveLevel(LevelData levelData)
    {
        this.savedLevelData = levelData;
        grid = ArrayHelper.GetArrayFromGrid(levelData.grid, levelData.sizes);
        Debug.Log($"Saved {levelData.levelName}");
    }

    public LevelData LoadLevel()
    {
        LevelData levelData = savedLevelData;
        levelData.grid = ArrayHelper.GetGridFromArray(grid, levelData.sizes);
        Debug.Log($"Loaded {levelData.levelName}");
        return levelData;
    }
}
