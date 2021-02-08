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
    enum State { Idle, Sucking, Blowing}
    State currentState;
    float intensity;
    bool fadeIn;
    int suckDirection; 

    public override void OnInit()
    {
        base.OnInit();
        container = new Stack<ParticleType>();
    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {

    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        currentState = State.Idle;
        base.OnUpdate(ref tickBlock);


        if (currentState == State.Sucking)
        {
            player.isDirectionLocked = true;
            suckDirection = (player.lookLeft ? -1 : 1);
            fadeIn = true;
            intensity += GameManager.DeltaTime * settings.fadeIn;
        }
        else 
        {
            fadeIn = false;
            player.isDirectionLocked = false;
            intensity -= GameManager.DeltaTime * settings.fadeOut;
        }

        intensity = math.saturate(intensity);
        int shakeIntensity = 
            (int)(settings.shakeIntensity * 
            noise.cnoise((float2)tickBlock.tick * settings.shakeFrequency) * 
            math.sin(tickBlock.tick * settings.shakeFrequency * 2 * math.PI));


        pixelCamera.transform.offset = new int2(1, 0) * suckDirection * (int2)(settings.cameraOffset * GetCurrentFadeIntensity()) + new int2((int)(GetCurrentFadeIntensity() * shakeIntensity), 0);
    }

    public override void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        if (intensity == 0)
        {
            return;
        }

        var inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        int2 attractionMiddle = GetWorldPositionOffset(settings.attractionOffset);

        new ParticleSuckingEffectJob()
        {
            outputColors = outputColors,
            inputColors = inputColors,
            direction = player.lookLeft ? -1 : 1,
            settings = settings.effects,
            tickBlock = tickBlock,
            intensity = GetCurrentFadeIntensity(),
            bound = Bound.CenterAligned(pixelCamera.GetRenderPosition(attractionMiddle), settings.attractionRadius)
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
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
        if(altButton && container.Count > 0)
        {
            BlowParticles(ref tickBlock);
            currentState = State.Blowing;
        }
        else if(!altButton && container.Count < settings.capacity)
        {
            SuckParticles(ref tickBlock);
            currentState = State.Sucking;
        }
    }
 
    public void SuckParticles(ref TickBlock tickBlock)
    {
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

        for (int i = 0; i < positions.Length; i++)
        {
            if(map.GetParticleType(positions[i]) == ParticleType.None)
                continue;

            Particle particle = map.GetParticle(positions[i]);
            float xVelocity = player.lookLeft ? 1 : -1;

            int yDiff = attractionMiddle.y - positions[i].y;
            float verticalDir = math.sign(yDiff);
            particle.velocity += new float2(0, verticalDir) * GameManager.DeltaTime * settings.vortexVelocity;

            bool canSuckThisFrame = tickBlock.tick % 2 == 0;
            if (math.abs(yDiff) <= settings.suckY && canSuckThisFrame)
                particle.velocity += new float2(xVelocity, 0) * GameManager.DeltaTime * settings.suckVelocity;

            //cancel gravity
            particle.velocity += new float2(0, 1) * GameManager.DeltaTime * GameManager.PhysiXVIISetings.gravity;

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
        for (int y = 0; y < settings.maxParticlePerFrame; y++)
        {
            int xPos = player.lookLeft ? absorbBound.min.x : absorbBound.max.x;
            int ypos = random.NextInt(absorbBound.min.y, absorbBound.max.y + 1);

            int2 position = new int2(xPos, ypos);

            //if (container.Count <= 0 || map.GetParticleType(position) != ParticleType.None)
            //    continue;
            if (container.Count <= 0 || !map.InBound(position) || map.GetParticleType(position) != ParticleType.None)
                continue;

            Particle newParticle = new Particle()
            {
                fracPosition = 0.5f, //middle of the cell
                type = container.Pop(),
                velocity = velocity
            };
            map.SetParticle(position, newParticle);
        }


        //Throw bill ball
        //for (int y = 0; y < settings.absorbBound; y++)
        //{
        //    for (int x = 0; x < settings.absorbBound; x++)
        //    {
        //        int2 position = new int2(absorbBound.min.x + x, absorbBound.min.y + y);
        //        if (container.Count <= 0 || map.GetParticleType(position) != ParticleType.None)
        //            continue;

        //        Particle newParticle = new Particle()
        //        {
        //            fracPosition = 0.5f, //middle of the cell
        //            type = container.Pop(),
        //            velocity = velocity
        //        };
        //        map.SetParticle(position, newParticle);
        //    }
        //}
    }

    float GetCurrentFadeIntensity()
    {
        return EaseXVII.Evaluate(intensity, fadeIn ? settings.fadeInCurve : settings.fadeOutCurve);
    }
}
