using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using System;

[RequireComponent(typeof(TMPro.TMP_Dropdown))]
public class UIDropdownVentVisData : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public VentilationManager VentilationManager;
    
    public bool SendToClients = false;

    public class OptionData : TMP_Dropdown.OptionData
    {
        public string PrimaryKey;

        public OptionData(string primaryKey)
        {
            PrimaryKey = primaryKey;

            var splitKey = primaryKey.Split('\\', '/');
            if (splitKey == null || splitKey.Length <= 0)
            {
                this.text = "???";
                return;
            }

            this.text = splitKey[splitKey.Length - 1];
        }

        public override string ToString()
        {
            return PrimaryKey;
        }
    }

    private TMP_Dropdown _dropdown;

    // Start is called before the first frame update
    async void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _dropdown = GetComponent<TMP_Dropdown>();

        if (_dropdown.options == null)
            _dropdown.options = new List<TMP_Dropdown.OptionData>();

        _dropdown.options.Clear();
        //_dropdown.options.Add(new OptionData("None"));

        var effects = await Addressables.LoadResourceLocationsAsync("VentVisData").Task;

        foreach (var effect in effects)
        {
            //Debug.Log($"VentVisData: {effect.PrimaryKey}");

            _dropdown.options.Add(new OptionData(effect.PrimaryKey));
        }


        _dropdown.onValueChanged.AddListener(OnSelectionChanged);

        _dropdown.RefreshShownValue();
        OnSelectionChanged(0);
    }

    private void OnSelectionChanged(int sel)
    {
        if (_dropdown == null || _dropdown.options == null || _dropdown.options.Count < sel)
            return;

        OptionData data = (OptionData)_dropdown.options[sel];

        Debug.Log($"Selected VentVisData {data.PrimaryKey}");

        if (SendToClients)
        {
            VRNVentVisualization ventVis = new VRNVentVisualization();
            ventVis.ToggleAction = VRNToggleAction.ToggleOn;
            ventVis.VisData = data.PrimaryKey;
            NetworkManager.SendNetMessage(VRNPacketType.SetVentVisualization, ventVis);
        }
        else
        {
            VentilationManager.SetVisData(data.PrimaryKey);
        }
    }

}
