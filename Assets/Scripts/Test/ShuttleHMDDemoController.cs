using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(UnityStandardAssets.Vehicles.Car.CarController))]
public class ShuttleHMDDemoController : MonoBehaviour {

	//public UnityStandardAssets.Vehicles.Car.CarController ShuttleController;
	public float StopDistance;
	public float BrakeThresholdConstant;

	public Transform FrontOfCarTransform;
	public Vector3 PlayerPosition;

	private bool _motionStarted = false;
	private bool _motionComplete = false;

	private void Start()
	{
		PlayerPosition = new Vector3(48, 0, -24);
	}

	// Update is called once per frame
	void Update () {
		if (!_motionComplete)
		{
			if (_motionStarted)
			{
				if (FrontOfCarTransform.position.z - PlayerPosition.z <= StopDistance + BrakeThresholdConstant)
				{
					Debug.Log(FrontOfCarTransform.position.z + ", POSITION : " + (FrontOfCarTransform.position.z - PlayerPosition.z) + ", diff");
					//ShuttleController.Move(0, 0, -1, 1);
					_motionComplete = true;
				}
				else
				{
					//ShuttleController.Move(0, 1, 0, 0);
				}
			}
			//else
			//{                
			//    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			//    {
			//        _motionStarted = true;
			//    }
			//}
		}
		else
		{
			//ShuttleController.Move(0, 0, -1, 1);
			Debug.Log("Braking?");
		}
	}

	public void StartMotion()
	{
		if (!_motionStarted)
		{
			_motionStarted = true;
		}
	}

	public bool GetMotionComplete()
	{
		return _motionComplete;
	}
}
