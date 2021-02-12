using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "StatisGunScriptable", menuName = "Equipable/StatisGunScriptable", order = 1)]
public class StatisGunScriptable : BaseGunScriptable
{
    public int2 boundSizes = new int2(100, 50);
    public float statisDuration = 3;
    public int2 beamOffset;
    public Color tint = Color.white;
    public Texture2D beamTexture;
}

