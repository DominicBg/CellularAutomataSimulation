using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GolemController : CharacterController
{
    public bool isSummoned { get; private set; }
    [SerializeField] Explosive.Settings explosiveSettings = new Explosive.Settings() { radius = 5, strength = 15 };
    List<ParticleType> particles;
    int2 sizes;

    public void SummonGolem(int2 position, List<ParticleType> particles, int2 sizes)
    {
        SetPosition(position);
        this.particles = particles;
        this.sizes = sizes;
        isSummoned = true;
        SetControls(controlsGolem: true);

        physicData.velocity = player.physicData.velocity + new float2(0, 100);
    }

    public void SetControls(bool controlsGolem)
    {
        player.SetIsControlled(!controlsGolem);
        SetIsControlled(controlsGolem);

        pixelCamera.transform.target = (controlsGolem) ? (LevelObject)this : (LevelObject)player;
    }

    public void ToggleControls()
    {
        SetControls(!allowsInput);
    }

    public void ExploseGolem()
    {

        //set particles at position
        Bound bound = new Bound(position, sizes);
        var positions = bound.GetPositionsGrid();
        for (int i = 0; i < positions.Length; i++)
        {
            if (i < particles.Count && !map.HasCollision(positions[i], PhysiXVII.GetFlag(ParticleType.Player)))
            {
                Particle particle = new Particle() { type = particles[i], velocity = physicData.velocity, fracPosition = 0.5f };
                map.SetParticle(positions[i], particle);
            }
        }
        positions.Dispose();

        Explosive.SetExplosive(GetBound().center, in explosiveSettings, map);
        SetControls(controlsGolem: false);

        isSummoned = false;
        particles.Clear();
    }


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        if (!isSummoned)
            return;

        base.OnUpdate(ref tickBlock);
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (!isSummoned)
            return;

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
