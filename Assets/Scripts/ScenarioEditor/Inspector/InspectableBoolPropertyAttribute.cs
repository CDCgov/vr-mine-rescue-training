using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectableBoolPropertyAttribute : InspectablePropertyAttribute
{
    public string OnText;
    public string OffText;

    public InspectableBoolPropertyAttribute(string displayName, string tooltip = null) : base(displayName, tooltip)
    {        
    }
}
