using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISliderDynamicVentControl : UISliderFieldBase, IComponentInspector
{
    public enum TargetParameter
    {
        MinResistance,
        MaxResistance,
        CurrentPosition,
    }

    public TargetParameter ControlledParameter;

    private ComponentInfo_DynamicVentControl _component;

    protected override void Start()
    {
        base.Start();

        SetCurrentValue(GetValue());
    }

    public void SetInspectedComponent(ModularComponentInfo component)
    {
        _component = component as ComponentInfo_DynamicVentControl;

        SetCurrentValue(GetValue());
    }

    protected override void OnValueChanged(float val)
    {
        if (_component == null)
            return;

        SetValue(val);
    }

    private float GetValue()
    {
        if (_component == null)
            return 0;

        switch (ControlledParameter)
        {
            case TargetParameter.CurrentPosition:
                return _component.CurrentPosition * 100.0f;
            case TargetParameter.MinResistance:
                return _component.MinResistance;
            case TargetParameter.MaxResistance:
                return _component.MaxResistance;
            default:
                return 0;
        }
    }

    private void SetValue(float val)
    {
        if (_component == null)
            return;

        switch (ControlledParameter)
        {
            case TargetParameter.CurrentPosition:
                _component.CurrentPosition = val / 100.0f;
                return;

            case TargetParameter.MinResistance:
                _component.MinResistance = val;
                return;
                
            case TargetParameter.MaxResistance:
                _component.MaxResistance = val;
                return;
                
        }
    }
}
