using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LightSourceElement : LevelElement, ILightSource
{
    [SerializeField] int2 position;
    [SerializeField] LevelObject target = default;
    [SerializeField] LightSourceScriptable lightSource = default;

    [SerializeField] bool useTarget;

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        //
    }


    public LightSource GetLightSource(int tick)
    {
        int2 pos = useTarget ? target.GetBound().center : position;
        return lightSource.GetLightSource(pos, tick);
    }
}
