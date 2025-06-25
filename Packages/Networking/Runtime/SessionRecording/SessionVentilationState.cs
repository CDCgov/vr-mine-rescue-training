using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionVentilationState : ISessionTimeSeriesData<SessionVentilationState>
{
    public int VentStateIndex;
    public VRNVentGraph VentGraph;

    public void CopyTo(SessionVentilationState dest)
    {
        dest.VentGraph = VentGraph;
    }

    public void Interpolate(SessionVentilationState next, float interp, ref SessionVentilationState result)
    {
        CopyTo(result);
    }
}
