using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//obscelete

public class LevelCollision : LevelElement
{
    [SerializeField] Texture2D texture = default;
    NativeArray<Color32> nativeTexture;
    public bool hasCollision;

    public override void OnInit()
    {
        nativeTexture = RenderingUtils.GetNativeArray(texture, Allocator.Persistent);

        if(hasCollision)
        {
            for (int i = 0; i < nativeTexture.Length; i++)
            {
                if(nativeTexture[i].a != 0)
                {
                    int2 position = ArrayHelper.IndexToPos(i, GameManager.RenderSizes);
                    map.SetParticleType(position, ParticleType.Rock);
                }
            }
        }
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        GridRenderer.ApplyTexture(ref outputColor, ref nativeTexture);
    }

    public override void Dispose()
    {
        base.Dispose();
        nativeTexture.Dispose();
    }
}
