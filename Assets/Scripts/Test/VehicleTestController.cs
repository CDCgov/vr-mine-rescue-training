using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleTestController : MonoBehaviour 
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public Transform LeftVisual;
        public Transform RightVisual;
        public bool motor;
        public float steering;
    }

    public List<AxleInfo> axleInfos;
    public float MaxMotorTorque;
    public float MaxSteeringAngle;

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider, Transform visualWheel)
    {
        if (collider == null || visualWheel == null)
            return;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void FixedUpdate()
    {
        float motor = 0;// MaxMotorTorque * Input.GetAxis("Vertical");
        float steering = 0;// MaxSteeringAngle * Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.W))
            motor += MaxMotorTorque;
        if (Input.GetKey(KeyCode.S))
            motor -= MaxMotorTorque;
        if (Input.GetKey(KeyCode.A))
            steering -= MaxSteeringAngle;
        if (Input.GetKey(KeyCode.D))
            steering += MaxSteeringAngle;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.leftWheel.steerAngle = steering * axleInfo.steering;
            axleInfo.rightWheel.steerAngle = steering * axleInfo.steering;
            
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel, axleInfo.LeftVisual);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel, axleInfo.RightVisual);
        }
    }
}