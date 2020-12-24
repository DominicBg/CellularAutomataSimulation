using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[CreateAssetMenu(fileName = "DiamondTransition", menuName = "Transition/DiamondTransition", order = 1)]
public class DiamondTransition : TransitionBase
{
    [SerializeField] int diamondSize;
    [SerializeField] Color32 color;
    //add ease

    public override void Transition(ref NativeArray<Color32> outputColors, ref NativeArray<Color32> firstImage, ref NativeArray<Color32> secondImage, float t)
    {
        //t = ease(t);
        new DiamondTransitionJob()
        {
            color = color,
            diamondSize = diamondSize,
            firstImage = firstImage,
            secondImage = secondImage,
            outputColors = outputColors,
            t = t
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
}
