using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;

public class ParticleBlower : EquipableElement
{
    public ParticleBlowerScriptable settings => (ParticleBlowerScriptable)baseSettings;
    Stack<ParticleType> container;


    public override void OnInit()
    {
        base.OnInit();
        container = new Stack<ParticleType>();
    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {

    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        int2 absorbMiddle = GetWorldPositionOffset(settings.absorbOffset);
        int2 attractionMiddle = GetWorldPositionOffset(settings.attractionOffset);
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(absorbMiddle, settings.absorbBound), scene.CameraPosition, Color.red * 0.75f);
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(attractionMiddle, settings.attractionRadius), scene.CameraPosition, Color.blue * 0.25f);
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        if(altButton)
        {
            BlowParticles(ref tickBlock);
        }
        else
        {
            SuckParticles();
        }
    }
 
    public void SuckParticles()
    {
        if (container.Count >= settings.capacity)
            return;

        int2 absorbMiddle = GetWorldPositionOffset(settings.absorbOffset);
        Bound absorbBound = Bound.CenterAligned(absorbMiddle, settings.absorbBound);
        var absorbBoundPos = absorbBound.GetPositionsGrid(Allocator.Temp);

        for (int i = 0; i < absorbBoundPos.Length; i++)
        {
            if (map.CanPush(absorbBoundPos[i], GameManager.PhysiXVIISetings))
            {
                container.Push(map.GetParticleType(absorbBoundPos[i]));
                map.SetParticleType(absorbBoundPos[i], ParticleType.None);
            }
        }

        int2 attractionMiddle = GetWorldPositionOffset(settings.attractionOffset);

        var positions = Bound.CenterAligned(attractionMiddle, settings.attractionRadius).GetPositionsGrid();
        // GridHelper.GetCircleAtPosition(attractionMiddle, settings.attractionRadius, map.Sizes, Allocator.Temp);

        for (int i = 0; i < positions.Length; i++)
        {
            if(map.GetParticleType(positions[i]) == ParticleType.None)
                continue;

            Particle particle = map.GetParticle(positions[i]);
            float2 direction = math.normalize(absorbMiddle - positions[i]);
            particle.velocity += direction * GameManager.DeltaTime * settings.suckVelocity;
            map.SetParticle(positions[i], particle);
        }

        positions.Dispose();
        absorbBoundPos.Dispose();
    }

    public void BlowParticles(ref TickBlock tickBlock)
    {
        int2 middlePos = GetWorldPositionOffset(settings.absorbOffset);
        Bound absorbBound = Bound.CenterAligned(middlePos, settings.absorbBound);
        float2 velocity = 0;
        velocity.x = player.lookLeft ? -settings.blowVelocity : settings.blowVelocity;

        Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex((uint)tickBlock.tick);

        for (int y = 0; y < settings.absorbBound; y++)
        {
            int x = random.NextInt(settings.absorbBound + 1);

            int2 position = new int2(absorbBound.min.x + x, absorbBound.min.y + y);

            if (container.Count <= 0 || map.GetParticleType(position) != ParticleType.None)
                continue;

            Particle newParticle = new Particle()
            {
                fracPosition = 0.5f, //middle of the cell
                type = container.Pop(),
                velocity = velocity
            };
            map.SetParticle(position, newParticle);
        }
    }
}
