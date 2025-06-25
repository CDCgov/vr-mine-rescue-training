using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIInputFieldVentFireProp : MonoBehaviour
{
    public enum VentFireProp
    {
        ContamFlowRate,
        ContamConcentration,
        HeatInput,
        TransitionTime,
    }

    public VentilationManager VentilationManager;

    public VentFireProp EditedProperty;
    private TMP_InputField _inputField;

    private VentFire _selectedFire;


    void Start()
    {

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _inputField = GetComponent<TMP_InputField>();
        _inputField.onValueChanged.AddListener(OnValueChanged);

        VentilationManager.SelectedVentObjChanged += OnSelectionChanged;

    }

    private void OnSelectionChanged(VentUIObj obj)
    {
        if (obj == null || !(obj.VentObj is VentFire))
        {
            _selectedFire = null;
            _inputField.text = "";
            return;
        }

        var fire = obj.VentObj as VentFire;
        _selectedFire = fire;

        string txt = "Unknown";

        switch (EditedProperty)
        {
            case VentFireProp.ContamConcentration:
                txt = fire.MFFire.ContamConcentration.ToString("F2");
                break;

            case VentFireProp.ContamFlowRate:
                txt = fire.MFFire.ContamFlowRate.ToString("F2");
                break;

            case VentFireProp.HeatInput:
                txt = fire.MFFire.HeatInput.ToString("F2");
                break;

            case VentFireProp.TransitionTime:
                txt = fire.MFFire.TransitionTime.ToString("F3");
                break;
        }

        _inputField.text = txt;
    }

    private void OnValueChanged(string newText)
    {
        if (_selectedFire == null)
            return;

        float val;
        if (!float.TryParse(newText, out val))
            return;

        switch (EditedProperty)
        {
            case VentFireProp.ContamConcentration:
                _selectedFire.MFFire.ContamConcentration = val;
                break;

            case VentFireProp.ContamFlowRate:
                _selectedFire.MFFire.ContamFlowRate = val;
                break;

            case VentFireProp.HeatInput:
                _selectedFire.MFFire.HeatInput = val;
                break;

            case VentFireProp.TransitionTime:
                _selectedFire.MFFire.TransitionTime = val;
                break;
        }
    }
}
