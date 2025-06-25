using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NumericPropertyUnitType
{
    Undefined = 0,
    Ratio = 1,
    Meters = 2,
}

public class InspectableNumericPropertyAttribute : InspectablePropertyAttribute
{
    public float MinValue = float.NaN;
    public float MaxValue = float.NaN;
    public bool SliderControl = false;
    public NumericPropertyUnitType Units = NumericPropertyUnitType.Undefined;

    public InspectableNumericPropertyAttribute(string displayName, string tooltip = null) : base(displayName, tooltip)
    {
    }
}
