using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShuttleElement : GoalElement
{
    public override void UpdateCompleteStage()
    {
        position += new int2(0, 1);
    }

}
