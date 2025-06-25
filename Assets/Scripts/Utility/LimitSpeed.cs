using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LimitSpeed : MonoBehaviour 
{
    [Tooltip("Max Speed in m/s")]
    public float MaxSpeed;

    Rigidbody _rb;

    void Start () 
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate () 
    {
        float speed = _rb.velocity.magnitude;
        if (speed > MaxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * MaxSpeed;
        }
    }
}