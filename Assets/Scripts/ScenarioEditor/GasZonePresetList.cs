using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct GasZonePresetData
{
    public StaticMineAtmosphere MineAtmosphere;
    public string Name;
}

[CreateAssetMenu(fileName = "GasZonePresetList", menuName = "VRMine/GasZonePresetList", order = 0)]
public class GasZonePresetList : ScriptableObject
{
    public List<GasZonePresetData> GasZonePresets;

    public GasZonePresetData Default;


    void Reset()
    {
        Default = new GasZonePresetData
        {
            MineAtmosphere = null,
            Name = "Error",
        };
    }

    public GasZonePresetData GetPresetData(int index)
    {
        if (GasZonePresets == null || index < 0 || index >= GasZonePresets.Count)
            return Default;

        return GasZonePresets[index];
    }
}
