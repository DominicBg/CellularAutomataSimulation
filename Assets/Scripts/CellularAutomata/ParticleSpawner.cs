using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ParticleSpawner
{
    [Range(0, 1)] public float chanceSpawn;
    public int2 spawnPosition;
    public ParticleType particleType;
    public int particleSpawnCount;
}
