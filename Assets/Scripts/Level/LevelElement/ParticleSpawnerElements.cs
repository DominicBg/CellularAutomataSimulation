using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleSpawnerElements : LevelObject
{
    [Range(0, 1)] public float chanceSpawn;
    public ParticleType particleType;
    public int particleSpawnCount;

    public override Bound GetBound()
    {
        return new Bound(position, 1);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        bool canEmit = particleSpawnCount != 0;
        if (canEmit && tickBlock.random.NextFloat() <= chanceSpawn && map.IsFreePosition(position))
        {
            particleSpawnCount--;
            map.SetParticleType(position, particleType);
        }
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        int index = ArrayHelper.PosToIndex(renderPos, GameManager.RenderSizes);
        outputColors[index] = Color.white;
    }
}
