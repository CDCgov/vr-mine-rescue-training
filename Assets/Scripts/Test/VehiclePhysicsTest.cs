using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
//using Sirenix.OdinInspector;

//[RequireComponent(typeof(CarController))]
public class VehiclePhysicsTest : MonoBehaviour
{

	public bool MonitorOnly = false;

	[Range(0,1)]
	public float BrakePercent;
	[Range(0, 1)]
	public float AccelPercent;

	public float ExtremumSlip = 0.1f;
	public float ExtremumValue =  0.35f;
	public float AsymptoteSlip = 0.3f;
	public float AsymptoteValue = 0.45f;
	public float Stiffness = 1;


	// [Title("Status", bold: false)]
	// [HideLabel]
	// [MultiLineProperty(25)]
	public string StatusInfo; 

	//private CarController _car;
	private Rigidbody _rb;

	private Vector3 _startPos;
	private Quaternion _startRot;

	//[Button()]
	private void ResetPosition()
	{
		transform.position = _startPos;
		transform.rotation = _startRot;
	}

	//[Button()]
	private void SetVelocity()
	{
		_rb.velocity = transform.forward * 5;
	}

	// Use this for initialization
	void Start()
	{
		//_car = GetComponent<CarController>();
		_rb = GetComponent<Rigidbody>();

		_startPos = transform.position;
		_startRot = transform.rotation;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		//var wheels = _car.GetWheelColliders();

		if (!MonitorOnly)
		{

			//for (int i = 0; i < wheels.Length; i++)
			//{
			//	WheelCollider wc = wheels[i];

			//	WheelFrictionCurve wfc = wc.forwardFriction;
			//	wfc.extremumSlip = ExtremumSlip;
			//	wfc.extremumValue = ExtremumValue;
			//	wfc.asymptoteSlip = AsymptoteSlip;
			//	wfc.asymptoteValue = AsymptoteValue;
			//	wc.forwardFriction = wfc;
			//}

			//_car.ApplyDrive(AccelPercent, BrakePercent * -1);
		}

		StringBuilder sbStatus = new StringBuilder();		

		sbStatus.Append("BrakeTorque: ");
		//for (int i = 0; i < wheels.Length; i++)
		//{
		//	WheelCollider wc = wheels[i];
		//	sbStatus.AppendFormat("{0:F2} ", wc.brakeTorque);
		//}
		sbStatus.AppendLine();

		sbStatus.Append("MotorTorque: ");
		//for (int i = 0; i < wheels.Length; i++)
		//{
		//	WheelCollider wc = wheels[i];
		//	sbStatus.AppendFormat("{0:F2} ", wc.motorTorque);
		//}
		sbStatus.AppendLine();

		sbStatus.Append("WheelRPM: ");
		//for (int i = 0; i < wheels.Length; i++)
		//{
		//	WheelCollider wc = wheels[i];
		//	sbStatus.AppendFormat("{0:F2} ", wc.rpm);
			
		//}
		sbStatus.AppendLine();

		sbStatus.Append("ForwardSlip: ");
		//for (int i = 0; i < wheels.Length; i++)
		//{
		//	WheelCollider wc = wheels[i];			
		//	WheelHit wheelHit;
		//	wc.GetGroundHit(out wheelHit);

		//	sbStatus.AppendFormat("{0:F2} ", wheelHit.forwardSlip);

		//}
		sbStatus.AppendLine();
		

		sbStatus.AppendFormat("Velocity(m/s): {0:F2}", _rb.velocity.magnitude);

		StatusInfo = sbStatus.ToString();
	}
}
