using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GolemMaker : EquipableElement
{
    public GolemController controller;

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        controller.SummonGolem(position, new List<ParticleType>() { ParticleType.Sand });
    }
}
