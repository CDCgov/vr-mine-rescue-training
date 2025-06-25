using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BackAndForthVehicle : MonoBehaviour {

	public Vector3 StartPoint;
	public Vector3 EndPoint;
	//public UnityStandardAssets.Vehicles.Car.CarController CControl;

	public bool MovingForward;
	public LightController LControl;
	public Light[] FrontLights;
	public Light[] RearLights;

	private Flare frontFlareRef;
	private Flare rearFlareRef;
	private bool FlaresEnabled = true;
	// Use this for initialization
	void Start () {
		//CControl.Move(0, 1, 0, 0);
		//LControl.SetLight(MovingForward);
		//frontFlareRef = FrontLights[0].flare;
		//rearFlareRef = RearLights[0].flare;
	}
	
	// Update is called once per frame
	void Update () {
		//if (Input.GetKeyUp(KeyCode.F))
		//{            
		//	for(int i = 0; i < FrontLights.Length; i++)
		//	{
		//		FrontLights[i].flare = null;
		//	}
		//	for (int i = 0; i < RearLights.Length; i++)
		//	{
		//		RearLights[i].flare = null;
		//	}            
		//}
		if (MovingForward)
		{
			if(Vector3.Distance(transform.position,EndPoint) > 0.1f)
			{
				//transform.Translate(0, 0, Time.deltaTime);
				transform.position = Vector3.MoveTowards(transform.position, EndPoint, Time.deltaTime * 2);
			}
			else
			{
				//CControl.Move(0, 0, 1, 0);
				Rigidbody rb = GetComponent<Rigidbody>();
				if(rb != null)
				{
					Debug.Log("Stopped at end?");
					//rb.velocity = Vector3.zero;
									 
				}
				MovingForward = false;
				//LControl.SetLight(false);
			}
		}
		else
		{
			if (Vector3.Distance(transform.position, StartPoint) > 0.1f)
			{
				//CControl.Move(0, -1, 0, 0);
				//transform.Translate(0, 0, -Time.deltaTime);
				transform.position = Vector3.MoveTowards(transform.position, StartPoint, Time.deltaTime * 2);
			}
			else
			{
				//CControl.Move(0, 0, 1, 0);
				Debug.Log("Stopped at start?");
				Rigidbody rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					//rb.velocity = Vector3.zero;
									 
				}
				MovingForward = true;
				//LControl.SetLight(true);
			}
		}
	}
}
