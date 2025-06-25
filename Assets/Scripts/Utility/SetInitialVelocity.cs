using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SetInitialVelocity : MonoBehaviour
{
    public Vector3 InitialVelocity = Vector3.zero;
    public Vector3 InitialAngularVelocity = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.velocity = transform.TransformDirection(InitialVelocity);
        rb.angularVelocity = transform.TransformDirection(InitialAngularVelocity);
    } 

}
