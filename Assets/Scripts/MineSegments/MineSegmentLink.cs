using UnityEngine;
using System.Collections.Generic;

public class MineSegmentLink : MonoBehaviour
{
    public MineSegment Segment1;
    public int Seg1ConnIndex;//index of the connection used to connect segment 1

    public MineSegment Segment2;
    public int Seg2ConnIndex; //index of the connection used to connect segment 2

    [System.NonSerialized]
    public int MFireAirway;

    public float ComputeLength()
    {
        if (Segment1 == null || Segment2 == null)
        {
            Debug.Log("ERROR INVALID SEGMENT LENGTH");
            return 100;
        }		
        
        //should actually use the connection point not the center
        Vector3 delta = Segment1.transform.position - Segment2.transform.position;
        return delta.magnitude;
    }

    public Vector3 GetWorldSpaceCenter()
    {
        if (Segment1 == null || Segment2 == null)
            return Vector3.zero;

        Vector3 pos = Segment1.transform.position + Segment2.transform.position;
        pos *= 0.5f;

        return pos;
    }
}
