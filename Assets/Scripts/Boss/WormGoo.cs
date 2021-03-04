using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class WormGoo : PhysicObject
{
    public int2 sizes = 25;
    public int2 collisionSize = 15;
    public int mass = 5;
    public float angle = 0;
    public Texture2D texture;
    public Texture2D normalTexture;
    public Texture2D reflectionTexture;
    NativeSprite sprite;

    public EnvironementReflectionInfo envReflInfo = EnvironementReflectionInfo.Default();

    public float angleSmooth = 3;
    public float maxVelocity = 25;
    public float stretchMax = 5f;
    private float2 currentSizes;
    public bool useSuperSample = false;

    public override Bound GetBound()
    {
        return Bound.CenterAligned(position, (int2)currentSizes);
    }

    public override void OnInit()
    {
        currentSizes = sizes;
        base.OnInit();
        sprite = new NativeSprite(texture, normalTexture, reflectionTexture);
        InitPhysicData(new PhysicBound(Bound.CenterAligned(0, collisionSize)), mass);
        physicData.applyFriction = true;
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        PhysicBound physicBound = new PhysicBound(Bound.CenterAligned(0, collisionSize));
        physicData.physicBound = physicBound;
        base.OnUpdate(ref tickBlock);

        float previousVelocityLength = math.length(physicData.velocity);
        HandlePhysic();

        if(math.all(physicData.velocity == 0) || math.all(math.abs(physicData.velocity) < 0.01f))
        {
            currentSizes = math.lerp(currentSizes, sizes, GameManager.DeltaTime * angleSmooth);
        }
        else if(physicData.hasCollision)
        {
            angle = math.degrees(math.atan2(physicData.collisionNormalNormalized.x, -physicData.collisionNormalNormalized.y));
            float squashStrength = -math.remap(0, maxVelocity, 0, 1, previousVelocityLength);
            currentSizes = CalculateSquish(squashStrength);
        }
        else
        {
            float velocityLength = math.length(physicData.velocity);
            float2 velocityDir = physicData.velocity / velocityLength;
            float desiredAngle = math.degrees(math.atan2(velocityDir.y, velocityDir.x));
            angle = MathUtils.LerpAngle(angle, desiredAngle, GameManager.DeltaTime * angleSmooth);

            //float stretchRatio = MathUtils.RemapSaturate(0, maxVelocity, -stretchMax/2, stretchMax/2, velocityLength);
            float stretchRatio = math.remap(0, maxVelocity, 0, 1, velocityLength);

            float2 desiredSize = CalculateSquish(stretchRatio);
            currentSizes = math.lerp(currentSizes, desiredSize, GameManager.DeltaTime * angleSmooth);
        }
    }

    /// <summary>
    /// squash = -1, normal = 0, squish = 1
    /// </summary>
    float2 CalculateSquish(float squishRatio)
    {
        squishRatio = math.clamp(squishRatio, -1, 1);
        return new float2(
              sizes.x + squishRatio * stretchMax,
              sizes.y - squishRatio * stretchMax
              );
    }

    NativeGrid<Color32> tempPixels;
    NativeGrid<float3> tempNormal;
    NativeGrid<float> tempReflection;
    int relfectionIndex;
    int2 trueRenderPos;

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        RotationBound rotationBound = new RotationBound(GetBound(), angle, RotationBound.Anchor.Center);
        if(!useSuperSample)
        {
            GridRenderer.DrawRotationSprite(ref outputColors, in rotationBound, info.cameraHandle, in sprite);
        }
        else
        {
            relfectionIndex = info.GetReflectionIndex();
            sprite.GetRotationSprite(in rotationBound, info.cameraHandle, out tempPixels, out tempNormal, out tempReflection, out int2 min, out int2 max);
            trueRenderPos = info.cameraHandle.GetRenderPosition(min);
            GridRenderer.DrawLitSprite(ref outputColors, in tempPixels, in tempNormal, position, trueRenderPos, info.lightSources, ShadingLitInfo.Default());  
        }
    }

    public override void RenderReflection(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        if (useSuperSample)
        {    
            GridRenderer.ApplySpriteEnvironementReflection(ref outputColors, in tempPixels, in tempNormal, in tempReflection, trueRenderPos, relfectionIndex, ref info, envReflInfo);
            tempPixels.Dispose();
            tempNormal.Dispose();
            tempReflection.Dispose();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        sprite.Dispose();
    }
}
