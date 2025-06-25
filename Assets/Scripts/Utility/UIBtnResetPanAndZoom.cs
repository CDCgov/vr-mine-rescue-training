using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnResetPanAndZoom : MonoBehaviour
{
    public UIMousePanAndZoom PanAndZoomController;
    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (PanAndZoomController == null)
            return;

        PanAndZoomController.ResetTransform();

    }

}
