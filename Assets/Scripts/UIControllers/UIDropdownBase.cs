using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMPro.TMP_Dropdown))]
public abstract class UIDropdownBase : MonoBehaviour
{
    protected TMP_Dropdown _dropdown;

    protected abstract int GetCurrentSelectedIndex();
    protected abstract IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions();
    protected abstract void OnSelectionChanged(TMP_Dropdown.OptionData data);
    
    protected virtual void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();

        if (_dropdown.options == null)
            _dropdown.options = new List<TMP_Dropdown.OptionData>();

        UpdateDropdownOptions();
        _dropdown.onValueChanged.AddListener(OnSelectionChanged);

        UpdateDropdownSelection();
    }

    protected virtual void OnDestroy()
    {
        if (_dropdown != null)
            _dropdown.onValueChanged.RemoveListener(OnSelectionChanged);
    }

    protected virtual void UpdateDropdownOptions()
    {
        if (_dropdown == null)
            return;

        _dropdown.options.Clear();

        foreach (var option in GetDropdownOptions())
        {
            _dropdown.options.Add(option);
        }
    }

    protected virtual void UpdateDropdownSelection()
    {
        if (_dropdown == null)
            return;

        int sel = GetCurrentSelectedIndex();
        _dropdown.SetValueWithoutNotify(sel);
        _dropdown.RefreshShownValue();
    }


    private void OnSelectionChanged(int sel)
    {
        if (_dropdown == null || _dropdown.options == null ||
            _dropdown.options.Count < sel)
            return;

        TMP_Dropdown.OptionData data = _dropdown.options[sel];

        OnSelectionChanged(data);
    }

}
