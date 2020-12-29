using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TimeGemElement : EquipableElement
{
    bool timeStopped;
    [SerializeField] PostProcessManager.ScreenFlashSettings onEnableFlash;
    [SerializeField] PostProcessManager.ScreenFlashSettings onDisableFlash;

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
        if(timeStopped)
        {
            //add post process?
        }
    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        timeStopped = !timeStopped;
        
        if(timeStopped)
        {
            Debug.Log("Time shall never be altered");
            PostProcessManager.EnqueueScreenFlash(in onEnableFlash, tickBlock.tick);
        }
        else
        {
            Debug.Log("Release time.. from its chains");
            PostProcessManager.EnqueueScreenFlash(in onDisableFlash, tickBlock.tick);
        }


        LevelContainer[] containers = FindObjectsOfType<LevelContainer>();
        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].updateSimulation = !timeStopped;
        }
    }
}
