using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRInputHandler : MonoBehaviour
{
    public ActorHost    PlayerActor;
    public ControllerHand Hand;

    public Vector3      HeadPosition;
    public Quaternion   HeadRotation;
    public Vector3      LeftHandPosition;
    public Quaternion   LeftHandRotation;
    public Vector3      LeftHandVelocity;
    public Vector3 LeftHandAcceleration;
    public Vector3      RightHandPosition;
    public Quaternion   RightHandRotation;
    public Vector3      RightHandVelocity;
    public Vector3 RightHandAcceleration;
    public float        RightTriggerAxis;
    public float        LeftTriggerAxis;
    public GameObject   HeldObjectRight;
    public GameObject   HeldObjectLeft;
    public FixedJoint   LeftFixedJoint;
    public FixedJoint   RightFixedJoint;
    public FixedJoint HandJoint;
    public GameObject HeldObject;

    public Transform RightHandTransform;
    public Transform LeftHandTransform;


    private float _priorRightTriggerAxis = 0;
    private float _priorLeftTriggerAxis = 0;
    private Vector3 _handPosition;
    private Vector3 _handVelocity;
    private float _triggerValue = 0;
    private float _priorTriggerValue = 0;
    private GameObject _selectedObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ////Hand postion
        //switch (Hand)
        //{
        //    case ControllerHand.LeftHand:
        //        GetLeftControllerData();
        //        LeftHandPosition = LeftHandTransform.position;
        //        _handPosition = LeftHandTransform.position;
        //        _handVelocity = LeftHandVelocity;
        //        _triggerValue = LeftTriggerAxis;
        //        break;
        //    case ControllerHand.RightHand:
        //        GetRightControllerData();
        //        RightHandPosition = RightHandTransform.position;
        //        _handPosition = RightHandTransform.position;
        //        _handVelocity = RightHandVelocity;
        //        _triggerValue = RightTriggerAxis;
        //        break;
        //    default:
        //        break;
        //}

        //if (_triggerValue > 0)
        //{
        //    if (_priorTriggerValue == 0)
        //    {
        //        Collider[] cols = Physics.OverlapSphere(_handPosition, 0.1f);
        //        foreach (Collider col in cols)
        //        {
        //            if (col.tag == "Pickupable")
        //            {
        //                HeldObjectRight = col.gameObject;
        //                Rigidbody rb = HeldObject.GetComponent<Rigidbody>();
        //                if (rb != null)
        //                {
        //                    HandJoint.connectedBody = rb;
        //                    Debug.Log("Grabbed " + col.gameObject.name);
        //                }
        //                else
        //                {
        //                    HeldObject = null;
        //                }
        //                break;
        //            }
        //            Interactable interact = col.gameObject.GetComponent<Interactable>();
        //            if (interact != null)
        //            {
        //                interact.Interact(PlayerActor);
        //            }
        //        }
        //    }
        //    _priorTriggerValue = _triggerValue;
        //}
        //else
        //{
        //    if (HeldObjectRight != null)
        //    {
        //        DropHeldItem(Hand);
        //    }
        //    _priorTriggerValue = 0;
        //}

        //if (RightTriggerAxis > 0)
        //{
        //    if (_priorRightTriggerAxis == 0)
        //    {
        //        Collider[] cols = Physics.OverlapSphere(RightHandPosition, 0.1f);
        //        foreach (Collider col in cols)
        //        {
        //            if (col.tag == "Pickupable")
        //            {
        //                HeldObjectRight = col.gameObject;
        //                Rigidbody rb = HeldObjectRight.GetComponent<Rigidbody>();
        //                if (rb != null)
        //                {
        //                    RightFixedJoint.connectedBody = rb;
        //                    Debug.Log("Grabbed " + col.gameObject.name);
        //                }
        //                else
        //                {
        //                    HeldObjectRight = null;
        //                }
        //                break;
        //            }
        //            Interactable interact = col.gameObject.GetComponent<Interactable>();
        //            if (interact != null)
        //            {
        //                interact.Interact(PlayerActor);
        //            }
        //        }
        //    }
        //    _priorRightTriggerAxis = RightTriggerAxis;
        //}
        //else
        //{
        //    if (HeldObjectRight != null)
        //    {
        //        DropHeldItem(ControllerHand.RightHand);
        //    }
        //    _priorRightTriggerAxis = 0;
        //}
        //if (LeftTriggerAxis > 0)
        //{
        //    if(_priorLeftTriggerAxis == 0) 
        //    {
        //        Collider[] cols = Physics.OverlapSphere(LeftHandPosition, 0.1f);
        //        foreach(Collider col in cols)
        //        {
        //            if(col.tag == "Pickupable")
        //            {
        //                HeldObjectLeft = col.gameObject;
        //                Rigidbody rb = HeldObjectLeft.GetComponent<Rigidbody>();
        //                if (rb != null)
        //                {
        //                    LeftFixedJoint.connectedBody = rb;
        //                }
        //                else
        //                {
        //                    HeldObjectLeft = null;
        //                }
        //                break;
        //            }
        //            Interactable interact = col.gameObject.GetComponent<Interactable>();
        //            if(interact != null)
        //            {
        //                interact.Interact(PlayerActor);
        //            }
        //        }
        //    }
        //    _priorLeftTriggerAxis = LeftTriggerAxis;
        //}
        //else
        //{
        //    if(HeldObjectLeft != null)
        //    {
        //        DropHeldItem(ControllerHand.LeftHand);
        //    }
        //    _priorLeftTriggerAxis = 0;
        //}
    }
    public virtual void OnRightTrigger()
    {

    }

    public virtual void OnLeftTrigger()
    {

    }

    void DropHeldItem(ControllerHand hand)
    {
        Debug.Log("Dropping held item");
        DropBehavior dropScript = null;
        dropScript = HeldObject.GetComponent<DropBehavior>();
        if(dropScript != null)
        {            
            dropScript.HandVelocity = transform.TransformVector(_handVelocity * 5);
            dropScript.OnDrop();
        }
        HandJoint.connectedBody = null;
        HeldObject = null;
        //switch (hand)
        //{
        //    case ControllerHand.LeftHand:
        //        //dropScript = HeldObjectLeft.GetComponent<DropBehavior>();
        //        if(dropScript != null)
        //        {
        //            dropScript.HandVelocity = LeftHandVelocity;
        //        }
        //        //LeftFixedJoint.connectedBody = null;
        //        HeldObjectLeft = null;
        //        break;
        //    case ControllerHand.RightHand:
        //        dropScript = HeldObjectRight.GetComponent<DropBehavior>();
        //        if (dropScript != null)
        //        {
        //            dropScript.HandVelocity = RightHandVelocity;
        //        }
        //        LeftFixedJoint.connectedBody = null;
        //        HeldObjectRight = null;
        //        break;
        //    default:
        //        break;
        //}
        //if (dropScript != null)
        //{
        //    dropScript.OnDrop();
        //}
        Debug.Log("Should have dropped?");
    }

    public virtual void GetLeftControllerData()
    {

    }

    public virtual void GetRightControllerData()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //Highlight behavior?
        Debug.Log("Entered: " + other.name);
        if(other.tag == "Pickupable")
        {
            _selectedObject = other.gameObject;
        }

        Interactable inter = other.gameObject.GetComponent<Interactable>();
        if(inter != null && _triggerValue > 0.1f)
        {
            inter.Interact();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(_selectedObject != null)
        {
            _selectedObject = null;
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log("In trigger! " + other.name + " " + _selectedObject.name);
        switch (Hand)
        {
            case ControllerHand.LeftHand:
                GetLeftControllerData();
                LeftHandPosition = LeftHandTransform.position;
                _handPosition = LeftHandTransform.position;
                _handVelocity = LeftHandVelocity;
                _triggerValue = LeftTriggerAxis;
                //Debug.Log("LTAxis: " + LeftTriggerAxis + ", LTVel: " + LeftHandVelocity);
                break;
            case ControllerHand.RightHand:
                GetRightControllerData();
                RightHandPosition = RightHandTransform.position;
                _handPosition = RightHandTransform.position;
                _handVelocity = RightHandVelocity;
                _triggerValue = RightTriggerAxis;
                break;
            default:
                break;
        }

        if (_triggerValue > 0.1f)
        {
            //if (_priorTriggerValue == 0)
            //{
            //    if(_selectedObject != null)
            //    {
            //        HeldObject = _selectedObject;
            //    }
            //}
            if(HeldObject == null && _selectedObject != null)
            {
                HeldObject = _selectedObject;
                Rigidbody rb = HeldObject.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    if (rb.isKinematic)
                    {
                        rb.isKinematic = false;
                    }
                    HandJoint.connectedBody = rb;
                }
            }
            //_priorTriggerValue = _triggerValue;
        }
        else
        {
            if (HeldObject != null)
            {
                DropHeldItem(Hand);
            }
            //_priorTriggerValue = 0;
        }
    }   
}
