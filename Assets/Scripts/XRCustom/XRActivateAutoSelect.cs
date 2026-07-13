using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class XRActivateAutoSelect : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{
    //public bool isSelected = true;

    public override bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        return true;
    }
     
}
