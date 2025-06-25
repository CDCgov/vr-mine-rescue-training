using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_Door : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public string ComponentName = "Door";
    public DynamicVentControl DynamicVentControl;

    [InspectableNumericProperty("Open Resistance", MinValue = 0, MaxValue = 50, SliderControl = true)]
    public float MinResistance
    {
        get => DynamicVentControl.MinResistance;
        set => DynamicVentControl.MinResistance = value;
    }

    [InspectableNumericProperty("Closed Resistance", MinValue = 0, MaxValue = 2500, SliderControl = true)]
    public float MaxResistance
    {
        get => DynamicVentControl.MaxResistance;
        set => DynamicVentControl.MaxResistance = value;
    }

    [InspectableBoolProperty("Door Open", OnText = "Open", OffText = "Closed")]
    public bool DoorOpen
    {
        get => DynamicVentControl.GetCurrentPosition() > 0.5f;
        set => DynamicVentControl.SetCurrentPosition(value ? 1.0f : 0.0f);
    }

    public string ComponentInspectorTitle => ComponentName;

    public void Awake()
    {
        if (DynamicVentControl == null)
            TryGetComponent<DynamicVentControl>(out DynamicVentControl);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        if (DynamicVentControl == null)
        {
            Debug.LogError($"No DynamicVentilationControl on {name}");
            return;
        }

        MinResistance = component.GetParamValueFloat("MinResistance", 0.0f);
        MaxResistance = component.GetParamValueFloat("MaxResistance", 10.0f);
        DoorOpen = component.GetParamValueBool("DoorOpen", false);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "MinResistance|" + MinResistance,
            "MaxResistance|" + MaxResistance,
            "DoorOpen|" + DoorOpen,
        };
    }

    public string SaveName()
    {
        return ComponentName;
    }
}
