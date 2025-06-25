using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Car = UnityStandardAssets.Vehicles.Car;

/// <summary>
/// Basic controller, used in a non-waypoint based AI Navigation
/// </summary>
public class SimpleAICarInitialCondition : MonoBehaviour 
{
    //public Car.CarController myCar;
    void Start () 
    {
        //myCar.ContinuousMove(0, 1, 0, 0, true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Debug.Log(rb.angularVelocity);
            //Vector3 dir = -gameObject.transform.right;
            Debug.Log(rb.velocity);
        }
    }

    public void Restart()
    {
        //myCar.ContinuousMove(0, 1, 0, 0, true);
        Debug.Log("Restart");
    }
}