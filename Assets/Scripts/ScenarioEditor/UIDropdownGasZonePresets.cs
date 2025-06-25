using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownGasZonePresets : UIDropdownBase, ISelectedSubGasZoneView
{
    private ComponentInfo_StaticGasZone _gasZone;
    private SubGasZoneData _subGasZoneData;
    private GasZonePresetList _presets;

    private int _selectedIndex = 0;

    public class OptionData : TMP_Dropdown.OptionData
    {
        public GasZonePresetData PresetData;
        public int Index;

        public OptionData(GasZonePresetData data, int index)
        {
            PresetData = data;
            Index = index;
            this.text = data.Name;
        }
    }

    public void SetSubGasZone(ComponentInfo_StaticGasZone gasZone, SubGasZoneData data)
    {
        if (gasZone == null || data == null)
            return;

        _gasZone = gasZone;
        _subGasZoneData = data;
        _presets = _gasZone.GasZonePresets;

        if (_dropdown != null)
        {
            UpdateDropdownOptions();
            UpdateDropdownSelection();
        }
    }

    protected override int GetCurrentSelectedIndex()
    {
        return _selectedIndex;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        if (_presets == null || _presets.GasZonePresets == null)
            yield break;

        var custom = new GasZonePresetData()
        {
            MineAtmosphere = null,
            Name = "Custom",
        };

        yield return new OptionData(custom, -1);

        for (int i = 0; i < _presets.GasZonePresets.Count; i++)
        {
            var presetData = _presets.GasZonePresets[i];
            yield return new OptionData(presetData, i);
        }
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (_subGasZoneData == null || _gasZone == null)
            return;

        var customData = (OptionData)data;
        if (customData.PresetData.MineAtmosphere == null)
            return;

        _selectedIndex = customData.Index;

        _subGasZoneData.SetGasValuePreset(customData.PresetData.MineAtmosphere.MineAtmosphere);
    }
}
