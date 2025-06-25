using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIDropdownSetGasZoneColor : UIDropdownBase, ISelectedSubGasZoneView
{

    private ComponentInfo_StaticGasZone _gasZone;
    private SubGasZoneData _subGasZoneData;
    private IndexedColorList _colors;


    public class OptionData : TMP_Dropdown.OptionData
    {
        public IndexedColorData ColorData;
        public int Index;

        public OptionData(IndexedColorData data, int index)
        {
            ColorData = data;
            Index = index;
            this.text = data.Name;
        }
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        if (_colors == null || _colors.IndexedColors == null)
            yield break;

        for (int i = 0; i < _colors.IndexedColors.Count; i++)
        {
            var colorData = _colors.IndexedColors[i];
            //_dropdown.options.Add(new OptionData(colorData, i));
            yield return new OptionData(colorData, i);
        }
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (_subGasZoneData != null)
            return _subGasZoneData.Color;
        else
            return 0;
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (_gasZone == null || _subGasZoneData == null)
            return;

        OptionData customData = (OptionData)data;

        _subGasZoneData.Color = customData.Index;
        _subGasZoneData.ChangeColor(customData.ColorData.Color);
    }

    public void SetSubGasZone(ComponentInfo_StaticGasZone gasZone, SubGasZoneData data)
    {
        if (gasZone == null || data == null)
            return;

        _gasZone = gasZone;
        _subGasZoneData = data;
        _colors = gasZone.GasZoneColors;

        if (_dropdown != null)
        {
            UpdateDropdownOptions();
            UpdateDropdownSelection();
        }
    }
}
