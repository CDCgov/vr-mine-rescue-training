using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RootMotion;
using RootMotion.FinalIK;
using System.Dynamic;
//using Oculus.Avatar;
//using Oculus.Platform.Samples.VrHoops;

namespace BAHDOL
{
	public class VrSimpleCapsuleWithStickMovement : MonoBehaviour
	{
		[Tooltip("Toggle being able to use stick for movement")]
		public bool EnableLinearMovement = true;

		[Tooltip("Toggle ability to use controller to rotate player")]
		public bool EnableRotation = true;

		[Tooltip("Toggles ability to let player rotate based off of helmet movement")]
		public bool HMDRotatesPlayer = true;

		[Tooltip("Toggles ability to use both thumbsticks instead of just one")]
		public bool RotationEitherThumbstick = false;

		[Tooltip("If Enable rotation is on, this value determines the angle at which we can rotate. ")]
		public float RotationAngle = 45.0f;

		public float Speed = 0.0f;

		//[Tooltip("Reference to VR camera rig ")]
		//public OVRCameraRig CameraRig;

		[Tooltip("Player's velocity ")]
		public Vector3 Velocity;

		private bool ReadyToSnapTurn;
		private Rigidbody _rigidbody;

		[Tooltip("Reference to player's VRIK component.")]
		public VRIK vRIK;

		public event Action CameraUpdated;
		public event Action PreCharacterMove;

		[Tooltip("Referneced used to handle VRcontroller actions ")]
		public VrHandController vrHandController;

		[Tooltip("This value is used to determin the threshold at which the hand velocity will determine if the player will rotate via the headset" +
			"If the velocity magnitude of the vr hands is below the threshold, the player will not rotate via the HMD. So essentially if we keep the hands still we can shake our heads without moving the body")]
		public float vrHandVelocityThreshold = .1f;

		[Tooltip("The player mesh which is seperate from this transform")]
		public Transform PlayerMesh;

		[Tooltip("We are lerping the player mesh to this transform.position. this value controls the speed ")]
		public float PlayerMeshPositioLerpSpeed = 10f;
		public CapsuleCollider capsuleCollider;


		private float RightHandDelta;
		private float LeftHandDelta;
		public float HandMovementThreshold = .1f;

		[Tooltip(" Tells us if we are using the stick ")]
		public bool isUsingStickmovement;
		public Vector2 primaryAxis;
		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			//if (CameraRig == null) CameraRig = GetComponentInChildren<OVRCameraRig>();
			capsuleCollider = GetComponentInChildren<CapsuleCollider>();

		}

		private void AfterVRIK()
		{
		}

		void Start()
		{

		}

		private void Update()
		{

		}

		//used to move player mesg to current position per frame
		public void UpdatePlayerMeshTransform()
		{
			//Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

			if (vrHandController.LeftHandVelocity > vrHandVelocityThreshold || vrHandController.RightHandVelocity > vrHandVelocityThreshold || primaryAxis.magnitude > .1f)
			{
				Vector3 CurrentPos = PlayerMesh.position;
				//Vector3 TargetPos = new Vector3(CameraRig.centerEyeAnchor.position.x, transform.position.y - (capsuleCollider.height / 2), CameraRig.centerEyeAnchor.position.z);
				//Vector3 PlayerMeshPos = Vector3.Lerp(CurrentPos, TargetPos, PlayerMeshPositioLerpSpeed * Time.deltaTime);
				//PlayerMesh.position = PlayerMeshPos;
			}
		}

		private void FixedUpdate()
		{

			if (CameraUpdated != null) CameraUpdated();
			if (PreCharacterMove != null) PreCharacterMove();
			if (HMDRotatesPlayer) RotatePlayerToHMD();
			if (EnableLinearMovement) StickMovement();
			if (EnableRotation) SnapTurn();
			Velocity = _rigidbody.velocity;

			UpdatePlayerMeshTransform();
		}


		void RotatePlayerToHMD()
		{
			//Transform root = CameraRig.trackingSpace;
			//Transform centerEye = CameraRig.centerEyeAnchor;

			//Vector3 prevPos = root.position;
			//Quaternion prevRot = root.rotation;



			//root.position = prevPos;


			//if (LeftHandDelta > HandMovementThreshold || RightHandDelta > HandMovementThreshold)
			if (vrHandController.LeftHandVelocity > vrHandVelocityThreshold || vrHandController.RightHandVelocity > vrHandVelocityThreshold)
			{
				//transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);
				//root.rotation = prevRot;
			}

		}




		void StickMovement()
		{
			//Quaternion ort = CameraRig.centerEyeAnchor.rotation;
			//Vector3 ortEuler = ort.eulerAngles;
			//ortEuler.z = ortEuler.x = 0f;
			//ort = Quaternion.Euler(ortEuler);

			Vector3 moveDir = Vector3.zero;
			//primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
			if (primaryAxis.magnitude > .1f)
			{
				//moveDir += ort * (primaryAxis.x * Vector3.right);
				//moveDir += ort * (primaryAxis.y * Vector3.forward);
				//_rigidbody.MovePosition(_rigidbody.transform.position + moveDir * Speed * Time.fixedDeltaTime);
				_rigidbody.MovePosition(_rigidbody.position + moveDir * Speed * Time.fixedDeltaTime);
			}
			else
			{
				//TrackHeadMovement();
			}




		}

		public void TrackHeadMovement()
		{
			//_rigidbody.MovePosition(new Vector3(CameraRig.centerEyeAnchor.transform.position.x, _rigidbody.position.y, CameraRig.centerEyeAnchor.transform.position.z));
		}


		//Function used to rotate the player via the controller
		void SnapTurn()
		{
			Vector3 euler = transform.rotation.eulerAngles;

			//if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) ||
			//	(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft)))
			//{
			//	if (ReadyToSnapTurn)
			//	{
			//		euler.y -= RotationAngle;
			//		ReadyToSnapTurn = false;
			//	}
			//}
			//else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) ||
			//	(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight)))
			//{
			//	if (ReadyToSnapTurn)
			//	{
			//		euler.y += RotationAngle;
			//		ReadyToSnapTurn = false;
			//	}
			//}
			//else
			//{
			//	ReadyToSnapTurn = true;
			//}

			transform.rotation = Quaternion.Euler(euler);
		}
	}
}

