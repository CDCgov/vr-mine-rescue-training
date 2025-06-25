using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSyncServerKinematicConstraint : NetSyncServerConstraint
{
    public override void GetConstrainedMotion(ref Vector3 constrainedPosition, ref Quaternion constrainedRotation)
    {
        //I do nothing on purpose.
    }

    
}
