using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class InspectablePropertyAttribute : Attribute
{
    public string DisplayName;
    public string Tooltip = null;

    public InspectablePropertyAttribute(string displayName, string tooltip)
    {
        DisplayName = displayName;
        Tooltip = tooltip;
    }
}
