using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Text;

public class VentAirway : VentObj
{    
    public int AirwayID { get; set; }
    public int MFireID
    {
        get
        {
            if (MFAirway == null)
                return -1;

            return MFAirway.Number;
        }
        set
        {
            MFAirway.Number = value;
        }
    }

    public double Resistance
    {
        get
        {
            return MFAirway.Resistance;
        }
        set
        {
            MFAirway.Resistance = value;
        }
    }

    public double AddedResistance;
    public double ControlResistance;
    public double OverrideResistance = -1;

    //private const double DefaultAirwayResistance = 0.007;
    private const double DefaultAirwayResistance = 0.007;

    public double CH4EmissionRate
    {
        get
        {
            return MFAirway.CH4EmissionRateAirway;
        }
        set
        {
            MFAirway.CH4EmissionRateAirway = value;
        }
    }

    public List<VentFan> LinkedFans;
    public List<VentControl> LinkedVentControls;

    public VentJunction Start;
    public VentJunction End;

    public MFAirway MFAirway;

    private MFireServerControl _serverControl;
    private double _lastResistance = -1000;

    public VentAirway()
    {
        LinkedFans = new List<VentFan>();
        LinkedVentControls = new List<VentControl>();
        MFAirway = new MFAirway();

        ResetToDefaults();
    }

    public float ComputeAirSpeed()
    {
        //FlowRate in CFM, CrossSectionalArea ft^2, speed in feet per minute
        float actualSpeed = (float)(MFAirway.FlowRate / MFAirway.CrossSectionalArea);

        //convert from feet per minute to meters per minute
        actualSpeed *= Constants.FeetToMeters;

        //convert from meters per minute to m/s
        actualSpeed /= 60.0f;

        return actualSpeed;
    }

    public Vector3 ComputeAirVelocity()
    {
        if (Start == null || End == null)
            return Vector3.zero;

        var dir = (End.WorldPosition - Start.WorldPosition).normalized;

        return dir * ComputeAirSpeed();
    }

    public void ResetToDefaults()
    {
        //MFAirway.Length = link.ComputeLength() * Constants.MetersToFeet;

        CalculateLength();


        //MFAirway.Perimeter = 100;
        //MFAirway.CrossSectionalArea = 140;

        //for 18' by 7' entries:
        MFAirway.CrossSectionalArea = 126;
        MFAirway.Perimeter = 50;

        MFAirway.FlowRate = 0;
        //MFAirway.Resistance = 1.5;
        //MFAirway.Resistance = 0.007;
        MFAirway.Resistance = DefaultAirwayResistance;
        MFAirway.FrictionFactor = 50;
        MFAirway.Type = 0;
    }

    public void ResetVentilationData()
    {
        MFAirway.FlowRate = 0;
        MFAirway.FlowDirection = 0;
        MFAirway.Type = 0;
        MFAirway.Resistance = DefaultAirwayResistance;
        MFAirway.FrictionFactor = 50;
        MFAirway.RockTemperature = 0;
        MFAirway.ThermalConductivity = 0;
        MFAirway.ThermalDefusivity = 0;
        
    }

    public void CopyVentDataFrom(MFAirway data)
    {
        if (data.Number != MFireID)
        {
            Debug.LogError($"Tried to update airway {AirwayID}:{MFireID} witih mfire data from {data.Number}");
            return;
        }

        MFAirway.CH4EmissionRateAirway = data.CH4EmissionRateAirway;
        MFAirway.CH4EmissionRateSurfArea = data.CH4EmissionRateSurfArea;
        MFAirway.CrossSectionalArea = data.CrossSectionalArea;
        MFAirway.EndJunction = data.EndJunction;
        MFAirway.StartJunction = data.StartJunction;
        MFAirway.FlowDirection = data.FlowDirection;
        MFAirway.FlowRate = data.FlowRate;
        MFAirway.FrictionFactor = data.FrictionFactor;
        MFAirway.Length = data.Length;
        MFAirway.Perimeter = data.Perimeter;
        MFAirway.Resistance = data.Resistance;
        MFAirway.RockTemperature = data.RockTemperature;
        MFAirway.ThermalConductivity = data.ThermalConductivity;
        MFAirway.ThermalDefusivity = data.ThermalDefusivity;
        MFAirway.Type = data.Type;

        bool reversed = false;
        
        if ((MFAirway.StartJunction != Start.MFireID && MFAirway.StartJunction != End.MFireID) ||
            (MFAirway.EndJunction != Start.MFireID && MFAirway.EndJunction != End.MFireID))
        {
            if (MFAirway.EndJunction == Start.MFireID || MFAirway.StartJunction == End.MFireID)
            {
                Debug.Log($"Vent: Airflow reversed in virtual airway connecting to airway {AirwayID}:{MFireID} {AirwayID}:{MFireID} corrected: {MFAirway.StartJunction} -> {MFAirway.EndJunction}");
                reversed = true;
            }
        }

        if (MFAirway.EndJunction == Start.MFireID && MFAirway.StartJunction == End.MFireID)
        {
            Debug.Log($"Vent: Airflow reversed in airway {AirwayID}:{MFireID} corrected: {MFAirway.StartJunction} -> {MFAirway.EndJunction}");
            reversed = true;
        }

        if (reversed)
        {
            var tmp = Start;
            Start = End;
            End = tmp;
        }
    }

    public void CopyTo(VRNVentAirway vrnAirway)
    {
        vrnAirway.AirwayID = AirwayID;
        vrnAirway.MFireID = MFireID;
        vrnAirway.Resistance = (float)Resistance;
        vrnAirway.AddedResistance = (float)AddedResistance;
        vrnAirway.ControlResistance = (float)ControlResistance;
        vrnAirway.OverrideResistance = (float)OverrideResistance;
        vrnAirway.CH4EmissionRate = (float)CH4EmissionRate;
        vrnAirway.StartJunction = Start.JunctionID;
        vrnAirway.EndJunction = End.JunctionID;
        vrnAirway.CrossSectionalArea = (float)MFAirway.CrossSectionalArea;
        vrnAirway.FlowDirection = MFAirway.FlowDirection;
        vrnAirway.FlowRate = (float)MFAirway.FlowRate;
        vrnAirway.FrictionFactor = (float)MFAirway.FrictionFactor;
        vrnAirway.Length = (float)MFAirway.Length;
        vrnAirway.Perimeter = (float)MFAirway.Perimeter;
        vrnAirway.RockTemperature = (float)MFAirway.RockTemperature;
        vrnAirway.ThermalConductivity = (float)MFAirway.ThermalConductivity;
        vrnAirway.ThermalDefusivity = (float)MFAirway.ThermalDefusivity;
        vrnAirway.Type = MFAirway.Type;
    }

    public void LoadFrom(VRNVentAirway vrnAirway, VentGraph graph)
    {
        //copy data
        AirwayID = vrnAirway.AirwayID;
        MFireID = vrnAirway.MFireID;
        Resistance = vrnAirway.Resistance;
        AddedResistance = vrnAirway.AddedResistance;
        ControlResistance = vrnAirway.ControlResistance;
        OverrideResistance = vrnAirway.OverrideResistance;
        CH4EmissionRate = vrnAirway.CH4EmissionRate;
        MFAirway.CrossSectionalArea = vrnAirway.CrossSectionalArea;
        MFAirway.FlowDirection = vrnAirway.FlowDirection;
        MFAirway.FlowRate = vrnAirway.FlowRate;
        MFAirway.FrictionFactor = vrnAirway.FrictionFactor;
        MFAirway.Length = vrnAirway.Length;
        MFAirway.Perimeter = vrnAirway.Perimeter;
        MFAirway.RockTemperature = vrnAirway.RockTemperature;
        MFAirway.ThermalConductivity = vrnAirway.ThermalConductivity;
        MFAirway.ThermalDefusivity = vrnAirway.ThermalDefusivity;
        MFAirway.Type = vrnAirway.Type;

        //lookup junctions
        Start = graph.FindJunction(vrnAirway.StartJunction);
        if (Start == null)
            Debug.LogError($"VentAirway: Couldn't find start junction {vrnAirway.StartJunction} when loading airway {vrnAirway.AirwayID}");
        End = graph.FindJunction(vrnAirway.EndJunction);
        if (End == null)
            Debug.LogError($"VentAirway: Couldn't find start junction {vrnAirway.EndJunction} when loading airway {vrnAirway.AirwayID}");

        //add to graph
        graph.UpdateAirway(this);
    }

    public void CalculateLength()
    {
        if (Start == null || End == null)
            MFAirway.Length = 40;
        else
            MFAirway.Length = Vector3.Distance(Start.WorldPosition, End.WorldPosition) * Constants.MetersToFeet;
    }

    public void CalculateResistance()
    {
        double resistance = DefaultAirwayResistance + AddedResistance + ControlResistance;
        if (LinkedVentControls != null)
        {
            foreach (var control in LinkedVentControls)
            {
                resistance += control.ResistanceChange;
            }
        }

        if (resistance < 0)
            resistance = 0;

        if (OverrideResistance > 0)
            resistance = OverrideResistance;

        //Debug.Log($"VentAirway Calculated Resistance: {resistance:F2}");
        Resistance = resistance;

        if (Resistance != _lastResistance)
        {
            _lastResistance = Resistance;
            UpdateAirway();
        }
    }

    public IEnumerable<VentAirway> GetAdjacentAirways()
    {
        if (Start != null && Start.LinkedAirways != null)
        {
            foreach (var airway in Start.LinkedAirways)
            {
                if (airway == this)
                    continue;

                yield return airway;
            }
        }

        if (End != null && End.LinkedAirways != null)
        {
            foreach (var airway in End.LinkedAirways)
            {
                if (airway == this)
                    continue;

                yield return airway;
            }
        }
    }

    public void UpdateAirway()
    {
        UpdateAirway(_serverControl);
    }

    public void UpdateAirway(MFireServerControl serverControl)
    {
        if (serverControl == null)
            return;

        _serverControl = serverControl;

        MFAirway.StartJunction = Start.MFireID;
        MFAirway.EndJunction = End.MFireID;

        //UpdateLength();
        serverControl.ChangeAirway(MFAirway);
    }

    public override void AppendText(StringBuilder sb)
    {
        var airSpeed = ComputeAirSpeed();

        sb.AppendLine($"AirwayID : {AirwayID}");
        sb.AppendLine($"MFire ID : {MFireID}");
        if (Start != null)
            sb.AppendLine($"Start : {Start.JunctionID}:{Start.MFireID}");
        if (End != null)
            sb.AppendLine($"End : {End.JunctionID}:{End.MFireID}");
        sb.AppendLine($"CH4 Emission : {CH4EmissionRate:F2}");
        sb.AppendLine($"Flow Rate : {MFAirway.FlowRate:F2}");
        sb.AppendLine($"Flow Speed : {airSpeed:F3}");
        sb.AppendLine($"Flow Dir : {MFAirway.FlowDirection}");
        sb.AppendLine($"Length : {MFAirway.Length:F2}");
        sb.AppendLine($"Resistance : {MFAirway.Resistance:F3}");
        sb.AppendLine($"Added Resistance : {AddedResistance:F2}");
        sb.AppendLine($"Rock Temp : {MFAirway.RockTemperature:F2}");
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        AppendText(sb);
        return sb.ToString();
    }

    public IEnumerable<Vector3> GetSamplePoints()
    {
        var p1 = Start.WorldPosition;
        var p2 = End.WorldPosition;

        //move initial points slightly inward
        var dir = (p2 - p1).normalized;
        p1 = p1 + dir * 0.2f;
        p2 = p2 - dir * 0.2f;
        var midp = (p1 + p2) * 0.5f;

        yield return p1;
        yield return midp;
        yield return p2;
    }

    //public void SetMFireID(int id)
    //{

    //}

    /*
    public int Number;
    public double ThermalConductivity;
    public double RockTemperature;
    public double Resistance;
    public double Perimeter;
    public double CrossSectionalArea;
    public double Length;
    public double ThermalDefusivity;
    public double FrictionFactor;
    public double CH4EmissionRateSurfArea;
    public double CH4EmissionRateAirway;
    public int FlowDirection;
    public int Type;
    public int EndJunction;
    public int StartJunction;
    public double FlowRate;
    */

}
