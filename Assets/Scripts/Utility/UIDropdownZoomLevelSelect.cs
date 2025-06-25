using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropdownZoomLevelSelect : MonoBehaviour
{
    public UIMousePanAndZoom PanAndZoomController;
    private TMP_Dropdown _dropdown;

    private List<TMP_Dropdown.OptionData> _dropdownOptions;

    private class ZoomOption : TMP_Dropdown.OptionData
    {
        public ZoomOption(string text, float zoom)
        {
            this.text = text;
            this.ZoomLevel = zoom;
        }

        public float ZoomLevel;
    }

    // Start is called before the first frame update
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();


        _dropdownOptions = new List<TMP_Dropdown.OptionData>();

        _dropdownOptions.Add(new ZoomOption("40%", 0.4f));
        _dropdownOptions.Add(new ZoomOption("60%", 0.6f));
        _dropdownOptions.Add(new ZoomOption("80%", 0.8f));
        _dropdownOptions.Add(new ZoomOption("100%", 1.0f));
        _dropdownOptions.Add(new ZoomOption("120%", 1.2f));
        _dropdownOptions.Add(new ZoomOption("140%", 1.4f));
        _dropdownOptions.Add(new ZoomOption("160%", 1.6f));

        _dropdown.ClearOptions();
        _dropdown.AddOptions(_dropdownOptions);
        _dropdown.value = 3;

        _dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int option)
    {
        if (PanAndZoomController == null)
            return;

        var zoomOpt = _dropdownOptions[option] as ZoomOption;
        if (zoomOpt != null)
        {
            PanAndZoomController.SetZoom(zoomOpt.ZoomLevel);
        }
    }
}
