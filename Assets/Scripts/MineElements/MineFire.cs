using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MFireProtocol;

[System.Serializable]
public class MineFire : MineElement
{
    public double ContamFlowRate = 100;
    public double ContamConcentration = 70;
    public double HeatInput = 10;
    public double O2ConcLeavingFire = 0;
    public double ContamPerCuFtO2 = 0;
    public double HeatPerCuFtO2 = 0;
    public double StandardAirFlow = 0;
    public double TransitionTime = 0.01;

    public void CopyTo(MFFire fire)
    {
        fire.ContamFlowRate = ContamFlowRate;
        fire.ContamConcentration = ContamConcentration;
        fire.HeatInput = HeatInput;
        fire.O2ConcLeavingFire = O2ConcLeavingFire;
        fire.ContamPerCuFtO2 = ContamPerCuFtO2;
        fire.HeatPerCuFtO2 = HeatPerCuFtO2;
        fire.StandardAirFlow = StandardAirFlow;
        fire.TransitionTime = TransitionTime;
    }
}