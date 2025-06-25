using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTest : MonoBehaviour
{
    public float MinAngle = -45;
    public float MaxAngle = 45;
    public float Period = 1.5f;


    // Use this for initialization
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        float range = MaxAngle - MinAngle;

        float percent = (Mathf.Sin(Time.time * Period) + 1.0f) / 2.0f;

        transform.localRotation = Quaternion.Euler(0, MinAngle + range * percent, 0);
    }
}
