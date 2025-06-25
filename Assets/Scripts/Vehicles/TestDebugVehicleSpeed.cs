using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDebugVehicleSpeed : MonoBehaviour
{
    public Rigidbody Rigidbody;

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(Time.time + ": " + Rigidbody.velocity.magnitude * 2.4f);
    }
}
