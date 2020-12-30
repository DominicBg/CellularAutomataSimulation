using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    public override void RenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        int index = ArrayHelper.PosToIndex(position, GameManager.GridSizes);
        outputColor[index] = Color.red;
    }
}
