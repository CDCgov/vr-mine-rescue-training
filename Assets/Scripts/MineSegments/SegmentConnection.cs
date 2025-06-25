using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SegmentConnection : System.Object
{
    public Vector3 Centroid;
    public Vector3 Normal;
    public Vector3[] Boundary;

    public int ConnectionType;
    public string ConnectionID;

    public static bool DoConnectionsMatch(string connID_1, string connID_2)
    {
        if (connID_1 == null || connID_2 == null || connID_1.Length <= 0 || connID_2.Length <= 0)
            return false;

        if (connID_1.Length != connID_2.Length)
            return false;

        var length = connID_1.Length;

        //first part of connection string must match
        for (int i = 0; i < length - 1; i++)
        {
            if (connID_1[i] != connID_2[i])
                return false;
        }

        //last letter (A or B) must be different
        if (connID_1[length - 1] == connID_2[length - 1])
            return false;

        return true;
    }

    public Vector3 GetWorldSpaceCentroid(Transform seg)
    {
        return seg.TransformPoint(Centroid);
    }
}

/// <summary>
/// class for cacheing information on the connected mine segment
/// constructed for each connection on a mine segment for quick
/// access to the opposing MineSegment & connection info
/// </summary>
public class SegmentConnectionInfo
{
    public SegmentConnection Connection;
    public int ConnIndex;
    public SegmentConnection OppConnection;
    public int OppConnIndex;
    public MineSegment OppMineSegment;
}