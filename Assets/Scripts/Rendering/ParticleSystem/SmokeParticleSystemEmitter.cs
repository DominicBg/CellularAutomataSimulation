using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SmokeParticleSystemEmitter
{
    public Color32 minStartColor;
    public Color32 maxStartColor;
    public Color32 minEndColor;
    public Color32 maxEndColor;
    public int2 minOffset;
    public int2 maxOffset;
    public float minDuration;
    public float maxDuration;

    public SmokeParticle GetSmokeParticle(int2 position, ref TickBlock tickBlock)
    {
        SmokeParticle particle = new SmokeParticle()
        {
            duration = tickBlock.random.NextFloat(minDuration, maxDuration),
            position = position + tickBlock.random.NextInt2(minOffset, maxOffset),
            startColor = Color32.Lerp(minStartColor, maxStartColor, tickBlock.random.NextFloat()),
            endColor = Color32.Lerp(maxStartColor, maxEndColor, tickBlock.random.NextFloat()),
            startTick = tickBlock.tick
        };
        return particle;
    }
}
