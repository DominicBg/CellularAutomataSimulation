using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseSettings", menuName = "Enemies/EnemyBaseSettings", order = 1)]
public class EnemyBaseSettings : ScriptableObject
{
    public float movementSpeed;

    public int maxHp;

    public SpriteSheet spriteSheet;
    public int2 sizes;
    public float aggroRange;
}
