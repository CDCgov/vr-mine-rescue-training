using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetSyncServerConstraint : MonoBehaviour
{
    public abstract void GetConstrainedMotion(ref Vector3 constrainedPosition, ref Quaternion constrainedRotation);
}
