using FiniteStateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//to delete?
public abstract class LevelBase
{
    protected GameLevelManager gameLevelManager;
    protected Map map;
    protected PlayerCellularAutomata player;
    protected TickBlock tickBlock;


    public virtual void Init(GameLevelManager gameLevelManager, Map map, PlayerCellularAutomata player)
    {
        this.gameLevelManager = gameLevelManager;
        this.map = map;
        this.player = player;
        tickBlock.Init();
    }

    public abstract void Update();
    public abstract void OnRender();
}

