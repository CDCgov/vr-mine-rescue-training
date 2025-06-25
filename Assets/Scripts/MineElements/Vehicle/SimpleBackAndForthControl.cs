using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBackAndForthControl : MonoBehaviour {

    public Transform StartPoint;
    public Transform EndPoint;
    public bool MovingForward = true;

    //public UnityStandardAssets.Vehicles.Car.CarController CarControllerScript;
    public float DelayTime = 2;
    public ProxSystemController ProxControl;
    private Rigidbody _rb;
    private float _timer = 0;
    private bool _stopTriggered = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate () {
        if (ProxControl.ActiveProxZone != ProxZone.RedZone)
        {
            float yellowMultiplier = 1;
            if (ProxControl.ActiveProxZone == ProxZone.YellowZone)
            {
                yellowMultiplier = 0.75f;
            }
            else
            {
                yellowMultiplier = 1;
            }
            if (MovingForward)
            {                
                if (!_stopTriggered)
                {
                    //CarControllerScript.Move(0, 1 * yellowMultiplier, 0, 0);
                    if (Vector3.Distance(transform.position, EndPoint.position) < 0.5f || transform.position.z <= EndPoint.position.z)
                    {
                        _stopTriggered = true;
                        _timer = Time.time + DelayTime;
                    }
                }
                else
                {
                    //CarControllerScript.Move(0, 0, -1, 1);
                    if (Time.time > _timer)
                    {
                        MovingForward = false;
                        //CarControllerScript.MovingReverse = true;
                        _stopTriggered = false;
                    }
                }

            }
            else
            {
                if (!_stopTriggered)
                {
                    //CarControllerScript.Move(0, -1 * yellowMultiplier, 0, 0);
                    if (Vector3.Distance(transform.position, StartPoint.position) < 0.5f)
                    {
                        _stopTriggered = true;
                        _timer = Time.time + DelayTime;
                    }
                }
                else
                {
                    //CarControllerScript.Move(0, 0, -1, 1);
                    if (Time.time > _timer)
                    {
                        MovingForward = true;
                        //CarControllerScript.MovingReverse = false;
                        _stopTriggered = false;
                    }
                }

            }
        }
        else
        {
            //CarControllerScript.Move(0, 0, -1, 1);
        }
    }

}
