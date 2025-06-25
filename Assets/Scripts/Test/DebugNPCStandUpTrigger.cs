using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DebugNPCStandUpTrigger : MonoBehaviour
{
    public Animator Animator;

    public bool ShowNPCVelocity = false;

    private Vector3 _priorPosition;

    private void Start()
    {
        _priorPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F12))
        {
            Animator.SetTrigger("StandUp");
        }

        if (ShowNPCVelocity)
        {
            var vel = (transform.position - _priorPosition)/Time.deltaTime;
            _priorPosition = transform.position;
            Debug.Log($"Walking speed: {vel.magnitude} m/s");
        }
    }
}
