using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatPathAiController : MonoBehaviour {

    //public UnityStandardAssets.Utility.WaypointCircuit ForwardCircuit;
    //public UnityStandardAssets.Utility.WaypointCircuit ReverseCircuit;
    public Transform StartPoint;
    public Transform EndPoint;
    public int StartingWaypointIndex = 0;
    
    public bool MovingForward = true;
    //public UnityStandardAssets.Vehicles.Car.CarAIControl Car;
    public Transform VehicleMesh;
    public GameObject ForwardLights;
    public GameObject ReverseLights;

    public PauseShuttleCar PauseCarRef;

    public bool SimplifiedReverse;

    public WheelCollider[] WheelColliders;
    public Transform[] WheelMeshes;

    //private UnityStandardAssets.Utility.WaypointProgressTracker Progress;
    private bool _switch = false;
    private Transform _Target;



    private float Delay = 1;
    // Use this for initialization
    void Awake () {
        //Car = GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
        //Progress = GetComponent<UnityStandardAssets.Utility.WaypointProgressTracker>();
        //if (MovingForward)
        //{
        //    _Target = EndPoint;
        //    Progress.SetProgress(StartingWaypointIndex);
        //}
        //else
        //{
        //    _Target = StartPoint;
        //    Progress.SetProgress(StartingWaypointIndex);
        //}   
    }
    
    // Update is called once per frame
    void Update () {

        //if (!_switch && Vector3.Distance(Car.transform.position, _Target.position) < 2f)
        //{
        //    Car.SetDrivingStatus(false);
        //    _switch = true;
        //    //Debug.Log("Should have stopped!");
            
        //    MovingForward = !MovingForward;
        //    if (MovingForward)
        //    {
        //        _Target = EndPoint;
        //        Delay = Time.time + 1;
        //        if (PauseCarRef != null)
        //        {
        //            PauseCarRef.EmptyCar = true;
        //        }
        //    }
        //    else
        //    {
        //        _Target = StartPoint;
        //        Delay = Time.time + 10;
        //        if (PauseCarRef != null)
        //        {
        //            PauseCarRef.EmptyCar = false;
        //        }
        //    }
        //}

        //if (_switch)
        //{
        //    if (Time.time > Delay)
        //    {
        //        Car.enabled = false;
        //        Progress.enabled = false;
        //        if (!SimplifiedReverse)
        //        {
        //            transform.Rotate(0, 180, 0);
        //        }
        //        else
        //        {

        //        }
        //        if (MovingForward)
        //        {
        //            Progress.SetCircuit(ForwardCircuit);
        //            //Progress.Reset();
        //            Progress.SetProgress(0);
        //            Car.Circuit = ForwardCircuit;
        //            if (!SimplifiedReverse)
        //            {
        //                VehicleMesh.localPosition = new Vector3(1.01f, 0.6060616f, 1.35f);
        //                Quaternion rot = VehicleMesh.localRotation;
        //                rot.eulerAngles = new Vector3(0, 0, 0);
        //                VehicleMesh.localRotation = rot;
        //            }
        //            else
        //            {
        //                //foreach(WheelCollider col in WheelColliders)
        //                //{
        //                //    col.transform.Rotate(0, 180, 0);
        //                //}
        //            }
        //            ForwardLights.SetActive(true);
        //            ReverseLights.SetActive(false);
        //        }
        //        else
        //        {
        //            Progress.SetCircuit(ReverseCircuit);
        //            Progress.SetProgress(0);
        //            //Progress.Reset();
        //            Car.Circuit = ReverseCircuit;
        //            if (!SimplifiedReverse)
        //            {
        //                VehicleMesh.localPosition = new Vector3(-1, 0.6060616f, -1.162f);
        //                Quaternion rot = VehicleMesh.localRotation;
        //                rot.eulerAngles = new Vector3(0, 180, 0);
        //                VehicleMesh.localRotation = rot;
        //            }
        //            else
        //            {
        //                //foreach (WheelCollider col in WheelColliders)
        //                //{
        //                //    col.transform.Rotate(0, 180, 0);
        //                //}
        //            }
        //            ForwardLights.SetActive(false);
        //            ReverseLights.SetActive(true);
        //        }

        //        Car.SetDrivingStatus(true);
        //        Car.enabled = true;
        //        //Progress.Reset();
        //        Progress.enabled = true;
        //        _switch = false;
        //    }
        //}        
    }
}
