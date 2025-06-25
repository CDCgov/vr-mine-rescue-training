using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleCameraLayer : MonoBehaviour
{
    public Camera Camera;
    public string LayerName;

    private Toggle _toggle;
    private int _layer;
    private int _mask;

    void Start()
    {
        _layer = LayerMask.NameToLayer(LayerName);
        _mask = LayerMask.GetMask(LayerName);
        _toggle = GetComponent<Toggle>();

        _toggle.onValueChanged.AddListener(OnToggleChanged);

        if (Camera == null)
            return;

        _toggle.SetIsOnWithoutNotify(IsLayerOn());
    }

    private bool IsLayerOn()
    {
        return (Camera.cullingMask & _mask) > 0;
    }

    private void OnToggleChanged(bool val)
    {
        if (Camera == null)
            return;

        if (val)
            Camera.cullingMask = Camera.cullingMask | _mask;
        else
            Camera.cullingMask = Camera.cullingMask & (~_mask);
    }
}
