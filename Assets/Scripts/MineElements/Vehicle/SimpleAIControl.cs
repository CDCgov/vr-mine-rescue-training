using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Car = UnityStandardAssets.Vehicles.Car;

public class SimpleAIControl : MonoBehaviour 
{
    
    //public UnityStandardAssets.Utility.WaypointCircuit Circuit;
      
    public float TurnValue; //-1 to 1 value for steering
    public float AccelValue = 1; //-1 to 1 value for acceleration
    public float BrakeValue = 0; //0 to 1 brake value, usually not used in conjunction with acceleration I'd say.

    
    void OnTriggerEnter(Collider col)
    {
        //Car.CarController myCar = col.gameObject.GetComponent<Car.CarController>();
        
        //if(myCar != null)
        //{
        //    float maxAngle = myCar.GetMaxTurnAngle();
        //    int thisPointIndex = transform.GetSiblingIndex();
        //    int nextPoint = Circuit.MovingForward ? (thisPointIndex+1) : (thisPointIndex-1);
            
        //    if(nextPoint == transform.parent.childCount || nextPoint < 0)
        //    {
        //        myCar.ContinuousMove(0, 0, -1, 1, true);//Start the return trip later, set a value on the repeat path ai controller?
        //        return;
        //    }
        //    float angle;
        //    if (Circuit.MovingForward)
        //    {
        //        Debug.Log(transform.name + ", " + transform.parent.GetChild(nextPoint).name);
        //        angle = Mathf.Atan2(transform.position.z - transform.parent.GetChild(nextPoint).position.z, transform.position.x - transform.parent.GetChild(nextPoint).position.x) * 180 / Mathf.PI;
        //        angle = angle / 8;
        //    }
        //    else
        //    {
        //        angle = Vector3.Angle(transform.forward*-1, transform.parent.GetChild(nextPoint).position + transform.position);
        //        AccelValue = AccelValue * -1;
        //    }

        //    if(angle > 0)
        //    {
        //        TurnValue = angle / maxAngle;
        //    }
        //    else
        //    {
        //        TurnValue = angle / -maxAngle;
        //    }
        //    Debug.Log(angle);

        //    myCar.ContinuousMove(TurnValue, AccelValue, BrakeValue, 0, true);
        //}
    }
}