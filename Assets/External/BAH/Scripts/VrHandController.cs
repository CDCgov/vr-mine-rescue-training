using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using OVRTouchSample;

namespace BAHDOL
{
    public class VrHandController : MonoBehaviour
    {

        //[SerializeField]
        //private OVRInput.Controller L_controller = OVRInput.Controller.LTouch;
        //[SerializeField]
        //private OVRInput.Controller R_controller = OVRInput.Controller.RTouch;
        public float LHandGrip;
        public float RHandGrip;

        public Animator playerAnimator;

        public bool LeftHandClosed;
        public bool RightHandClosed;

        public Transform RightHandTransform;
        public Transform LeftHandTransform;

        private Vector3 LeftHandLastPos;
        private Vector3 RightHandLastPos;
        private Vector3 LeftHandCurPos;
        private Vector3 RightHandCurPos;

        public float LeftHandVelocity;
        public float RightHandVelocity;

        // Start is called before the first frame update
        void Start()
        {
            SetHandState();
            LeftHandLastPos = LeftHandTransform.position;
            RightHandLastPos = RightHandTransform.position;
        }

        // Update is called once per frame
        void Update()
        {
            //SetHandState();
            SetAnimatorHandState();
        }

        public void LateUpdate()
        {
            GetHandVelocity();
        }

        public void SetHandState()
        {
            //float LHandGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, L_controller);
            //float RHandGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, R_controller);

            if (LHandGrip > 0f)
            {
                LeftHandClosed = true;
            }
            else
            {
                LeftHandClosed = false;
            }

            if (RHandGrip > 0f)
            {
                RightHandClosed = true;

            }
            else
            {
                RightHandClosed = false;
            }
        }

        public void GetHandVelocity()
        {
            LeftHandCurPos = (LeftHandTransform.position - LeftHandLastPos) / Time.deltaTime;
            RightHandCurPos = (RightHandTransform.position - RightHandLastPos) / Time.deltaTime;

            LeftHandVelocity = LeftHandCurPos.magnitude;
            RightHandVelocity = RightHandCurPos.magnitude;

            LeftHandLastPos = LeftHandTransform.position;
            RightHandLastPos = RightHandTransform.position;
        }

        public void SetAnimatorHandState()
        {
            playerAnimator.SetBool("isLeftHandClosed", LeftHandClosed);
            playerAnimator.SetBool("isRightHandClosed", RightHandClosed);
        }
    }
}
