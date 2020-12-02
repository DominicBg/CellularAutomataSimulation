using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

        if(PhysiXVII.IsGrounded(in physicData, map, physicData.gridPosition) && physicData.velocity.y < 0)
        {
            const float friction = 0.9f;
            physicData.velocity.y = 0;
            physicData.velocity *= friction;
        }
        else
        {
            const float gravity = 15;
            physicData.velocity.y -= gravity;
        }

        float2 currentPosition = physicData.position;
        float2 nextPosition = currentPosition + (physicData.velocity + physicData.controlledVelocity) * deltaTime;
        int2 currentGridPosition = physicData.gridPosition;


        int2 nextGridPosition = (int2)(nextPosition / GameManager.GridScale);
        if (math.all(currentGridPosition == nextGridPosition))
        {
            physicData.position = nextPosition;
        }
        else
        {
            PhysiXVII.HandlePhysics(ref physicData, map, nextPosition);

            if (math.distancesq(currentPosition, nextPosition) > 0.01f)
            {
                map.RemoveSpriteAtPosition(currentGridPosition, ref physicData.physicBound);
                map.SetSpriteAtPosition(physicData.gridPosition, ref physicData.physicBound);
            }
        }

        physicDataReference.Value = physicData;
    }
}