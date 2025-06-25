using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class VehicleProxTrialSettings : TrialSettings
{
    public string SceneName { get; set; }
    public string MachinePrefabName { get; set; }
    //public string MachineType { get; set; }
    
    //public float MachineMass { get; set; }
    public float LoadLevel { get; set; }
    public float Slope { get; set; }
    public float InitialSpeedMultiplier { get; set; }
    public float BrakeTorqueMultiplier { get; set; }
    public float AsymptoteForce { get; set; }
    public float AsymptoteSlip { get; set; }
    public float ExtremumForce { get; set; }
    public float ExtremumSlip { get; set; }	
    public float FrictionStiffness { get; set; }
    public int WheelColliderSubsteps { get; set; }
    public float LagTime { get; set; }
    public string MWCLocation { get; set; }
    public string BrakingModel { get; set; }

    public string ProxConfig { get; set; }
    
    public float ProxYFront { get; set; }
    public float ProxYLeft { get; set; }
    public float ProxYRight { get; set; }
    public float ProxYBack { get; set; }

    public float ProxRFront { get; set; }
    public float ProxRLeft { get; set; }
    public float ProxRRight { get; set; }
    public float ProxRBack { get; set; }

    public float WheelWeightOverride { get; set; }

    private StringBuilder _sb;

    public override string ToString()
    {
        if (_sb == null)
            _sb = new StringBuilder();

        _sb.Length = 0;

        //_sb.AppendLine($"Scene: {SceneName}");
        _sb.AppendLine($"Prefab: {MachinePrefabName}");
        _sb.AppendLine($"ProxConfig: {ProxConfig}");
        _sb.AppendLine($"LoadLevel: {LoadLevel:F2}");
        _sb.AppendLine($"Slope: {Slope:F2}");
        _sb.AppendLine($"F_Asymptote: {AsymptoteForce:F2} {AsymptoteSlip:F2}");
        _sb.Append($"F_Extremum: {ExtremumForce:F2} {ExtremumSlip:F2}");

        return _sb.ToString();
    }

}