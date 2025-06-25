using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropDownPanelSelect : MonoBehaviour
{

    [System.Serializable]
    public class PanelOption : TMP_Dropdown.OptionData
    {
        public GameObject Panel;
    }

    public List<PanelOption> PanelOptions;

    private TMP_Dropdown _dropdown;    

    // Start is called before the first frame update
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();

        _dropdown.options.Clear();

        if (PanelOptions == null || PanelOptions.Count <= 0)
            return;

        foreach (var opt in PanelOptions)
        {
            _dropdown.options.Add(opt);
        }        
        //_dropdown.AddOptions(PanelOptions);

        _dropdown.onValueChanged.AddListener(OnDropdownChanged);

        _dropdown.SetValueWithoutNotify(-1);
        _dropdown.value = 0;

        UpdateSelectedPanel(0);
    }

    private void OnDestroy()
    {
        if (_dropdown != null)
            _dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int optIndex)
    {
        UpdateSelectedPanel(optIndex);
    }

    private void UpdateSelectedPanel(int selIndex)
    {
        for (int i = 0; i < _dropdown.options.Count; i++)
        {
            PanelOption opt = (PanelOption)_dropdown.options[i];
            if (opt.Panel == null)
                continue;

            if (selIndex == i)
                opt.Panel.SetActive(true);
            else
                opt.Panel.SetActive(false);
        }
    }
}
