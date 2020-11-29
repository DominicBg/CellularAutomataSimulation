using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct HandlePlayerJob : IJob
{
    public Map map;
    public PhysicBound physicBound;
    public int2 direction;
    public int jumpIndex;
    public NativeArray<int> jumpArray;

    public NativeReference<int2> positionRef;
    public void Execute()
    {
        int2 position = positionRef.Value;
        int2 nextPosition = position;

        if (jumpIndex < jumpArray.Length)
        {
            int jumpDirection = jumpArray[jumpIndex];
            if (jumpDirection == 1)
            {
                nextPosition = map.Jump(ref physicBound, position);
            }
            else if (jumpDirection == -1)
            {
                nextPosition = map.ApplyGravity(ref physicBound, position);
            }
        }
        else
        {
            nextPosition = map.ApplyGravity(ref physicBound, position);
        }

        nextPosition = map.HandlePhysics(ref physicBound, nextPosition, nextPosition + direction);
        if (math.any(position != nextPosition))
        {
            map.RemoveSpriteAtPosition(position, ref physicBound);
            map.SetPlayerAtPosition(nextPosition, ref physicBound);
        }
        positionRef.Value = nextPosition;
    }
}