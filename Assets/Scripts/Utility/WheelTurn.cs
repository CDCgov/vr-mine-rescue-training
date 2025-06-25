using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTurn : MonoBehaviour
{
    public Transform SteeringWheel;
    public Transform LeftWheel;
    private Vector3 _priorRot;

    private void Start()
    {
        _priorRot = LeftWheel.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 _rotation = SteeringWheel.localRotation.eulerAngles;
        _rotation.y = LeftWheel.localRotation.eulerAngles.y;
        Debug.Log("Rot: " + _rotation);
        SteeringWheel.localRotation = Quaternion.Euler(_rotation);
    }
}
