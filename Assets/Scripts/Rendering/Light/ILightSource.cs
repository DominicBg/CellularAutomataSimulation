
using Unity.Mathematics;

public interface ILightSource
{
    bool isVisible();
    LightSource GetLightSource(int tick);
}