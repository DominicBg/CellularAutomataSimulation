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
    public int combustionParticleCount = 5;
    public int comubstionRandomRange = 5;

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
        Unity.Mathematics.Random rng = new Unity.Mathematics.Random(tickBlock.tickSeed);
        for (int i = 0; i < particleEvents.combustionEvents.Length; i++)
        {
            int2 position = particleEvents.combustionEvents[i];
            for (int j = 0; j < combustionParticleCount; j++)
            {
                combustionParticleSystem.EmitParticleAtPosition(position + rng.NextInt2(0, comubstionRandomRange+1), ref tickBlock);
            }
        }
        combustionParticleSystem.Update(ref tickBlock);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
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