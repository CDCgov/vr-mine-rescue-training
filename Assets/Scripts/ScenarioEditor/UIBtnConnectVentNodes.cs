using NIOSH_MineCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnConnectVentNodes : UIButtonBase
{
    protected override void OnButtonClicked()
    {
        var manager = FindObjectOfType<VentLayerManager>();
        if (manager == null)
            return;

        StartCoroutine(manager.AutoConnectVentNodes());
    }
} 
