using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "World1GemContextScriptable", menuName = "Context/World1GemContextScriptable", order = 1)]
public class World1GemContextScriptable : ScriptableObject
{
    public World1GemRMJob.CameraSettings cameraSettings;
    public World1GemRMJob.DiamondSettings diamondSettings;
    public RayMarchingEdgeDetectorJob.RayMarchingEdgeDetectorSettings edgeDetectionSettings;
}
