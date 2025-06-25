using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

//[RequireComponent(typeof(CarController))]
public class CarKeyboardControl : MonoBehaviour
{
    //private CarController _carControl;
    // Start is called before the first frame update
    void Start()
    {
        //_carControl = GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        float steering = 0;
        float accel = 0;
        float braking = 0;

        if (Input.GetKey(KeyCode.W))
        {
            accel = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            accel = -1;
        }
        else
        {
            braking = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            steering = -15;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            steering = 15;
        }

        //_carControl.Move(steering, accel, braking, 0);
    }
}
