using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GolemController : CharacterController
{

    bool isControlled;
    List<ParticleType> particles;

    public void SummonGolem(int2 position, List<ParticleType> particles)
    {
        SetPosition(position);
        this.particles = particles;
        isControlled = true;
    }


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if(isControlled)
            base.OnUpdate(ref tickBlock);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        var infoPtr = info;
        var tickBlockPtr = tickBlock;
        var mapPtr = map;

        var sprite = spriteAnimator.GetCurrentSprite();
        Func<int2, bool> canRender = pos => sprite[pos].a != 0;
        Func<int2, Color> getColor = pos =>
        {
            int index = (pos.y * sprite.Sizes.x + pos.x) % particles.Count;
            ParticleType type = particles[index];
            return ParticleRenderUtil.GetColorForType(pos, type, ref GridRenderer.Instance.particleRendering, ref tickBlockPtr, ref mapPtr, infoPtr.lightSources);
        };
        GridRenderer.ApplySpriteCustom(ref outputColors, in sprite, renderPos, canRender, getColor);
    }
}
