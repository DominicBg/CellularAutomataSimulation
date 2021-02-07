using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public abstract class PhysicObject : LevelObject
{
    [Header("Debug")]
    public PhysicBound.BoundFlag debugBoundFlag;
    [Header("PhysicData")]
    public PhysicData physicData;

    protected void InitPhysicData(int2 sizes, float mass = 10)
    {
        physicData.physicBound = new PhysicBound(new Bound(0, sizes));
        physicData.gridPosition = position;
        physicData.position = position;
        physicData.mass = mass;
    }
    protected void InitPhysicData(Texture2D collisionTexture, float mass = 10)
    {
        physicData.physicBound = new PhysicBound(collisionTexture);
        physicData.gridPosition = position;
        physicData.position = position;
        physicData.mass = mass;
    }

    protected void HandlePhysic()
    {
        PhysiXVII.HandlePhysics(this, map);    
    }

    protected bool IsGrounded()
    {
        return PhysiXVII.IsGrounded(in physicData, map, position);
    }

    public void SetPosition(int2 position)
    {
        this.position = position;
        this.physicData.position = position;
        this.physicData.gridPosition = position;
    }

    /// <summary>
    /// If the physic object is submerged, this will make it move upward without destroying particles
    /// </summary>
    //protected void MoveUpFromPile()
    //{
    //    int2 pushUp = new int2(0, 1);
    //    Bound bound = physicData.physicBound.GetTopCollisionBound(position + pushUp);
    //    using (var boundPos = bound.GetPositionsGrid(Allocator.Temp))
    //    {
    //        bool isBlocked = false;
    //        for (int i = 0; i < boundPos.Length; i++)
    //        {
    //            if(!map.IsFreePosition(boundPos[i]) && !map.CanPush(boundPos[i], GameManager.PhysiXVIISetings))
    //            {
    //                isBlocked = true;
    //                break;
    //            }
    //        }

    //        if (!isBlocked)
    //        {
    //            //Set particles at feet position, move up the character
    //            int yPos = physicData.physicBound.GetBottomCollisionBound(position).min.y;
    //            map.RemoveSpriteAtPosition(position, ref physicData.physicBound);
    //            for (int i = 0; i < boundPos.Length; i++)
    //            {
    //                int2 oldPos = boundPos[i];
    //                int2 newPos = new int2(boundPos[i].x, yPos);
    //                ParticleType type = map.GetParticleType(oldPos);
    //                map.SetParticleType(newPos, type);
    //            }
    //            SetPosition(position + pushUp);
    //            map.SetSpriteAtPosition(position, ref physicData.physicBound);
    //        }
    //    }
    //}


    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        if ((debugBoundFlag & PhysicBound.BoundFlag.All) > 0)
            GridRenderer.DrawBound(ref outputColors, physicData.physicBound.GetCollisionBound(position), scene.CameraPosition, Color.magenta * 0.5f);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Feet) > 0)
            GridRenderer.DrawBound(ref outputColors, physicData.physicBound.GetBottomCollisionBound(position), scene.CameraPosition, Color.yellow * 0.5f);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Left) > 0)
            GridRenderer.DrawBound(ref outputColors, physicData.physicBound.GetLeftCollisionBound(position), scene.CameraPosition, Color.red * 0.5f);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Right) > 0)
            GridRenderer.DrawBound(ref outputColors, physicData.physicBound.GetRightCollisionBound(position), scene.CameraPosition, Color.blue * 0.5f);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Top) > 0)
            GridRenderer.DrawBound(ref outputColors, physicData.physicBound.GetTopCollisionBound(position), scene.CameraPosition, Color.cyan * 0.5f);
    }
}
