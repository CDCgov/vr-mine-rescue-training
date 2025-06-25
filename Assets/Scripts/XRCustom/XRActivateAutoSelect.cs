using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRActivateAutoSelect : XRSimpleInteractable
{
    //public bool isSelected = true;

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        return true;
    }
     
}
