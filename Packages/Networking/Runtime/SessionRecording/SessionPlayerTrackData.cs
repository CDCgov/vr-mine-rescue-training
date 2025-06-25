using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionPlayerTrackData : ISessionTimeSeriesData<SessionPlayerTrackData>
{
    public Vector3 Position;
    //public Vector3 AnchorSpacePosition;
    public string TeleportPoint;
    public int TeamstopIndex;

    public void CopyTo(SessionPlayerTrackData dest)
    {
        dest.Position = Position;
    }

    public void Interpolate(SessionPlayerTrackData next, float interp, ref SessionPlayerTrackData result)
    {
        CopyTo(result);
    }
}
