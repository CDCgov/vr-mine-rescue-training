using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct MapSegment
{
    public Vector2[] SegVertices;

    //public IEnumerator<g3.Line> GetLine
}

public class SegmentGeometry : MonoBehaviour
{
    public SegmentConnection[] SegmentConnections;
    public MineSegment AssociatedSegment;
    public GameObject MeshRef;
    public GameObject StrapRef;
    public Vector2[] MapVertices;
    public MapSegment[] MapSegments;

    public void OnDrawGizmosSelected()
    {
        if (SegmentConnections == null)
            return;

        foreach (SegmentConnection conn in SegmentConnections)
        {			
            Vector3 pos = transform.TransformPoint(conn.Centroid);

            Gizmos.DrawSphere(pos, 0.1f);
            Gizmos.DrawLine(pos, pos + (transform.rotation * conn.Normal).normalized * 5.0f);

            //Gizmos.DrawLine(pos, pos + (Quaternion.Euler(conn.Normal) * transform.rotation.eulerAngles).normalized * 5.0f);
        }

        foreach (MapSegment seg in MapSegments)
        {
            
        }

    }
}