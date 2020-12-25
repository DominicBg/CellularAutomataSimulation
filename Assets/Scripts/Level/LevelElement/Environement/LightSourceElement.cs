﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LightSourceElement : LevelElement, ILightSource
{
    [SerializeField] LevelObject target;
    [SerializeField] LightSourceScriptable lightSource;


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        //
    }

    public LightSource GetLightSource(out int2 position)
    {
        position = target.GetBound().center;

        return lightSource.lightSource;
    }
}
