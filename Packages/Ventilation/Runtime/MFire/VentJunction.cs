using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MFireProtocol;
using ProtoBuf;
using DelaunatorSharp;

public class VentJunction : VentObj, IPoint
{
    public int JunctionID;
    public int MFireID
    {
        get
        {
            if (MFJunction == null)
                return -1;

            return MFJunction.Number;
        }
        set
        {
            MFJunction.Number = value;
        }
    }

    public bool IsStartJunction 
    {
        get
        {
            return _isStartJunction;
        }
        set
        {
            if (value)
            {
                MFJunction.AtmosphereJuncType = 1;
                _isStartJunction = true;
            }
            else
            {
                MFJunction.AtmosphereJuncType = 0;//"start junction" is type 1, all others 0, per docs
                _isStartJunction = false;
            }

        }
    }

    public bool IsInAtmosphere
    {
        get
        {
            return MFJunction.IsInAtmosphere;
        }
        set
        {
            MFJunction.IsInAtmosphere = value;
        }
    }

    public double X { get => WorldPosition.x; set => throw new System.NotImplementedException(); }
    public double Y { get => WorldPosition.z; set => throw new System.NotImplementedException(); }

    public Vector3 WorldPosition;
    public List<VentAirway> LinkedAirways;

    public MFJunction MFJunction;

    private bool _isStartJunction = false;
    private MFireServerControl _serverControl;

    public VentJunction()
    {
        LinkedAirways = new List<VentAirway>();

        MFJunction = new MFJunction();
        ResetToDefault();
    }

    public void ResetToDefault()
    {
        IsStartJunction = false;
        MFJunction.Temperature = 50;        
        MFJunction.IsInAtmosphere = false;
    }

    public void ResetVentilationData()
    {
        MFJunction.Temperature = 50;
        MFJunction.TotalCH4 = 0;
        MFJunction.TotalContaminant = 0;
        MFJunction.TotalAirFlow = 0;
        MFJunction.TotalHeat = 0;
        MFJunction.TemperatureBkp = 0;
        MFJunction.AtmosphereTemperature = 0;
        MFJunction.CH4Concentration = 0;
        MFJunction.CH4ConcentrationBkp = 0;
        MFJunction.CH4InitialConc = 0;
        MFJunction.ConditionChanged = false;
        MFJunction.ContamConcentration = 0;
        MFJunction.ContamConcentrationBkp = 0;
        MFJunction.TemperatureInter = 0;
    }

    public void CopyVentDataFrom(MFJunction data)
    {
        if (data.Number != MFireID)
        {
            Debug.LogError($"Tried to update junction {JunctionID}:{MFireID} witih mfire data from {data.Number}");
            return;
        }

        MFJunction.AtmosphereJuncType = data.AtmosphereJuncType;
        MFJunction.AtmosphereTemperature = data.AtmosphereTemperature;
        MFJunction.CH4Concentration = data.CH4Concentration;
        MFJunction.CH4ConcentrationBkp = data.CH4ConcentrationBkp;
        MFJunction.CH4InitialConc = data.CH4InitialConc;
        MFJunction.ConditionChanged = data.ConditionChanged;
        MFJunction.ContamConcentration = data.ContamConcentration;
        MFJunction.ContamConcentrationBkp = data.ContamConcentrationBkp;
        MFJunction.Elevation = data.Elevation;
        MFJunction.IsInAtmosphere = data.IsInAtmosphere;
        MFJunction.Temperature = data.Temperature;
        MFJunction.TemperatureBkp = data.TemperatureBkp;
        MFJunction.TemperatureInter = data.TemperatureInter;
        MFJunction.TotalAirFlow = data.TotalAirFlow;
        MFJunction.TotalCH4 = data.TotalCH4;
        MFJunction.TotalContaminant = data.TotalContaminant;
        MFJunction.TotalHeat = data.TotalHeat;
    }

    public void CopyTo(VRNVentJunction vrnJunction)
    {
        vrnJunction.JunctionID = JunctionID;
        vrnJunction.MFireID = MFireID;
        vrnJunction.IsStartJunction = IsStartJunction;
        vrnJunction.IsInAtmosphere = IsInAtmosphere;
        vrnJunction.WorldPosition = WorldPosition.ToVRNVector3();
        vrnJunction.CH4Concentration = (float)MFJunction.CH4Concentration;
        vrnJunction.TotalAirFlow = (float)MFJunction.TotalAirFlow;
        vrnJunction.TotalContaminant = (float)MFJunction.TotalContaminant;
        vrnJunction.TotalCH4 = (float)MFJunction.TotalCH4;
        vrnJunction.ContamConcentration = (float)MFJunction.ContamConcentration;
        vrnJunction.TemperatureInter = (float)MFJunction.TemperatureInter;
        vrnJunction.TotalHeat = (float)MFJunction.TotalHeat;
        vrnJunction.ConditionChanged = MFJunction.ConditionChanged;
        vrnJunction.Temperature = (float)MFJunction.Temperature;
        vrnJunction.Elevation = (float)MFJunction.Elevation;
        vrnJunction.CH4InitialConc = (float)MFJunction.CH4InitialConc;
        vrnJunction.AtmosphereJuncType = MFJunction.AtmosphereJuncType;
        vrnJunction.AtmosphereTemperature = (float)MFJunction.AtmosphereTemperature;

    }

    public void LoadFrom(VRNVentJunction vrnJunction, VentGraph graph)
    {
        JunctionID =  vrnJunction.JunctionID;
        MFireID = vrnJunction.MFireID;
        IsStartJunction = vrnJunction.IsStartJunction;
        IsInAtmosphere = vrnJunction.IsInAtmosphere;
        WorldPosition = vrnJunction.WorldPosition.ToVector3();
        MFJunction.CH4Concentration = vrnJunction.CH4Concentration;
        MFJunction.TotalAirFlow = vrnJunction.TotalAirFlow;
        MFJunction.TotalContaminant = vrnJunction.TotalContaminant;
        MFJunction.TotalCH4 = vrnJunction.TotalCH4;
        MFJunction.ContamConcentration = vrnJunction.ContamConcentration;
        MFJunction.TemperatureInter = vrnJunction.TemperatureInter;
        MFJunction.TotalHeat = vrnJunction.TotalHeat;
        MFJunction.ConditionChanged = vrnJunction.ConditionChanged;
        MFJunction.Temperature = vrnJunction.Temperature;
        MFJunction.Elevation = vrnJunction.Elevation;
        MFJunction.CH4InitialConc = vrnJunction.CH4InitialConc;
        MFJunction.AtmosphereJuncType = vrnJunction.AtmosphereJuncType;
        MFJunction.AtmosphereTemperature = vrnJunction.AtmosphereTemperature;

        graph.UpdateJunction(this);
    }

    public override void AppendText(StringBuilder sb)
    {
        sb.AppendLine($"JunctionID : {JunctionID}");
        sb.AppendLine($"MFire ID : {MFireID}");
        sb.AppendLine($"CH4Conc : {MFJunction.CH4Concentration:F4}");
        sb.AppendLine($"CH4InitialConc : {MFJunction.CH4InitialConc:F2}");
        sb.AppendLine($"Cond Changed : {MFJunction.ConditionChanged}");
        sb.AppendLine($"Contam Conc : {MFJunction.ContamConcentration:F6}");
        sb.AppendLine($"Is In Atmo : {MFJunction.IsInAtmosphere}");
        sb.AppendLine($"Temperature : {MFJunction.Temperature:F2}");
        sb.AppendLine($"Total Airflow : {MFJunction.TotalAirFlow:F2}");
        sb.AppendLine($"Toal CH4 : {MFJunction.TotalCH4:F2}");
        sb.AppendLine($"Total Contam : {MFJunction.TotalContaminant:F2}");
        sb.AppendLine($"Total Heat : {MFJunction.TotalHeat:F2}");
        
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        AppendText(sb);
        return sb.ToString();
    }

    public void UpdateJunction()
    {
        UpdateJunction(_serverControl);
    }

    public void UpdateJunction(MFireServerControl serverControl)
    {
        if (serverControl == null)
            return;

        _serverControl = serverControl;

        serverControl.UpdateJunction(MFJunction);
    }


    /*
    public int Number;
    public double CH4Concentration;
    public double TotalAirFlow;
    public double TotalContaminant;
    public double TotalCH4;
    public double CH4ConcentrationBkp;
    public double ContamConcentrationBkp;
    public double TemperatureBkp;
    public double ContamConcentration;
    public double TemperatureInter;
    public double TotalHeat;
    public bool ConditionChanged;
    public double Temperature;
    public double Elevation;
    public double CH4InitialConc;
    public bool IsInAtmosphere;
    public int AtmosphereJuncType;
    public double AtmosphereTemperature; */
}
