
using Unity.Mathematics;

public struct TickBlock
{
    public const uint randomTick = 1851936439u;

    public int tick;
    public uint tickSeed;
    public Random random;

    public void Init()
    {
        tick = 0;
        tickSeed = randomTick;
        random = new Random(tickSeed);
    }

    public void UpdateTick()
    {
        tick++;
        tickSeed = random.NextUInt();
        random.InitState(tickSeed);
    }

    public float DurationSinceTick(int oldTick)
    {
        return (tick - oldTick) * GameManager.DeltaTime;
    }
}
