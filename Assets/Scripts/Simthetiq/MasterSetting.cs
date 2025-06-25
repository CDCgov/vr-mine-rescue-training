using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;



/// <summary>
/// Contain All the settings for the other script, 
/// </summary>
[System.Serializable]
public class MasterSetting : MonoBehaviour
{ 
    public AnimationCurve acceleration = AnimationCurve.Linear(0,0,1,1);
    public AnimationCurve directionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve breakingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public Rigidbody bH20Rigidbody;
    public float bH20Mass;
    public Transform bH20centerOfMassTr;
    public float bH20Drag;
    public float bH20AngularDrag;


    //wheelCollider Variable
    public WheelCollider frontleft = new WheelCollider();
    public WheelCollider frontRight = new WheelCollider();
    public WheelCollider rearLeft = new WheelCollider();
    public WheelCollider rearRight = new WheelCollider();


    //AxisWheelController Variable;
    public WheelAxisController frontAxis;
    public WheelAxisController rearAxis;
    public float frontSpdTorque;
    public float frontBrkTorque;
    public float rearSpdTorque;
    public float rearBrkTorque;
    private void Awake()
    {

    }

    // Use this for initialization
    void Start ()
    {
        RigidBodyInit();
        InitWheelAxisController();

    }
    /// <summary>
    /// initiate the WheelAxisControllers Scripts
    /// </summary>
    public void InitWheelAxisController()
    {
        if (frontAxis != null)
        {
            frontAxis.torque = frontSpdTorque;
            frontAxis.brakeForce = frontBrkTorque;
            frontAxis.isDriving = false;
        }

        if (rearAxis != null)
        {
            rearAxis.torque = rearSpdTorque;
            rearAxis.brakeForce = rearBrkTorque;
            rearAxis.isDriving = true;
        }
    }

    /// <summary>
    /// initiate the RigidBodie variable 
    /// </summary>
    public void RigidBodyInit()
    {
        bH20Rigidbody.mass = bH20Mass;
        bH20Rigidbody.drag = bH20Drag;
        bH20Rigidbody.angularDrag = bH20AngularDrag;
    }
}