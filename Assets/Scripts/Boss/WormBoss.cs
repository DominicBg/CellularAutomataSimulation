using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class WormBoss : LevelObject
{
    public int bodySize = 10;
    public int headSize = 10;
    public int bodyCount = 7;
    public float angularFrequency = .1f;
    public float angularRadius = 25;
    public float sinFrequency = 1f;
    public float sinAmplitude = 5f;

    public float bodyFollowMinDist = 0.1f;
    public float bodyFollowSpeed = 5;
    public float maxT = 0.5f;

    public Texture2D headTexture;
    public Texture2D bodyTexture;

    NativeSprite headSprite;
    NativeSprite bodySprite;

    NativeArray<BodyPart> body;
    //int2 anchorPos;

    public struct BodyPart
    {
        public int2 position;
        public int radius;
        public float angle;
    }

    public override void OnInit()
    {
        headSprite = new NativeSprite(headTexture);
        bodySprite = new NativeSprite(bodyTexture);

        base.OnInit();
        body = new NativeArray<BodyPart>(bodyCount, Allocator.Persistent);

        body[0] = new BodyPart() { radius = bodySize };
        for (int i = 1; i < body.Length; i++)
        {
            body[i] = new BodyPart() { radius = bodySize };
        }
        //anchorPos = position;
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        base.OnUpdate(ref tickBlock);

        CalculateHeadPosition(ref tickBlock);

        //for (int i = body.Length - 1; i >= 1; i--)
        for (int i = 1; i < body.Length; i++)
        {
            BodyPart bodyPart = body[i];
            BodyPart prevBodyPart = body[i-1];
            float2 diff = bodyPart.position - prevBodyPart.position;
            float distance = math.length(diff);
            float2 dir = diff / distance;

            float t = (distance > bodyFollowMinDist) ? GameManager.DeltaTime * (distance / bodyFollowMinDist) * bodyFollowSpeed : 0;
            t = math.min(t, maxT);

            bodyPart.angle = math.atan2(dir.y, dir.x);

            bodyPart.position = (int2)math.lerp(bodyPart.position, prevBodyPart.position, t);
            body[i] = bodyPart;
        }
    }

    void CalculateHeadPosition(ref TickBlock tickBlock)
    {
        float2 polarPos = MathUtils.PolarToCartesian(angularRadius, angularFrequency * tickBlock.tick * 2 * math.PI);
        float2 polarDir = math.normalize(polarPos);
        float2 polarSin = polarPos + polarDir * sinAmplitude * math.sin(sinFrequency * tickBlock.tick * 2 * math.PI);

        BodyPart headPart = body[0];
        int2 headDesiredPos = position + (int2)polarSin;
        float2 diff = headPart.position - headDesiredPos;

        float distance = math.length(diff);
        float2 dir = diff / distance;

        float t = (distance > bodyFollowMinDist) ? GameManager.DeltaTime * (distance / bodyFollowMinDist) * bodyFollowSpeed : 0;
        t = math.min(t, maxT);

        headPart.angle = math.atan2(dir.y, dir.x);
        headPart.position = (int2)math.lerp(headPart.position, headDesiredPos, t);

        body[0] = headPart;
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        base.Render(ref outputColors, ref tickBlock, renderPos, ref info);

        for (int i = body.Length - 1; i >= 1; i--)
        {
            RotationBound bodyBound = new RotationBound(Bound.CenterAligned(body[i].position, bodySize), math.degrees(body[i].angle));
            GridRenderer.DrawRotationSprite(ref outputColors, in bodyBound, info.cameraHandle, in bodySprite);
        }

        RotationBound headBound = new RotationBound(Bound.CenterAligned(body[0].position, headSize), math.degrees(body[0].angle));
        GridRenderer.DrawRotationSprite(ref outputColors, in headBound, info.cameraHandle, in headSprite);
    }

    public override Bound GetBound()
    {
        return new Bound(position, bodySize);
    }

    public override void Dispose()
    {
        base.Dispose();
        body.Dispose();

        headSprite.Dispose();
        bodySprite.Dispose();
    }
}
