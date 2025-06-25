using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSyncServerLinearConstraint : NetSyncServerConstraint
{
    public Vector3 Point1;
    public Vector3 Point2;
    public bool InfiniteLine = false;
    public bool ConstrainRotation = true;

    private float _lineLength = 0;

    public override void GetConstrainedMotion(ref Vector3 constrainedPosition, ref Quaternion constrainedRotation)
    {
        GetWorldSpaceRay(out var ray);

        if (InfiniteLine)
            constrainedPosition = Util.ClosestPointOnLine(constrainedPosition, ray);
        else
            constrainedPosition = Util.ClosestPointOnLineSegment(constrainedPosition, ray, _lineLength);

        
        if (ConstrainRotation)
        {
            if (transform.parent != null)
                constrainedRotation = transform.parent.rotation;
            else
                constrainedRotation = Quaternion.identity;
        }
    }

    public float GetPercentAlongPath(Vector3 pos)
    {
        GetWorldSpaceRay(out var ray);

        var dist = Util.DistOnRay(pos, ray);
        if (dist >= _lineLength)
            return 1.0f;
        else if (dist <= 0)
            return 0.0f;

        return dist / _lineLength;
    }

    void Start()
    {
        GetWorldSpacePoints(out var p1, out var p2);

        _lineLength = Vector3.Distance(p1, p2);
    }

    public void GetWorldSpacePoints(out Vector3 p1, out Vector3 p2)
    {
        p1 = Point1;
        p2 = Point2;

        if (transform.parent != null)
        {
            p1 = transform.parent.TransformPoint(p1);
            p2 = transform.parent.TransformPoint(p2);
        }
    }

    private void GetWorldSpaceRay(out Ray ray)
    {
        GetWorldSpacePoints(out var p1, out var p2);

        ray = new Ray(p1, (p2 - p1).normalized);
    }

    private void OnDrawGizmosSelected()
    {
        GetWorldSpacePoints(out var p1, out var p2);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(p1, 0.03f);
        Gizmos.DrawSphere(p2, 0.03f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(p1, p2);
    }
}
