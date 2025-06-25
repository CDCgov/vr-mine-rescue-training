using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_VentFire : ModularComponentInfo, ISaveableComponent
{
    public MineFireHost MineFireHost;

    public double ContamFlowRate
    {
        get
        {
            if (MineFireHost == null || MineFireHost.MineFire == null)
                return 0;

            return MineFireHost.MineFire.ContamFlowRate;
        }
        set
        {
            if (MineFireHost == null)
                return;

            if (MineFireHost.MineFire != null)
                MineFireHost.MineFire.ContamFlowRate = value;
            if (MineFireHost.VentFire != null && MineFireHost.VentFire.MFFire != null)
                MineFireHost.VentFire.MFFire.ContamFlowRate = value;
        }

    }

    public double ContamConcentration
    {
        get
        {
            if (MineFireHost == null || MineFireHost.MineFire == null)
                return 0;

            return MineFireHost.MineFire.ContamConcentration;
        }
        set
        {
            if (MineFireHost == null)
                return;

            if (MineFireHost.MineFire != null)
                MineFireHost.MineFire.ContamConcentration = value;
            if (MineFireHost.VentFire != null && MineFireHost.VentFire.MFFire != null)
                MineFireHost.VentFire.MFFire.ContamConcentration = value;
        }
    }

    public double TransitionTime
    {
        get
        {
            if (MineFireHost == null || MineFireHost.MineFire == null)
                return 0;

            return MineFireHost.MineFire.TransitionTime;
        }
        set
        {
            if (MineFireHost == null)
                return;

            if (MineFireHost.MineFire != null)
                MineFireHost.MineFire.TransitionTime = value;
            if (MineFireHost.VentFire != null && MineFireHost.VentFire.MFFire != null)
                MineFireHost.VentFire.MFFire.TransitionTime = value;
        }
    }

    public double HeatInput
    {
        get
        {
            if (MineFireHost == null || MineFireHost.MineFire == null)
                return 0;

            return MineFireHost.MineFire.HeatInput;
        }
        set
        {
            if (MineFireHost == null)
                return;

            if (MineFireHost.MineFire != null)
                MineFireHost.MineFire.HeatInput = value;
            if (MineFireHost.VentFire != null && MineFireHost.VentFire.MFFire != null)
                MineFireHost.VentFire.MFFire.HeatInput = value;
        }
    }

    public void Awake()
    {
        if (MineFireHost == null)
            TryGetComponent<MineFireHost>(out MineFireHost);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null) 
            return;

        if (MineFireHost == null)
        {
            Debug.LogError($"No MineFireHost on {name}");
            return;
        }

        ContamFlowRate = component.GetParamValueDouble("ContamFlowRate", 1.8);
        ContamConcentration = component.GetParamValueDouble("ContamConcentration", 70);
        HeatInput = component.GetParamValueDouble("HeatInput", 5000);
        TransitionTime = component.GetParamValueDouble("TransitionTime", 0.01);
    }

    public string[] SaveInfo()
    {
        return new string[] 
        { 
            "ContamFlowRate|" + ContamFlowRate,
            "ContamConcentration|" + ContamConcentration,
            "HeatInput|" + HeatInput,
            "TransitionTime|" + TransitionTime,
        };
    }

    public string SaveName()
    {
        return "VentFire";
    }
}
