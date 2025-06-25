using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIBtnVentFanControl : MonoBehaviour
{

    public VentilationManager VentilationManager;
    public VentFanData FanData;
    public bool ApplyToAllFans = true;



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

        if (ApplyToAllFans)
        {
            var graph = VentilationManager.GetVentilationGraph();
            if (graph != null)
                graph.SetAllFans(FanData);
        }
        else if (VentilationManager.SelectedVentUIObj != null)
        {
            var fan = VentilationManager.SelectedVentUIObj.VentObj as VentFan;
            if (fan != null)
                fan.SetFanData(FanData);
        }
    }
}
