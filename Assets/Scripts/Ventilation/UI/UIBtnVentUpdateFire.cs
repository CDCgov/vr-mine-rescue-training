using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIBtnVentUpdateFire : MonoBehaviour
{

    public VentilationManager VentilationManager;
    


    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (VentilationManager == null)
            return;

        if (VentilationManager.SelectedVentUIObj == null)
            return;

        var fire = VentilationManager.SelectedVentUIObj.VentObj as VentFire;
        if (fire == null)
            return;

        fire.UpdateFire();
    }
}
