using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBtnMFireControl : MonoBehaviour
{
    public enum MFireControlAction
    {
        Start,
        Stop,
        Toggle,
        AdvanceSimulation,
        Reset,
        ShowUI,
        HideUI,
        ToggleUI,
        SetResistHigh,
        SetResistMed,
        SetResistLow,
    }

    public VentilationManager VentilationManager;
    public MFireControlAction ButtonAction;

    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
    }

    private void OnButtonClicked()
    {
        Debug.Log("UIBtnMFireControl Clicked");
        if (VentilationManager == null)
            return;

        switch (ButtonAction)
        {
            case MFireControlAction.Start:
                VentilationManager.AutoAdvanceEnabled = true;
                break;

            case MFireControlAction.Stop:
                VentilationManager.AutoAdvanceEnabled = false;
                break;

            case MFireControlAction.Toggle:
                VentilationManager.AutoAdvanceEnabled = !VentilationManager.AutoAdvanceEnabled;
                break;

            case MFireControlAction.AdvanceSimulation:
                VentilationManager.AdvanceSimulation();
                break;

            case MFireControlAction.Reset:
                VentilationManager.ResetSimulation();
                break;

            case MFireControlAction.ShowUI:
                VentilationManager.ShowVentUI(true);
                break;

            case MFireControlAction.HideUI:
                VentilationManager.ShowVentUI(false);
                break;

            case MFireControlAction.ToggleUI:
                VentilationManager.ShowVentUI(!VentilationManager.IsUIVisible);
                break;

            case MFireControlAction.SetResistHigh:
                if (VentilationManager.SelectedVentUIObj == null)
                    break;
                VentilationManager.SelectedVentUIObj.SetResistance(10000);
                break;

            case MFireControlAction.SetResistMed:
                if (VentilationManager.SelectedVentUIObj == null)
                    break;
                VentilationManager.SelectedVentUIObj.SetResistance(1000);
                break;

            case MFireControlAction.SetResistLow:
                if (VentilationManager.SelectedVentUIObj == null)
                    break;
                VentilationManager.SelectedVentUIObj.SetResistance(float.MinValue);
                break;
        }
    }
    
}
