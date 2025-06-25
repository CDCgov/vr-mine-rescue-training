using MFireProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBtnVentFireControl : MonoBehaviour
{

    public VentilationManager VentilationManager;
    public VentFireData FireData;
    public bool ApplyToAllFires = true;

    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
    }

    private void OnButtonClicked()
    {
        if (VentilationManager == null)
            return;

        if (ApplyToAllFires)
        {
            var graph = VentilationManager.GetVentilationGraph();
            if (graph != null)
                graph.SetAllFires(FireData);
        }
        else if (VentilationManager.SelectedVentUIObj != null)
        {
            var fire = VentilationManager.SelectedVentUIObj.VentObj as VentFire;
            if (fire != null)
                fire.SetFireData(FireData);
        }
    }
}
