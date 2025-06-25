using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ReportCarPosition : MonoBehaviour 
{
    StreamWriter sw;

    void Start()
    {
        //sw = File.CreateText(Path.Combine(Application.dataPath, "pathData.csv"));
        //sw.WriteLine("Time,Pos,,,Rot,,,Velocity");
        //sw.WriteLine("sec,x,y,z,x,y,z,mps");
    }
    void OnTriggerEnter(Collider col)
    {
        
        Debug.Log("Car Detected @ " + gameObject.name + ": " + col.transform.position + "(pos), " + col.transform.rotation.eulerAngles + "(rot)");
        Rigidbody rb = col.gameObject.GetComponentInParent<Rigidbody>();
        if(rb != null)
        {
            Debug.Log("Car velocity: " + rb.velocity.magnitude);
        }
        //sw.WriteLine(Time.time + "," + col.transform.position.x + "," + col.transform.position.y + "," + col.transform.position.z + "," + col.transform.rotation.eulerAngles.x + "," + col.transform.rotation.eulerAngles.y + "," + col.transform.rotation.eulerAngles.z + "," + rb.velocity.magnitude);
    }

    void OnDisable()
    {
        //sw.Close();
    }
}