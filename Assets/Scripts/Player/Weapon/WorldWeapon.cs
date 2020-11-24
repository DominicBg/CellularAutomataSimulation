using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class WorldWeapon : ScriptableObject
{
    public int2 worldPosition;
    public Texture2D texture;
    public PixelSprite pixelSprite;

    public void Init()
    {
        pixelSprite = new PixelSprite(worldPosition, texture);
    }
    public void Dispose()
    {
        pixelSprite.Dispose();
    }

    public abstract WeaponBase GetWeapon();
}
