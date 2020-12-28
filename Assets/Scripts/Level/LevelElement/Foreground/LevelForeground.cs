using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class LevelForeground : MonoBehaviour
{
    //todo interface?
    public abstract void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 levelPosition);
}
