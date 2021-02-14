﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LevelSmokeParticleRenderer : LevelParticleRenderer
{
    [SerializeField] SmokeParticleSystemScriptable settings = default;
    SmokeParticleSystem smokeParticleSystem;

    public bool autoEmit = true;
    public float delayEmitParticle = 0.2f;

    private float currentDelay;
    public bool inBackground;

    public override void OnInit()
    {
        smokeParticleSystem = new SmokeParticleSystem();
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        smokeParticleSystem.Update(ref settings.settings,ref tickBlock);
        if(autoEmit)
        {
            currentDelay += GameManager.DeltaTime;
            if (currentDelay >= delayEmitParticle)
            {
                currentDelay -= delayEmitParticle;
                smokeParticleSystem.EmitParticle(position, ref settings.emitter, ref tickBlock);
            }
        }
    }

    public override void EmitParticle(int2 position, ref TickBlock tickBlock)
    {
        smokeParticleSystem.EmitParticle(position, ref settings.emitter, ref tickBlock);
    }

    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if(inBackground)
            smokeParticleSystem.Render(ref outputColor, BlendingMode.Transparency);
    }
    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (!inBackground)
            smokeParticleSystem.Render(ref outputColor, BlendingMode.Transparency);
    }

    public override void Dispose()
    {
        smokeParticleSystem.Dispose();
        base.Dispose();
    }

    public override Bound GetBound()
    {
        return new Bound(position, 1);
    }
}
