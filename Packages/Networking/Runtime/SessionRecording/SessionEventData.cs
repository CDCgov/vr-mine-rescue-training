using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionEventData : ISessionTimeSeriesData<SessionEventData>
{
    public int EventID;
    public VRNLogEvent EventData;

    public void CopyTo(SessionEventData dest)
    {
        dest.EventID = EventID;
        dest.EventData = EventData;
    }

    public void Interpolate(SessionEventData next, float interp, ref SessionEventData result)
    {
        CopyTo(result);
    }
}
