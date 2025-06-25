using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VentFireData", menuName = "VRMine/VentFireData", order = 0)]
public class VentFireData : ScriptableObject
{
    public double ContamFlowRate = 100;
    public double ContamConcentration = 70;
    public double HeatInput = 200;
    public double O2ConcLeavingFire = 0;
    public double ContamPerCuFtO2 = 0;
    public double HeatPerCuFtO2 = 0;
    public double StandardAirFlow = 0;
    public double TransitionTime = 0.01;
}
