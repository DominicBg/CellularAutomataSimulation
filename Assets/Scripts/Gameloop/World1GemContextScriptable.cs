using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "World1GemContextScriptable", menuName = "Context/World1GemContextScriptable", order = 1)]
public class World1GemContextScriptable : ScriptableObject
{
    public World1GemRMJob.RenderSettings cameraSettings;
    public World1GemRMJob.DiamondSettings diamondSettings;
    public World1GemRMJob.PillarSettings pillarSettings;
    public World1GemRMJob.AstroSettings astroSettings;
    public World1GemRMJob.CameraTransform cameraTransform;
    public World1GemRMJob.LightSettings light;
    public RayMarchingEdgeDetectorJob.RayMarchingEdgeDetectorSettings edgeDetectionSettings;

    public float minPitch;
    public float maxPitch;
    public float speed;
    //public Texture2D astroTexture;
}
