using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputVentResistOverride : MonoBehaviour
{
    public VentilationManager VentilationManager;
    private TMP_InputField _input;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _input = GetComponent<TMP_InputField>();
        _input.onValueChanged.AddListener(OnValueChanged);

        VentilationManager.SelectedVentObjChanged += OnSelectedVentObjChanged;
    }

    private void OnDestroy()
    {
        if(VentilationManager != null)
        {
            VentilationManager.SelectedVentObjChanged -= OnSelectedVentObjChanged;
        }
    }

    private void OnSelectedVentObjChanged(VentUIObj obj)
    {
        if (obj == null)
            return;

        var airway = obj.VentObj as VentAirway;

        if (airway == null)
            return;

        _input.SetTextWithoutNotify(airway.OverrideResistance.ToString("F3"));
    }

    private void OnValueChanged(string val)
    {
        float resist;

        if (float.TryParse(_input.text, out resist))
            VentilationManager.SelectedVentUIObj.SetResistance(resist);
    }
}
