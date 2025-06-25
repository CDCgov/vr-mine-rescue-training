using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

public class UIDropdownScenarioSkybox : UIDropdownBase
{
    public class OptionData : TMP_Dropdown.OptionData
    {
        public SkyboxData SkyboxData;
    }

    private List<OptionData> _options;

    private string _selectedSkybox;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(UpdateOptionsList());
    }

    private void OnEnable()
    {
        UpdateDropdownSelection();
        Debug.Log("UIDropdownScenarioSkybox: Enabled");
    }

    private void OnDisable()
    {
        Debug.Log("UIDropdownScenarioSkybox: Disabled");
    }

    private IEnumerator UpdateOptionsList()
    {
        var options = new List<OptionData>();

        yield return Util.LoadAddressablesByLabel<SkyboxData>("SkyboxData", (data) =>
        {
            options.Add(new OptionData
            {
                SkyboxData = data,
                text = data.Name,
            });
        });

        options.Sort((a, b) =>
        {
            if (a.SkyboxData.SortOrder == b.SkyboxData.SortOrder)
                return 0;

            if (a.SkyboxData.SortOrder > b.SkyboxData.SortOrder)
                return 1;
            else
                return 0;
        });

        _options = options;

        UpdateDropdownOptions();
        UpdateDropdownSelection();
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (_options == null)
            return 0;

        if (ScenarioSaveLoad.Instance != null && ScenarioSaveLoad.Settings != null)
            _selectedSkybox = ScenarioSaveLoad.Settings.SkyboxID;

        for (int i = 0; i < _options.Count; i++)
        {
            if (_options[i].SkyboxData.SkyboxID == _selectedSkybox)
                return i;
        }

        return 0;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        if (_options == null)
            yield break;

        foreach (var option in _options)
        {
            yield return option;
        }
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (_options == null || data == null)
            return;

        _selectedSkybox = ((OptionData)data).SkyboxData.SkyboxID;

        if (ScenarioSaveLoad.Instance != null && ScenarioSaveLoad.Settings != null)
        {
            //ScenarioSaveLoad.Instance.Settings.SkyboxID = _selectedSkybox;
            ScenarioSaveLoad.Instance.SetSkyboxID(_selectedSkybox);
        }
    }
}
