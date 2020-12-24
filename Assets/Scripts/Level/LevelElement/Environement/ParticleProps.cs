using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleProps : LevelObject
{
    [SerializeField] ParticleType particleType;
    [SerializeField] Texture2D texture;

    NativeSprite nativeSprite;
    NativeGrid<bool> isParticles;

    public override void Init(Map map)
    {
        base.Init(map);
        nativeSprite = new NativeSprite(texture);
        isParticles = new NativeGrid<bool>(nativeSprite.sizes, Allocator.Persistent);

        for (int x = 0; x < nativeSprite.sizes.x; x++)
        {
            for (int y = 0; y < nativeSprite.sizes.y; y++)
            {
                bool isParticle = nativeSprite.pixels[x, y].a != 0;
                isParticles[x, y] = isParticle;

                if (isParticle)
                {
                    int2 mapPos = position + new int2(x, y);
                    map.SetParticleType(mapPos, particleType);
                }
            }
        }
    }

    public override Bound GetBound()
    {
        return new Bound(position, nativeSprite.sizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        for (int x = 0; x < nativeSprite.sizes.x; x++)
        {
            for (int y = 0; y < nativeSprite.sizes.y; y++)
            {
                int2 mapPos = position + new int2(x, y);
                if (isParticles[x, y] && map.InBound(mapPos) && map.GetParticleType(mapPos) != particleType)
                    isParticles[x, y] = false;
            }
        }
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        //Job?
        for (int x = 0; x < nativeSprite.sizes.x; x++)
        {
            for (int y = 0; y < nativeSprite.sizes.y; y++)
            {
                int2 mapPos = position + new int2(x, y);
                int index = ArrayHelper.PosToIndex(mapPos, GameManager.GridSizes);
                if (isParticles[x, y])
                {
                    outputColor[index] = nativeSprite.pixels[x, y];
                }

            }
        }
    }

    public override void OnRenderDebug(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplySprite(ref outputColor, nativeSprite, position);
    }

    public override void Dispose()
    {
        base.Dispose();
        nativeSprite.Dispose();
        isParticles.Dispose();
    }
}
