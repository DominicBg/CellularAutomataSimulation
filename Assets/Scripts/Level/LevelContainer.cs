using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class LevelContainer : MonoBehaviour
{
    [SerializeField] LevelElement[] levelElements;
    [SerializeField] ParticleType[] serializedParticles;

    GameLevelManager gameLevelManager;
    Map map;

    public void OnValidate()
    {
        levelElements = GetComponents<LevelElement>();
    }

    public void Init(GameLevelManager gameLevelManager, Map map)
    {
        this.gameLevelManager = gameLevelManager;
        this.map = map;

        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].Init(gameLevelManager, map);
        }
    }

    public void OnUpdate(ref TickBlock tickBlock)
    {
        gameLevelManager.UpdateSimulation();
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].OnUpdate(ref tickBlock);
        }
    }
    public void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].OnRender(ref outputcolor, ref tickBlock);
        }
    }
}
