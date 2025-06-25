using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectableDropdownPropertyAttribute : InspectablePropertyAttribute
{
    public InspectableDropdownPropertyAttribute(string displayName, string tooltip) : base(displayName, tooltip)
    {
    }
}
