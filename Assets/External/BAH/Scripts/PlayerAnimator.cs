//using Oculus.Platform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

namespace BAHDOL
{

    public class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("The Attached Player's animator")]
        public Animator playerAnimator;

        //[Tooltip("Reference to Player's SimpleCapsuleWithStickMovement script for movement data")]
        //public VrSimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement;

        [Tooltip("PLayer's velocity")]
        public Vector3 velocity;

        public VRIK VRIKReference;
        public float CrouchThreshold = 1f;
        public float PlayerScale = 1f;
        public bool IsDebug = false;
        public float FilterValue = 0.2f;
        public float FilterValueX = 0.2f;
        public float DampZeroVal = 0.1f;
        public float DampTimeVal = 0.1f;
        private Vector3 CurrentPosition;
        private Vector3 PreviousPosition;
        //private Vector3 _smoothingVelocity = Vector3.zero;
        private float _smoothTime = 0.1f;
        private Vector3 _priorVelocity = Vector3.zero;
        private float _xVel = 0;
        private float _priorXVal = 0;
        private float _zVel = 0;
        private float _priorZVal = 0;
        private float _xAvg = 0;
        private float _zAvg = 0;

        private Queue<float> _zQue;
        private Queue<float> _xQue;
        private int _count = 0;
        private Rigidbody _rb;

        [Tooltip("Player Head reference for computing position")]
        public Transform HeadTransform;

        [Tooltip("Sensitivity multiplier used for VR based locomotion animation movement")]
        public float VelocityAnimatorSensitivity = 5f;

        [Tooltip("Sensitivity multiplier used for Controller based locomtion animation movement")]
        public float PlayerControledVelocityAnimatorSensitivity = 1f;

        public float DampingSpeed = 1;
        public void Start()
        {
            playerAnimator = gameObject.GetComponent<Animator>();
            PreviousPosition = transform.position;
            //PreviousPosition = HeadTransform.position;
            if(VRIKReference == null)
            {
                VRIKReference = gameObject.GetComponent<VRIK>();
            }
            velocity = Vector3.zero;
            _rb = GetComponent<Rigidbody>();
            //VRIKReference.solver.OnPreUpdate += PerformUpdates;
            _xQue = new Queue<float>();
            _zQue = new Queue<float>();
            for(int i = 0; i < 10; i++)
            {
                _xQue.Enqueue(0);
                _zQue.Enqueue(0);
            }
        }

        //public void Update()
        //{
        //    GetVelocity();
        //    UpdateAnimator();
        //}

        private void Update()
        {
            GetVelocity();
            UpdateAnimator();
        }

        //public void FixedUpdate()
        //{
        //    GetVelocity();
        //}

        //we're calculationg the veolocity so we can drive the animation with it.
        public void GetVelocity()
        {
            CurrentPosition = transform.position;
            //CurrentPosition = HeadTransform.position;//May need to do smoothing
            //CurrentPosition = Vector3.SmoothDamp(PreviousPosition, HeadTransform.position, ref _smoothingVelocity, _smoothTime);
            _priorVelocity = velocity;
            velocity = (CurrentPosition - PreviousPosition) / Time.deltaTime;
            PreviousPosition = CurrentPosition;
            //PreviousPosition = Vector3.SmoothDamp(PreviousPosition, CurrentPosition, ref _smoothingVelocity, _smoothTime);
            //_smoothingVelocity = transform.InverseTransformDirection(_smoothingVelocity) * VelocityAnimatorSensitivity;
            //Debug.Log(velocity + " vel, " + velocity.magnitude + " mag");
            //PreviousPosition = CurrentPosition;
            //velocity = transform.InverseTransformDirection(_smoothingVelocity) * VelocityAnimatorSensitivity;
            velocity = transform.InverseTransformDirection(velocity) * VelocityAnimatorSensitivity;

            //Debug.Log($"Rigidbody velocity: {_rb.velocity}");
            //Debug.Log(velocity + " vel update?");
            //Debug.Log($"Velocity is {velocity}");
            // if we using the scront stick to move then use the player controller animation multiplier.
            //else use the base multiplier for VR
            //if (simpleCapsuleWithStickMovement.primaryAxis.magnitude > .1f)
            //{
            //    velocity *= PlayerControledVelocityAnimatorSensitivity;
            //}
            //else
            //{
            //    velocity *= VelocityAnimatorSensitivity;
            //}
            //velocity *= VelocityAnimatorSensitivity;
        }

        //we're updating the variables for the locomotion animations
        public void UpdateAnimator()
        {
            //if (Mathf.Abs(velocity.x) > 0.1f)
            //{
            //    playerAnimator.SetFloat("Lateral", velocity.x, .1f, Time.deltaTime);
            //}
            //else
            //{
            //    playerAnimator.SetFloat("Lateral", 0, 0.1f, Time.deltaTime);
            //}
            //if (Mathf.Abs(velocity.y) > 0.0001f)
            //{
            //    playerAnimator.SetFloat("Forward", velocity.z, .1f, Time.deltaTime);
            //}
            //else
            //{
            //    playerAnimator.SetFloat("Forward", 0, .1f, Time.deltaTime);
            //}
            //playerAnimator.SetFloat("Lateral", velocity.x, .1f, Time.deltaTime);

            //if (Mathf.Abs(velocity.z) > 0.05f)
            //    playerAnimator.SetFloat("Forward", velocity.z, .1f, Time.deltaTime);
            //else
            //    playerAnimator.SetFloat("Forward", 0, 0.1f, Time.deltaTime);

            _zQue.Dequeue();
            _zQue.Enqueue(velocity.z);
            _xQue.Dequeue();
            _xQue.Enqueue(velocity.x);

            
            float zavg = 0;
            float xavg = 0;
            foreach(float num in _zQue)
            {
                zavg += num;
            }
            zavg = zavg / _zQue.Count;
            foreach (float num in _xQue)
            {
                xavg += num;
            }
            xavg = xavg / _xQue.Count;

            //Debug.Log($"Avg Velocity {zavg:F1} {xavg:F1} Counts {_zQue.Count} {_zQue.Count}");

            if (IsDebug)
            {
                Debug.Log("Avg velocity: " + zavg);
            }
            
            if (HeadTransform != null)
                playerAnimator.SetFloat("HeadHeight", (HeadTransform.position.y / PlayerScale), 0.1f, Time.deltaTime);
            

            //if (velocity.z >= 4)
            //{
            //    playerAnimator.SetFloat("Forward", 5);
            //    playerAnimator.speed = Mathf.Abs(velocity.z) / 5f;
            //}
            //else if (velocity.z > 0.5f)
            //{
            //    playerAnimator.SetFloat("Forward", 2);
            //    playerAnimator.speed = Mathf.Abs(velocity.z) / 2.5f;
            //}
            //else if (velocity.z < -0.5f)
            //{
            //    playerAnimator.SetFloat("Forward", -2);
            //    playerAnimator.speed = Mathf.Abs(velocity.z) / 2.5f;
            //}
            //else
            //{
            //    playerAnimator.SetFloat("Forward", 0, 0.1f, Time.fixedDeltaTime);
            //}
            if(Mathf.Abs(zavg) > FilterValue)
                playerAnimator.SetFloat("Forward", zavg, DampTimeVal, Time.deltaTime);
            else
            {
                playerAnimator.SetFloat("Forward", 0, DampZeroVal, Time.deltaTime);
            }
            if(Mathf.Abs(xavg) > FilterValueX)
            {
                playerAnimator.SetFloat("Lateral", xavg, DampTimeVal, Time.deltaTime);
            }
            else
            {
                playerAnimator.SetFloat("Lateral", 0, DampZeroVal, Time.deltaTime);
            }
            //if (HeadTransform.position.y > 1.2)
            //{

            //    if (Mathf.Abs(_zAvg) > FilterValue)
            //    {
            //        if (_zAvg >= 4)
            //        {
            //            playerAnimator.SetFloat("Forward", 5, 0.1f, Time.fixedDeltaTime);
            //        }
            //        else if (_zAvg > 0.2f)
            //        {
            //            playerAnimator.SetFloat("Forward", 2, 0.1f, Time.fixedDeltaTime);
            //        }
            //        else if (_zAvg < -0.2f)
            //        {
            //            playerAnimator.SetFloat("Forward", -2, 0.1f, Time.fixedDeltaTime);
            //        }
            //        else
            //        {
            //            playerAnimator.SetFloat("Forward", 0, 0.1f, Time.fixedDeltaTime);
            //        }
            //        playerAnimator.speed = Mathf.Abs(velocity.z) / 2.5f;
            //    }
            //    else
            //    {
            //        playerAnimator.SetFloat("Forward", 0, 0.1f, Time.fixedDeltaTime);
            //    }
            //}
            //else
            //{
            //    if(Mathf.Abs(_zAvg) > FilterValue)
            //    {                    
            //        playerAnimator.SetFloat("Forward", 2, 0.1f, Time.fixedDeltaTime);
            //    }
            //    else
            //    {
            //        playerAnimator.SetFloat("Forward", 0, 0.1f, Time.fixedDeltaTime);
            //    }
            //}
            //if (Mathf.Abs(velocity.x) > FilterValueX)
            //{
            //    //playerAnimator.SetFloat("Lateral", xVal, 0.1f, Time.deltaTime);
            //    playerAnimator.SetFloat("Lateral", velocity.x, 0.1f, Time.fixedDeltaTime);
            //}
            //else
            //{
            //    playerAnimator.SetFloat("Lateral", 0, 0.1f, Time.fixedDeltaTime);
            //}
            //if(HeadTransform.position.y < CrouchThreshold)
            //{
            //    playerAnimator.SetBool("isCrouched", true);
            //}
            //else
            //{
            //    playerAnimator.SetBool("isCrouched", false);
            //}


        }
    }
}
