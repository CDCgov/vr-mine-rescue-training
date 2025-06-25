using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_MethaneGenerator : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public VentMethaneGenerator Generator;

    [InspectableNumericProperty("CH4 Emission Rate", MinValue = 0, MaxValue = 500, SliderControl = true)]
    public double CH4EmissionRate
    {
        get
        {
            return Generator.CH4EmissionRate;
        }
        set
        {
            Generator.CH4EmissionRate = value;
        }
    }

    public string ComponentInspectorTitle => "Methane Generator";

    void Awake()
    {
        if (Generator == null)
            TryGetComponent<VentMethaneGenerator>(out Generator);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        if (Generator == null)
        {
            Debug.LogError($"No Methane Generator on {name}");
            return;
        }

        CH4EmissionRate = component.GetParamValueDouble("CH4EmissionRate", 0.0f);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "CH4EmissionRate|" + CH4EmissionRate,
        };
    }

    public string SaveName()
    {
        return "VentMethaneGenerator";
    }
}
