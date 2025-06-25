using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabBehavior : MonoBehaviour {
    public Transform SnapPoint;
    public virtual void Grabbed()
    {
        if(SnapPoint != null)
        {
            Vector3 positionAdjust = SnapPoint.localPosition;
        }
        return;
    }
    public virtual void Released()
    {
        return;
    }
}
