using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBtnSetActive : UIButtonBase
{
    public GameObject TargetObject;
    public bool SetActive = false;
    public bool ToggleState = false;

    protected override void OnButtonClicked()
    {
        if (TargetObject == null)
            return;

        if (ToggleState)
            TargetObject.SetActive(!TargetObject.activeSelf);
        else
            TargetObject.SetActive(SetActive);
    }
}
