using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour {

    public Rigidbody rb;
    //public SteamVR_TrackedObject trackedObject;
    //public SteamVR_Controller.Device control;
    private bool _throwing = false;
    
    public void Pickup(ActorHost requester, Transform handTransform)
    {
        transform.parent = handTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0,180,0);
        _throwing = false;
        rb.isKinematic = true;
    }

    public void Drop()
    {
        transform.parent = null;
        rb.isKinematic = false;
        _throwing = true;
    }

    private void FixedUpdate()
    {
        //if (_throwing)
        //{
        //	Transform origin;
        //	if(trackedObject.origin != null)
        //	{
        //		origin = trackedObject.origin;
        //	}
        //	else
        //	{
        //		origin = trackedObject.transform.parent;
        //	}

        //	if(origin != null)
        //	{
        //		rb.velocity = origin.TransformVector(control.velocity);
        //	}
        //}
    }
}
