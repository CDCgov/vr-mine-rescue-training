using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMineMapDropdown : MonoBehaviour
{
    public MineMapSymbolManager MineMapSymManager;
    public MineMapManager MineMapManager;
    public SessionPlaybackControl SessionPlaybackControl;
    public TMP_Dropdown MapDropdown;

    private bool _mapLoaded = false;
    private int _selection = 0;
    private List<Guid> _symbolManagerGuids;
    private int _dropdownCount = 0;

    private VRNSymbolManagerState _lastSymbolState;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        if (MineMapManager == null)
            MineMapManager = MineMapManager.GetDefault(gameObject);

        _symbolManagerGuids = new List<Guid>();
        SessionPlaybackControl.SessionScrubbed += OnSessionScrubbed;
        MineMapManager.MapLoaded += PopulateDropdown;
        MapDropdown.onValueChanged.AddListener(DropdownChanged);
        //SessionPlaybackControl.
    }

    private void DropdownChanged(int arg0)
    {
        _selection = MapDropdown.value;
        OnSessionScrubbed();
    }

    private void OnSessionScrubbed()
    {
        if (MineMapManager == null || MineMapManager.SymbolManagers == null || MineMapManager.SymbolManagers.Values.Count <= 0)
            return;
        //Debug.Log($"Populate dropdown count: {MineMapManager.SymbolManagers.Count}");
        if (MineMapManager.SymbolManagers.Count != _dropdownCount)
        {
            PopulateDropdown();
        }
        else
        {
            MineMapData data = MineMapManager.SymbolManagers[_symbolManagerGuids[_selection]];
            if (data != null && _lastSymbolState != data.SymbolState)
            {
                MineMapSymManager.LoadFromSerializedState(data.SymbolState);
                _lastSymbolState = data.SymbolState;
            }
        }
    }

    private void PopulateDropdown()
    {
        MapDropdown.ClearOptions();
        _symbolManagerGuids.Clear();
        List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();
        Debug.Log($"Populate dropdown count: {MineMapManager.SymbolManagers.Count}");
        int index = 1;
        foreach(KeyValuePair<Guid,MineMapData> kvp in MineMapManager.SymbolManagers)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            if(kvp.Value.SymbolManagerName == "")
            {
                optionData.text = "Map Man " + index;
                index++;
            }
            else
                optionData.text = kvp.Value.SymbolManagerName;
            Debug.Log($"Name?: {kvp.Value.SymbolManagerName}, ID: {kvp.Value.SymbolManagerID}");
            optionList.Add(optionData);
            _symbolManagerGuids.Add(kvp.Key);
            Debug.Log($"Map found: {kvp.Value.SymbolManagerName}");
        }
        MapDropdown.AddOptions(optionList);
        MapDropdown.SetValueWithoutNotify(0);
        _selection = 0;
        _dropdownCount = MineMapManager.SymbolManagers.Count;
    }
}
