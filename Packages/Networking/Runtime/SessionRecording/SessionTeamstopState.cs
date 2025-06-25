using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionTeamstopState : ISessionTimeSeriesData<SessionTeamstopState>
{
    public int TeamstopIndex;
    public string TeleportTarget;
    public float TeamstopStartTime;
    public float TeamstopEndTime;
    
    public void CopyTo(SessionTeamstopState dest)
    {
        dest.TeleportTarget = TeleportTarget;
        dest.TeamstopStartTime = TeamstopStartTime;
        dest.TeamstopEndTime = TeamstopEndTime;
    }

    public void Interpolate(SessionTeamstopState next, float interp, ref SessionTeamstopState result)
    {
        CopyTo(result);
    }
}
