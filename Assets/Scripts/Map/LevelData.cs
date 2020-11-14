using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct LevelData
{
    public string levelName;
    public int2 playerPosition;
    public Texture2D playerTexture;
    public int2 sizes;
    public ParticleSpawner[] particleSpawners;

    //Non serialized by default
    public ParticleType[,] grid;
}
