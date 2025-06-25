using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WheelAxisController : MonoBehaviour {
    
    //CDC BH-20 EDITION
    public bool isDriving = true;

    public bool IsModelReversed = false;
    public float torque = 500f;
    public float maxSpeed = 80f;
    public float brakeForce;
    internal float previousRollAngle = 0.0f;

    public WheelCollider leftWheelCollider;
    public WheelCollider rightWheelCollider;
    public Transform leftWheelTransform;
    public Transform rightWheelTransform;

    private WheelCollider[] wheels;
    private Animator WheelReference;
    private int directionmodifier;
    private bool isFront = true;
    private bool defaultDrivingState;
    private float gas = 0f;
    private float brake = 0f;


    // Use this for initialization
    void Start () {
        wheels = new WheelCollider[2];
        wheels[0] = rightWheelCollider;
        wheels[1] = leftWheelCollider;

        if (GameManager.inputManager != null)
        {
            GameManager.inputManager.TriggersEvent.AddListener(GasBreakEvent);
            GameManager.inputManager.ButtonAEvent.AddListener(AwdEvent);
            GameManager.inputManager.ButtonLeftJoystickEvent.AddListener(GearBoxEvent);
        }

        defaultDrivingState = isDriving;
        directionmodifier = IsModelReversed ? -1 : 1;
    }


    // Update is called once per frame
    void Update () {
        UpdateWheels();
        UpdateRotation(rightWheelTransform,rightWheelCollider);
        UpdateRotation(leftWheelTransform,leftWheelCollider);

    }
    
    void UpdateWheels()
    {
        for ( int i = 0; i < wheels.Length; i++)
        {
            if (isDriving && Speedometer( ref wheels[i]) <= maxSpeed) 
            {
                wheels[i].motorTorque = gas > 0 ? ((EvaluateAnimationCurve(GameManager.masterSetting.acceleration, Mathf.Abs(gas)) * torque)* directionmodifier) : 0;

            }
            else
            {
                wheels[i].motorTorque = 0.00001f;
                wheels[i].brakeTorque += Time.deltaTime;
            }
            wheels[i].brakeTorque = brake > 0 ? EvaluateAnimationCurve(GameManager.masterSetting.breakingCurve,brake) * brakeForce : 0;
        }
    }
    float Speedometer(ref WheelCollider x)
    {
        float circonference = x.radius * Mathf.PI * 2;
        float Kmh = circonference * Mathf.Abs(x.rpm) * 0.06f;
        GameManager.currentSpeed = Kmh;
        return Kmh;
    }

    void UpdateRotation(Transform wT, WheelCollider wC)
    {

        float inv =(!isFront) ? -1f : 1f;
        float offset = (isFront) ? -90 : -90;
        Vector3 vec = Vector3.Normalize(transform.parent.position - wC.attachedRigidbody.transform.position);

        wC.steerAngle = Vector3.Angle(inv * transform.parent.right, vec) + offset;

        Vector3 pos;
        Quaternion q;

        wC.GetWorldPose(out pos, out q);
        wT.transform.rotation = q;



    }
    // event controlled method
    void AwdEvent(float value, InputControllerState cs)
    {
        
        if (defaultDrivingState) return;
        isDriving = (cs == InputControllerState.Hold);

    }


    private void GearBoxEvent(float value, InputControllerState cs)
    {
        if (cs == InputControllerState.Pressed)
        {
            if (directionmodifier == 1) directionmodifier = -1;
            else directionmodifier = 1;
            gas = 0f;
        }
    }

    private void GasBreakEvent(float x, InputControllerState cs)
    {
        brake = (x > 0.2f) ? x  : 0;
        gas = (x < -0.2f) ? Mathf.Abs(x) : 0;

        if (x < 0.2f && x > -0.2f)
        {
            gas = 0;
            brake = 0.1f;
        }

    }
    //Event End
    private float EvaluateAnimationCurve( AnimationCurve curve , float time)
    {
        return curve.Evaluate(time);
        
    }

}


