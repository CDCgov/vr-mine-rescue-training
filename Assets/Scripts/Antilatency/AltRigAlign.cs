using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltRigAlign : MonoBehaviour
{
    public AltPoseDriver ReferenceDriver;

    private float _currentOffset = 5000;

    private void Start()
    {
        if (ReferenceDriver == null)
        {
            Debug.LogError($"Reference driver not set on AltRigAlign!");
            enabled = false;
        }
    }

    private void Update()
    {
        var altEuler = ReferenceDriver.AltRotation.eulerAngles;
        var nativeEuler = ReferenceDriver.NativeRotation.eulerAngles;

        var yOffset = altEuler.y - nativeEuler.y;

    }
}
