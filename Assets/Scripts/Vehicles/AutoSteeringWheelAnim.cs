using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSteeringWheelAnim : MonoBehaviour
{
    public Transform TargetWheel;
    public float AngleMultiplier = 1;
    public float DeltaMaxMultiplier = 1;
    private bool _allowAnimate = false;
    private float _animProgress = 0;
    private float startAngle;
    private float endAngle;

    
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 rotation = transform.localEulerAngles;
        Vector3 targetRotation = TargetWheel.localEulerAngles;

        //if(Mathf.Abs(rotation.z - targetRotation.y) < 0.5f)
        //{
        //    rotation.z = rotation.z + (targetRotation.y - rotation.z) * Time.deltaTime;
        //}
        //transform.localEulerAngles = rotation;
        if(targetRotation.y > 180)
        {
            targetRotation.y = targetRotation.y - 360;
        }

        Vector3 modifiedRotation = rotation;
        modifiedRotation.z = targetRotation.y;
        modifiedRotation.z = modifiedRotation.z * AngleMultiplier;

        rotation = Quaternion.RotateTowards(Quaternion.Euler(rotation), Quaternion.Euler(modifiedRotation), Time.fixedDeltaTime * DeltaMaxMultiplier).eulerAngles;
        transform.localEulerAngles = rotation;
    }
}
