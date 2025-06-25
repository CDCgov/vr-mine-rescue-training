using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_VentResistancePlane : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public VentResistancePlane VentResistancePlane;

    [InspectableNumericProperty("Resistance", MinValue = 0, MaxValue = 2500, SliderControl = true)]
    public float AddedResistance
    {
        get
        {
            if (VentResistancePlane != null)
                return VentResistancePlane.AddedResistance;
            else
                return _addedResistance;
        }
        set
        {
            if (VentResistancePlane != null)
            {
                VentResistancePlane.AddedResistance = value;
                VentResistancePlane.UseDefaultResistance = false;
            }

            _addedResistance = value;
        }
    }

    public string ComponentInspectorTitle => "Ventilation Resistance";

    private float _addedResistance = 0;

    public void Awake()
    {
        if (VentResistancePlane == null)
            TryGetComponent<VentResistancePlane>(out VentResistancePlane);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        //if (VentResistancePlane == null)
        //{
        //    Debug.LogError($"No VentResistancePlane on {name}");
        //    return;
        //}

        AddedResistance = component.GetParamValueFloat("AddedResistance", 0.0f);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "AddedResistance|" + AddedResistance,
        };
    }

    public string SaveName()
    {
        return "VentResistancePlane";
    }
}
