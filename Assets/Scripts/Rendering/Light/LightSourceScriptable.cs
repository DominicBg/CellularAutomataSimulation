using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class LightSourceScriptable : ScriptableObject
{
    public abstract LightSource GetLightSource(int2 position, int tick);
}
