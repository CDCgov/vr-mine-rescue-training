using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectableColorPropertyAttribute : InspectablePropertyAttribute
{
   public InspectableColorPropertyAttribute(string displayName, string tooltip = null) : base(displayName, tooltip)
    {

    }
}
