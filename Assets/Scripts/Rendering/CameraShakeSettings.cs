[System.Serializable]
public struct CameraShakeSettings
{
    public float intensity;
    public float duration;
    public float speed;
    public EaseXVII.Ease intensityEase;
    public bool inverseSpeedEase;
    public EaseXVII.Ease speedEase;

    //todo blend with original, need 2 render pass
    //public float blendWithOriginal;   
}
