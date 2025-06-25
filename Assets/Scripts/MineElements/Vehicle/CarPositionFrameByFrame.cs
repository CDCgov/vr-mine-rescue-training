using UnityEngine;
using System.Collections;
using System.IO;

public class CarPositionFrameByFrame : MonoBehaviour {

    StreamWriter sw;
    Rigidbody rb;

    float reportTime = 5;

    //public UnityStandardAssets.Vehicles.Car.CarAIControl ai;
    
    // Use this for initialization
    void Start () {
        sw = File.CreateText(Path.Combine(Application.dataPath, "frameByFrame.csv"));
        sw.WriteLine("Time,DeltaTime,Frame,Pos,,,Rot,,,Velocity");
        sw.WriteLine("sec,sec,frame,x,y,z,x,y,z,mps");

        reportTime = 0;

        rb = gameObject.GetComponent<Rigidbody>();
        //ai = GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
    }

    void Update()
    {
        if(Time.time >= 5)
        {
            //ai.enabled = true;
        }
        if (Time.time >= reportTime)
        {
            sw.WriteLine(Time.time + "," + Time.deltaTime + "," + Time.frameCount + "," + transform.position.x + "," + transform.position.y + "," + transform.position.z + "," + transform.rotation.eulerAngles.x + "," + transform.rotation.eulerAngles.y + "," + transform.rotation.eulerAngles.z + "," + rb.velocity.magnitude);
            reportTime += 0.5f;
        }
    }

    void OnDisable()
    {
        sw.Close();
    }
}
