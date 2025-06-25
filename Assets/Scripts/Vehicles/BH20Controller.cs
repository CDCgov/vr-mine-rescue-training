using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mogoson.Machinery;

public class BH20Controller : MonoBehaviour
{
    public float MotorTorque = 1500;
    public float BrakeTorque = 1500;

    [Header("Handling MechAnimSetting")]
    public Mechanism handlingMech;
    public float HandlingVelocity = 1f;
    [Header("TrapDoor MechAnimSetting")]
    public Mechanism trapDoorMech;
    public float trapDoorVelocity = 10f;
    [Header("BucketHeightAnim MechAnimSetting")]
    public Mechanism bucketHeightMech;
    public float bucketHeightVelocity = 5f;
    [Header("DumpingAnim MechAnimSetting")]
    public Mechanism dumpingMech;
    public float dumpingVelocity = 10f;
    [Header("BatteryAnim MechAnimSetting")]
    public Mechanism batteryMech;
    public float batteryVelocity = 5f;

    public WheelCollider[] WheelColliders;
    public Transform[] WheelModels;

    public Transform RotatingJoint;
    
    private Rigidbody _rb;

    void Start()
    {
        //_wheelColliders = transform.GetComponentsInChildren<WheelCollider>(false);
        _rb = GetComponent<Rigidbody>();
    }

    public void SpeedBoost(float value)
    {
        Vector3 dir;
        if (_rb.velocity.magnitude > 0.1f)
            dir = _rb.velocity.normalized;
        else
            dir = transform.forward;

        _rb.velocity = _rb.velocity + dir * value;
    }

    public void Accelerate(float value)
    {
        float torque = MotorTorque * value;
        //Debug.Log($"Applying torque {torque} to {WheelColliders.Length}");
        WheelHit hit;
        foreach (var wheel in WheelColliders)
        {
            //check if we are reversing
            if (wheel.rpm * value < 0)
            {
                wheel.motorTorque = torque * 2;
            }
            else if (Mathf.Abs(wheel.rpm) < 100)
            {
                wheel.GetGroundHit(out hit);
                wheel.motorTorque = torque - hit.forwardSlip * torque;
                //wheel.motorTorque = torque;
            }
            else
            {
                wheel.motorTorque  = 0;
            }
            wheel.brakeTorque = 0;
        }
    }

    public void Brake(float value)
    {
        float torque = BrakeTorque * value;
        foreach (var wheel in WheelColliders)
        {
            wheel.brakeTorque = torque;
            wheel.motorTorque = 0;
        }
    }

    public void DriveCenterPivot(float value)
    {
        handlingMech.Drive(value * HandlingVelocity * -1.0f, DriveType.Ignore);
    }

    // all Method ending with Mech are method triggered by the input manager, each method send the value to expend or retract a specific moving part.
    // public void HandlingMech(float value, InputControllerState cs)
    // {
    // 	if (cs == InputControllerState.Active)
    // 	{
    // 		handlingMech.Drive(HandlingVelocity * GameManager.masterSetting.directionCurve.Evaluate(Mathf.Abs(value)) * -Mathf.Sign(value),
    // 			DriveType.Ignore);
    // 	}
    // }

    public void DriveTrapDoor(bool extend)
    {
        if (extend)
        {
            trapDoorMech.Drive(trapDoorVelocity, DriveType.Ignore);
        }
        else
        {
            trapDoorMech.Drive(-trapDoorVelocity, DriveType.Ignore);
        }
    }

    public void DriveBucket(bool extend)
    {
        if (extend)
        {
            bucketHeightMech.Drive(bucketHeightVelocity, DriveType.Ignore);
        }
        else
        {
            bucketHeightMech.Drive(-bucketHeightVelocity, DriveType.Ignore);
        }
    }


    public void DriveDumping(bool extend)
    {
        if (extend)
        {
            dumpingMech.Drive(dumpingVelocity, DriveType.Ignore);
        }
        else
        {
            dumpingMech.Drive(-dumpingVelocity, DriveType.Ignore);
        }
    }

    public void DriveBattery(bool extend)
    {
        if (extend)
        {
            batteryMech.Drive(batteryVelocity, DriveType.Ignore);
        }
        else
        {
            batteryMech.Drive(-batteryVelocity, DriveType.Ignore);
        }
    }

    void UpdateRotation(Transform wT, WheelCollider wC, bool isFront)
    {
        //float steerAngle = Vector3.Angle(transform.forward, wT.forward);
        //Debug.Log($"Angle {steerAngle}");
        float inv = (!isFront) ? -1f : 1f;
        float offset = (isFront) ? -90 : -90;


        //Vector3 vec = Vector3.Normalize(transform.parent.position - wC.attachedRigidbody.transform.position);
        //Vector3 vec = Vector3.Normalize(wC.transform.parent.parent.position - wC.attachedRigidbody.transform.position);
        //wC.steerAngle = Vector3.Angle(inv * transform.parent.right, vec) + offset;



        wC.steerAngle = wT.parent.eulerAngles.y - transform.eulerAngles.y;


        Vector3 pos;
        Quaternion q;

        wC.GetWorldPose(out pos, out q);
        wT.transform.rotation = q;



    }

    // Update is called once per frame
    void Update()
    {
        Debug.Assert(WheelColliders.Length == 4 && WheelModels.Length == 4);

        float steerAngle = RotatingJoint.eulerAngles.y;

        UpdateRotation(WheelModels[0], WheelColliders[0], true);
        UpdateRotation(WheelModels[1], WheelColliders[1], true);
        UpdateRotation(WheelModels[2], WheelColliders[2], false);
        UpdateRotation(WheelModels[3], WheelColliders[3], false);


        /* for (int i = 0; i < WheelColliders.Length && i < WheelModels.Length; i++)
        {
            UpdateRotation(WheelModels[i], WheelColliders[i]);
        }
        */
    }
}
