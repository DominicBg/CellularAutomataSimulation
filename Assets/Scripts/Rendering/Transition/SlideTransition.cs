using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SlideTransition", menuName = "Transition/SlideTransition", order = 1)]
public class SlideTransition : TransitionBase
{
    public enum Direction { LeftToRight, RightToLeft, UpToDown, DownToUp }
    public Direction direction;


    public override void Transition(ref NativeArray<Color32> outputColors, ref NativeArray<Color32> firstImage, ref NativeArray<Color32> secondImage, float t)
    {
        bool isHorizontal = direction == Direction.LeftToRight || direction == Direction.RightToLeft;
        bool isInverted = direction == Direction.RightToLeft || direction == Direction.DownToUp;
        t = t * t;
        t = (isInverted) ? 1 - t : t;
        new SlideTransitionJob()
        {
            firstImage = !isInverted ? firstImage : secondImage,
            secondImage = !isInverted ? secondImage : firstImage,
            outputColors = outputColors,
            isHorizontal = isHorizontal,
            t = t
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
}
