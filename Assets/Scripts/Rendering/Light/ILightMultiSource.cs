
using Unity.Collections;
using Unity.Mathematics;

public interface ILightMultiSource
{
    bool IsVisible();
    void GetLightSource(NativeList<LightSource> list, int tick);
}