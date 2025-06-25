using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadNPCHeadControl : MonoBehaviour
{
    public Vector3 RotationOverride;
    public Transform HeadBone;
    public bool PerformOverride = false;

    private void LateUpdate()
    {
        if (PerformOverride)
        {
            HeadBone.localEulerAngles = RotationOverride;
        }
    }
}
