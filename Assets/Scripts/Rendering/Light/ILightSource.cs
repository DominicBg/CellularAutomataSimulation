
using Unity.Mathematics;

public interface ILightSource
{
    bool IsVisible();
    LightSource GetLightSource(int tick);
}