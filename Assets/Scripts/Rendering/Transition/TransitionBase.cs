using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class TransitionBase : ScriptableObject
{
    public abstract void Transition(ref NativeArray<Color32> outputColors, ref NativeArray<Color32> firstImage, ref NativeArray<Color32> secondImage, float t);
}
