using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSyncServerVerticalConstraint : NetSyncServerConstraint
{
    public float PositiveYDistance = 1;
    public float NegativeYDistance = 0;

    private Vector3 _startPosition;

    public override void GetConstrainedMotion(ref Vector3 constrainedPosition, ref Quaternion constrainedRotation)
    {
        constrainedRotation = transform.rotation;
        constrainedPosition.x = transform.position.x;
        constrainedPosition.z = transform.position.z;
        float num = constrainedPosition.y;
        constrainedPosition.y = Mathf.Clamp(num, _startPosition.y + NegativeYDistance, _startPosition.y + PositiveYDistance);
    }

    void Awake()
    {
        _startPosition = transform.position;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 PointA = transform.position;
        PointA.y += PositiveYDistance;
        Vector3 PointB = transform.position;
        PointB.y += NegativeYDistance;
        //Vector3 worldA = transform.TransformPoint(PointA);
        //Vector3 worldB = transform.TransformPoint(PointB);
        Vector3 worldA = PointA;
        Vector3 worldB = PointB;
        //if (Reference != null)
        //{
        //    worldA = Reference.TransformPoint(PointA);
        //    worldB = Reference.TransformPoint(PointB);
        //}
        //else
        //{
        //    worldA = PointA;
        //    worldB = PointB;
        //}
        Gizmos.DrawSphere(worldA, 0.01f);
        Gizmos.DrawSphere(worldB, 0.01f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(worldA, worldB);
    }
}
