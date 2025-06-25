using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TemporaryGravityChange : MonoBehaviour
{
    private Rigidbody _rb;

    private float _endTime;
    private Vector3 _gravity;

    public void SetTemporaryGravity(Vector3 gravity, float duration)
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        _rb.useGravity = false;

        _endTime = Time.time + duration;
        _gravity = gravity;

        this.enabled = true;
    }


    private void FixedUpdate()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        if (_rb == null)
        {
            this.enabled = false;
            return;
        }

        if (Time.time > _endTime)
        {
            this.enabled = false;
            _rb.useGravity = true;
            return;
        }

        _rb.AddForce(_gravity, ForceMode.Acceleration);
    }
}
