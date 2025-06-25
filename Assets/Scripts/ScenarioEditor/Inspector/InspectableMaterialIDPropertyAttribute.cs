using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectableMaterialIDPropertyAttribute : InspectablePropertyAttribute
{
    public InspectableMaterialIDPropertyAttribute(string displayName, string tooltip) : base(displayName, tooltip)
    {
    }
}
