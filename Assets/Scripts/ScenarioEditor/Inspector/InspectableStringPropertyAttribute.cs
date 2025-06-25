using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectableStringPropertyAttribute : InspectablePropertyAttribute
{
    public InspectableStringPropertyAttribute(string displayName, string tooltip = null) : base(displayName, tooltip)
    {

    }
}
