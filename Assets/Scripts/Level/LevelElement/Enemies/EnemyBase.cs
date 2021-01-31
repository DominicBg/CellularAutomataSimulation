using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EnemyBase : PhysicObject
{
    public SpriteAnimator spriteAnimator;
    public EnemyBaseSettings baseSettings;
    int lookDirection;


    public override void OnInit()
    {
        base.OnInit();
        spriteAnimator = new SpriteAnimator(baseSettings.spriteSheet);
        InitPhysicData(baseSettings.sizes);
    }

    public override Bound GetBound()
    {
        return new Bound(position, baseSettings.sizes);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        lookDirection = (int)math.sign(player.position.x - position.x);

        if (math.distancesq(position.x, player.position.x) < baseSettings.aggroRange * baseSettings.aggroRange)
        {
            physicData.velocity.x = lookDirection * baseSettings.movementSpeed * GameManager.DeltaTime;
        }
        else
        {
            physicData.velocity.x = 0;
        }
        HandlePhysic();
        spriteAnimator.Update();
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        spriteAnimator.Render(ref outputColors, renderPos, lookDirection == -1);
    }
    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, GetBound(), scene.CameraPosition, Color.cyan * .5f);
    }
    public override void Dispose()
    {
        base.Dispose();
        spriteAnimator.Dispose();
    }
}
