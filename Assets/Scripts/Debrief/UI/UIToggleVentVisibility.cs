using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleVentVisibility : MonoBehaviour
{
    public VentilationManager VentilationManager;

    public Camera Camera;
    
    private Toggle _toggle;
    private int _layer;
    private int _mask;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();
        _layer = LayerMask.NameToLayer("VentVisualization");
        _mask = LayerMask.GetMask("VentVisualization");

        _toggle.SetIsOnWithoutNotify(false);
        Camera.cullingMask = Camera.cullingMask & (~_mask);
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value)
    {
        if (value)
        {
            VentilationManager.AddVentVisualizer(gameObject.name);
            Camera.cullingMask = Camera.cullingMask | _mask;
        }
        else
        {
            VentilationManager.RemoveVentVisualizer(gameObject.name);
            Camera.cullingMask = Camera.cullingMask & (~_mask);
        }
    }
}
