using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LevelParticleEffectSystem : LevelObject
{
    [SerializeField] ParticleEffectSystemScriptable settings = default;
    ParticleEffectSystem particleEffectSystem;

    public bool autoEmit = true;
    public float delayEmitParticle = 0.2f;

    private float currentDelay;
    public bool inBackground;

    public override void OnInit()
    {
        particleEffectSystem = new ParticleEffectSystem(settings);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        particleEffectSystem.Update(ref tickBlock);
        if(autoEmit)
        {
            currentDelay += GameManager.DeltaTime;
            if (currentDelay >= delayEmitParticle)
            {
                currentDelay -= delayEmitParticle;
                EmitParticle(ref tickBlock);
            }
        }
    }

    public void EmitParticle(ref TickBlock tickBlock)
    {
        particleEffectSystem.EmitParticle(position, ref tickBlock);
    }
    public void EmitParticleAtPosition(int2 position, ref TickBlock tickBlock)
    {
        particleEffectSystem.EmitParticleAtPosition(position, ref tickBlock);
    }

    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        if(inBackground)
            particleEffectSystem.Render(ref outputColor, pixelCamera);
    }
    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        if (!inBackground)
            particleEffectSystem.Render(ref outputColor, pixelCamera);
    }
    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(position, settings.settings.emitter.emitterSizes), pixelCamera.position, Color.red * .25f);
    }


    public override void Dispose()
    {
        particleEffectSystem.Dispose();
        base.Dispose();
    }

    public override Bound GetBound()
    {
        int2 emitterSize = settings.settings.emitter.emitterSizes;
        return math.any(emitterSize > particleEffectSystem.bound.sizes) ? Bound.CenterAligned(position, emitterSize) : particleEffectSystem.bound;
    }
}
