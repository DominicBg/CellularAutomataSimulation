using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class LevelParticleRenderer : LevelElement
{
    public abstract void EmitParticle(int2 position, ref TickBlock tickBlock);

}
