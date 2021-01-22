using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleDestroyerElement : LevelObject
{
    public override Bound GetBound()
    {
        return new Bound(position, 1);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        map.SetParticleType(position, ParticleType.None);
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        int index = ArrayHelper.PosToIndex(renderPos, GameManager.GridSizes);
        outputColor[index] = Color.red;
    }
}
