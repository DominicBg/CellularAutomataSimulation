using Unity.Mathematics;

public static class MathUtils
{
    /// <summary>
    /// From [-1, 1] to [0, 1]
    /// </summary>
    public static float unorm(float x) => x * 0.5f + 0.5f;

    /// <summary>
    /// From [0, 1] to [-1, 1]
    /// </summary>
    public static float snorm(float x) => x * 2 - 1;

    public static int2 quantize(int2 v, int2 cellSize)
    {
        return new int2(math.floor(v / (float2)cellSize));
    }

    public static Random CreateRandomAtPosition(int2 position, uint seed = 0)
    {
        uint randomCellSeed = (uint)(position.x + position.y * 100) + seed;
        return Random.CreateFromIndex(randomCellSeed);
    }
}
