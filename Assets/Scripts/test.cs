using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class test : MonoBehaviour
{
    void Start()
    {
        quaternion objQuaternion = quaternion.LookRotation(new float3(1, 1, 0), math.up());

        float3 forward = new float3(0, 0, 1);
        float3 globalForward = math.mul(objQuaternion, forward);
        float3 localForward = math.mul(math.inverse(objQuaternion), globalForward);
        Debug.Log($"forward {forward}, localForward {localForward}");
    }
}
