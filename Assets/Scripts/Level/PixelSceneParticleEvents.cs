using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PixelSceneParticleEvents : LevelElement, IAlwaysRenderable
{
    [SerializeField] ParticleEffectSystemScriptable combustionSettings = default;
    ParticleEffectSystem combustionParticleSystem;

    public ParticleEvents particleEvents;

    public override void OnInit()
    {
        combustionParticleSystem = new ParticleEffectSystem(combustionSettings);
        particleEvents = new ParticleEvents();
        particleEvents.Init();
    }
    public override void Dispose()
    {
        combustionParticleSystem.Dispose();
        particleEvents.Dispose();
    }

    public void UpdateParticleEvents(ref TickBlock tickBlock)
    {
        for (int i = 0; i < particleEvents.combustionEvents.Length; i++)
        {
            combustionParticleSystem.EmitParticleAtPosition(particleEvents.combustionEvents[i], ref tickBlock);
        }
        combustionParticleSystem.Update(ref tickBlock);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        combustionParticleSystem.Render(ref outputColors, pixelCamera);
    }

    //public override Bound GetBound()
    //{
    //    return combustionParticleSystem.bound;
    //}

    public struct ParticleEvents
    {
        public NativeList<int2> combustionEvents;
        //Add other events

        public void Init()
        {
            combustionEvents = new NativeList<int2>(25, Allocator.Persistent);
        }
        public void Dispose()
        {
            combustionEvents.Dispose();
        }
    }
}