using UnityEngine;
using System.Collections.Generic;

public class ProxGenericMachineStateControl : MonoBehaviour
{
    public string NormalState = "Default";
    public string MovingState = "Moving";

    //use two thresholds to add stability / hysteresis to the system

    //transition from moving to normal below this speed
    public float MovingMinSpeed = 1.5f;

    //transition from normal to moving above this speed
    public float NormalMaxSpeed = 1.8f;

    private Rigidbody _rb;
    private ProxSystemController _proxController;

    private string _currentState;

    // Use this for initialization
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _proxController = GetComponent<ProxSystemController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_rb == null || _proxController == null)
            return;

        float speed = _rb.velocity.magnitude;

        if (speed > NormalMaxSpeed && _currentState != MovingState)
        {
            _proxController.SetMachineState(MovingState);
            _currentState = MovingState;
        }
        else if (speed < MovingMinSpeed && _currentState != NormalState)
        {
            _proxController.SetMachineState(NormalState);
            _currentState = NormalState;
        }

    }
}
