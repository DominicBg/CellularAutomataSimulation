using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GolemMaker : EquipableElement
{
    public GolemController controller;

    [SerializeField] Explosive.Settings explosionSettings;
    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        if (!controller.isSummoned)
        {
            List<ParticleType> particles = new List<ParticleType>();
            for (int i = 0; i < 150; i++)
            {
                particles.Add(ParticleType.Sand);
            }
            controller.SummonGolem(player.position + new int2(0 ,5), particles);
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
