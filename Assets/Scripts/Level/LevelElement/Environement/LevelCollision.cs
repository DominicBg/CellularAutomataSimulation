using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelCollision : LevelElement
{
    [SerializeField] Texture2D texture = default;
    NativeArray<Color32> nativeTexture;
    public bool hasCollision;

    public override void Init(Map map)
    {
        base.Init(map);
        nativeTexture = RenderingUtils.GetNativeArray(texture, Allocator.Persistent);

        if(hasCollision)
        {
            for (int i = 0; i < nativeTexture.Length; i++)
            {
                if(nativeTexture[i].a != 0)
                {
                    int2 position = ArrayHelper.IndexToPos(i, GameManager.GridSizes);
                    //Add static collision?
                    map.SetParticleType(position, ParticleType.Rock);
                }
            }
        }
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        GridRenderer.ApplyTexture(ref outputColor, ref nativeTexture);
    }

    public override void Dispose()
    {
        base.Dispose();
        nativeTexture.Dispose();
    }
}
