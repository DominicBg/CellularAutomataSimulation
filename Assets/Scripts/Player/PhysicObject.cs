using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PhysicObject : MonoBehaviour
{
    public int2 GridPosition => physicData.GridPosition;
    PhysicData physicData;

    public struct PhysicData
    {
        public float2 position;
        public float2 velocity;
        public PhysicBound physicBound;
        public int2 GridPosition => (int2)(position / GameManager.GridScale);
        public void SetGridPosition(int2 gridPosition)
        {
            position = gridPosition * GameManager.GridScale;
        }
    }

    public void HandlePhysic(Map map, float deltaTime)
    {
        NativeReference<PhysicData> physicDataReference = new NativeReference<PhysicData>(Allocator.TempJob);
        physicDataReference.Value = physicData;
        new HandlePhysicJob(map, deltaTime, physicDataReference).Run();
        physicData = physicDataReference.Value;
        physicDataReference.Dispose();
    }

    [BurstCompile]
    public struct HandlePhysicJob : IJob
    {
        Map map;
        NativeReference<PhysicData> physicDataReference;
        float deltaTime;

        public HandlePhysicJob(Map map, float deltaTime, NativeReference<PhysicData> physicData)
        {
            this.map = map;
            this.deltaTime = deltaTime;
            this.physicDataReference = physicData;
        }

        public void Execute()
        {
            PhysicData physicData = physicDataReference.Value;

            float2 currentPosition = physicData.position;
            float2 nextPosition = currentPosition + physicData.velocity * deltaTime;


            int2 currentGridPosition = physicData.GridPosition;
            int2 nextGridPosition = (int2)(nextPosition / GameManager.GridScale);

            //refactor gravity
            nextGridPosition = map.ApplyGravity(ref physicData.physicBound, nextGridPosition);
            nextGridPosition = map.HandlePhysics(ref physicData.physicBound, currentGridPosition, nextGridPosition);

            if (math.any(currentGridPosition != nextGridPosition))
            {
                map.RemoveSpriteAtPosition(currentGridPosition, ref physicData.physicBound);
                map.SetSpriteAtPosition(nextGridPosition, ref physicData.physicBound);
            }

            physicData.SetGridPosition(nextGridPosition);
            physicDataReference.Value = physicData;
        }
    }
}
