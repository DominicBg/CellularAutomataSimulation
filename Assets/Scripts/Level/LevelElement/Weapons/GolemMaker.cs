using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GolemMaker : EquipableElement
{
    public GolemController controller;
    public ParticleType golemType;
    public int2 boundSizes = new int2(9, 10);
    [SerializeField] Explosive.Settings explosionSettings;
    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        if (!controller.isSummoned)
        {
            List<ParticleType> particles = new List<ParticleType>();
            for (int i = 0; i < boundSizes.x * boundSizes.y; i++)
            {
                particles.Add(golemType);
            }
            controller.SummonGolem(player.position + new int2(0 ,5), particles, boundSizes);
        }
        else
        {
            if (altButton)
            {
                controller.ExploseGolem(in explosionSettings);
            }
            else //!altButton
            {
                controller.ToggleControls();
            }
        }
    }
}
