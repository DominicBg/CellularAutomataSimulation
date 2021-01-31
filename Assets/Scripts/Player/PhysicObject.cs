using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public abstract class PhysicObject : LevelObject
{
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

    [Header("Debug")]
    public PhysicBound.BoundFlag debugBoundFlag;
    protected void DebugAllPhysicBound(ref NativeArray<Color32> outputColor)
    {
        if ((debugBoundFlag & PhysicBound.BoundFlag.All) > 0)
            DebugPhysicBound(ref outputColor, physicData.physicBound.GetCollisionBound(position), Color.magenta);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Feet) > 0)
            DebugPhysicBound(ref outputColor, physicData.physicBound.GetBottomCollisionBound(position), Color.yellow);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Left) > 0)
            DebugPhysicBound(ref outputColor, physicData.physicBound.GetLeftCollisionBound(position), Color.red);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Right) > 0)
            DebugPhysicBound(ref outputColor, physicData.physicBound.GetRightCollisionBound(position), Color.blue);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Top) > 0)
            DebugPhysicBound(ref outputColor, physicData.physicBound.GetTopCollisionBound(position), Color.cyan);
    }

    protected void DebugPhysicBound(ref NativeArray<Color32> outputColor, Bound bound, Color color)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }
        GridRenderer.ApplyPixels(ref outputColor, ref positions, ref colors);
        positions.Dispose();
        colors.Dispose();
    }
}
