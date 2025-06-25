using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInputFireContamConc : UIInspectorInputFieldBase
{
    public ComponentInspector_VentFire VentFireInspector;

    public override void Start()
    {
        if (VentFireInspector == null)
            VentFireInspector = GetComponentInParent<ComponentInspector_VentFire>();

        base.Start();
    }

    protected override string GetComponentValue()
    {
        if (VentFireInspector == null)
            return "";

        return VentFireInspector.TargetComponentInfo.ContamConcentration.ToString("F3");
    }

    protected override bool UpdateComponentValue(string value)
    {
        if (VentFireInspector == null)
            return false;

        if (!double.TryParse(value, out double dblVal))
            return false;

        VentFireInspector.TargetComponentInfo.ContamConcentration = dblVal;
        return true;
    }
}
