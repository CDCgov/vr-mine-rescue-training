using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_DynamicVentControl : ModularComponentInfo, ISaveableComponent
{
    DynamicVentControl DynamicVentilationControl;

    [InspectableNumericProperty("Min Resistance", MinValue = 0, MaxValue = 50, SliderControl = true)]
    public float MinResistance
    {
        get
        {
            return DynamicVentilationControl.MinResistance;
        }
        set
        {
            DynamicVentilationControl.MinResistance = value;
        }
    }

    [InspectableNumericProperty("Max Resistance", MinValue = 0, MaxValue = 2500, SliderControl = true)]
    public float MaxResistance
    {
        get
        {
            return DynamicVentilationControl.MaxResistance;
        }
        set
        {
            DynamicVentilationControl.MaxResistance = value;
        }
    }

    [InspectableNumericProperty("Current Position", MinValue = 0, MaxValue = 100, SliderControl = true, Units = NumericPropertyUnitType.Ratio)]
    public float CurrentPosition
    {
        get
        {
            return DynamicVentilationControl.GetCurrentPosition();
        }
        set
        {
            DynamicVentilationControl.SetCurrentPosition(value);
        }
    }


    public void Awake()
    {
        if (DynamicVentilationControl == null)
            TryGetComponent<DynamicVentControl>(out DynamicVentilationControl);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        if (DynamicVentilationControl == null)
        {
            Debug.LogError($"No DynamicVentilationControl on {name}");
            return;
        }

        MinResistance = component.GetParamValueFloat("MinResistance", 0.0f);
        MaxResistance = component.GetParamValueFloat("MaxResistance", 10.0f);
        CurrentPosition = component.GetParamValueFloat("CurrentPosition", 10.0f);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "MinResistance|" + MinResistance,
            "MaxResistance|" + MaxResistance,
            "CurrentPosition|" + CurrentPosition,            
        };
    }

    public string SaveName()
    {
        return "DynamicVentControl";
    }
}
