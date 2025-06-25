using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PauseShuttleCar : MonoBehaviour {

    
    //public UnityStandardAssets.Vehicles.Car.CarAIControl ai;
    public bool EmptyCar;
    public PauseShuttleCar OtherStopPoint;   
    public bool IgnoreFirstPass = false;
    public bool StopSign = false;

    private bool IsInterSectionClear = false;
    
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {

        //if (Input.GetKeyUp(KeyCode.KeypadEnter))
        //{
        //    ResumeShuttleCar();
        //}        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name + ", " + Vector3.Distance(transform.position, ai.transform.position));
        //ai = other.GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();   
        if (other.name.Contains("ShuttleCar"))
        {
            
            if (StopSign)
            {
                //ai.SetDrivingStatus(false);                
            }
            else
            {
                if (!EmptyCar)
                {
                    
                    if (OtherStopPoint.StopSign)
                    {
                        OtherStopPoint.ResumeShuttleCar();
                        StopSign = true;
                        //OtherStopPoint.StopSign = !OtherStopPoint.StopSign;
                    }
                    OtherStopPoint.StopSign = false;

                }
                else
                {
                    OtherStopPoint.StopSign = true;
                }
                //StopSign = true;
                //EmptyCar = true;                
            }
        }      
    }

    public void ResumeShuttleCar()
    {
        //ai.SetDrivingStatus(true);
        //EmptyCar = false;
    }
}
