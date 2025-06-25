using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Text;

public class VentFire : VentObj
{
    public const float MinFireContamFlowRate = 0.1f;

    public int FireID { get; set; }
    public int MFireID
    {
        get
        {
            if (MFFire == null)
                return -1;

            return MFFire.Number;
        }
        set
        {
            MFFire.Number = value;
        }
    }

    public VentAirway Airway;

    public Vector3 WorldPosition;

    public MFFire MFFire
    {
        get
        {
            if (Airway != null)
                _mfFire.AirwayNo = Airway.MFireID;

            return _mfFire;
        }
        set
        {
            _mfFire = value;
        }
    }

    private MFFire _mfFire;
    private MFireServerControl _serverControl;
    private double _startingContamFlowRate;

    public VentFire()
    {
        ResetToDefaults();
    }

    public void ResetToDefaults()
    {
        _mfFire = new MFFire();
        _startingContamFlowRate = _mfFire.ContamFlowRate;
    }

    public void SetFireData(VentFireData fireData)
    {
        MFFire.ContamFlowRate = fireData.ContamFlowRate;
        MFFire.ContamConcentration = fireData.ContamConcentration;
        MFFire.HeatInput = fireData.HeatInput;
        MFFire.O2ConcLeavingFire = fireData.O2ConcLeavingFire;
        MFFire.ContamPerCuFtO2 = fireData.ContamPerCuFtO2;
        MFFire.HeatPerCuFtO2 = fireData.HeatPerCuFtO2;
        MFFire.StandardAirFlow = fireData.StandardAirFlow;
        MFFire.TransitionTime = fireData.TransitionTime;

        UpdateFire();
    }

    public void CopyTo(VRNVentFire fire)
    {
        fire.FireID = FireID;
        fire.MFireID = MFireID;
        if (Airway != null)
            fire.AirwayID = Airway.AirwayID;
        else
            fire.AirwayID = -1;
        fire.WorldPosition = WorldPosition.ToVRNVector3();
    }

    public void LoadFrom(VRNVentFire fire, VentGraph graph)
    {
        FireID = fire.FireID;
        MFireID = fire.MFireID;

        Airway = graph.FindAirway(fire.AirwayID);
        WorldPosition = fire.WorldPosition.ToVector3();
    }

    public override string ToString()
    {
        return $"VentFire {FireID}:{MFireID} Airway {MFFire.AirwayNo}";
    }

    public override void AppendText(StringBuilder sb)
    {
        var fire = MFFire;
        sb.AppendLine($"FireID : {FireID}");
        sb.AppendLine($"MFireID : {MFireID}");
        sb.AppendLine($"AirwayNo : {fire.AirwayNo}");
        sb.AppendLine($"ContamFlowRate : {fire.ContamFlowRate:F2}");
        sb.AppendLine($"ContamConc : {fire.ContamConcentration:F2}");
        sb.AppendLine($"HeatInput : {fire.HeatInput:F2}");
        sb.AppendLine($"O2ConcLeavingFire : {fire.O2ConcLeavingFire:F2}");
        sb.AppendLine($"ContamPerCuFtO2 : {fire.ContamPerCuFtO2:F2}");
        sb.AppendLine($"HeatPerCuFtO2 : {fire.HeatPerCuFtO2:F2}");
        sb.AppendLine($"StandardAirFlow : {fire.StandardAirFlow:F2}");
        sb.AppendLine($"TransitionTime : {fire.TransitionTime:F2}");

    }

    /// <summary>
    /// Change the fire strength to the supplied value (0 to 1)
    /// </summary>
    /// <param name="strength"></param>
    public void UpdateFireStrength(float strength)
    {
        var contamFlow = strength * _startingContamFlowRate;
        if (contamFlow < MinFireContamFlowRate)
            contamFlow = MinFireContamFlowRate;

        _mfFire.ContamFlowRate = contamFlow;
        UpdateFire(_serverControl, false);
    }

    public void UpdateFire()
    {
        UpdateFire(_serverControl);
    }

    public void UpdateFire(MFireServerControl serverControl, bool updateStartingStrength = true)
    {
        if (updateStartingStrength)
            _startingContamFlowRate = _mfFire.ContamFlowRate;

        if (serverControl == null)
            return;

        _serverControl = serverControl;
        serverControl.ChangeFire(MFFire);
    }
}
