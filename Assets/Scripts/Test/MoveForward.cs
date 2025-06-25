using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveForward : MonoBehaviour
{
    public Vector3 Velocity = new Vector3(0,0,5);

    private Rigidbody _rb;
    private Animator _anim;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();

        if (_anim != null)
        {
            _anim.SetInteger("Move", 2);
        }
    }

    void FixedUpdate()
    {
        _rb.AddRelativeForce(Velocity, ForceMode.VelocityChange);

        
        if (_anim != null)
        {
            
        }
    }
}
