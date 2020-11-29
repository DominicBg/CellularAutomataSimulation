using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LevelContainer))]
public abstract class LevelElement : MonoBehaviour
{
    //References
    protected GameLevelManager gameLevelManager;
    protected Map map;

    [HideInInspector] public bool isEnable = true;
    [HideInInspector] public bool isVisible = true;


    public virtual void Init(GameLevelManager gameLevelManager, Map map)
    {
        this.gameLevelManager = gameLevelManager;
        this.map = map;
    }

    public abstract void OnUpdate(ref TickBlock tickBlock);
    public abstract void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock);
}
