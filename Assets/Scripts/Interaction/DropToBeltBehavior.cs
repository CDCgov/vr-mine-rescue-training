using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom dropping behavior to have item snap to a position on the user's belt
/// </summary>
public class DropToBeltBehavior : DropBehavior
{
    public Vector3 BeltSnapPoint;
    public Quaternion BeltSnapRotation;

    private void Start()
    {
        
        BeltSnapPoint = transform.localPosition;
        BeltSnapRotation = transform.localRotation;
        
    }
    public override void OnDrop()
    {
        Debug.Log("On drop behavior called: " + BeltSnapPoint);
        ItemRigidbody.isKinematic = true;
        transform.localRotation = BeltSnapRotation;
        transform.localPosition = BeltSnapPoint;
    }
}
