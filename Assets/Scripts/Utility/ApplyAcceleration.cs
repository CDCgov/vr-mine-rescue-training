using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyAcceleration : MonoBehaviour
{
    public bool DisableGravity = true;
    public Vector3 Acceleration = new Vector3(0, -9.8f, 0);

    private Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_rb == null)
        {
            this.enabled = false;
            return;
        }

        if (DisableGravity && _rb.useGravity)
            _rb.useGravity = false;

        _rb.AddForce(Acceleration, ForceMode.Acceleration);
    }
}
