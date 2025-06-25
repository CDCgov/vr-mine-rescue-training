using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownMaterialID : UIDropdownBase
{
    public MaterialManager MaterialManager;

    public System.Action<string> ValueChanged;

    private GameObject _selectedObject = null;
    private LoadableAsset _loadableAsset;
    private ObjectInfo _selObjInfo;

    private List<OptionData> _options;
    private string _selectedMaterialID = null;

    protected override void Start()
    {
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);

        base.Start();
    }

    public class OptionData : TMP_Dropdown.OptionData
    {
        public string MaterialID;
    }

    public void SetSelected(string materialID)
    {
        _selectedMaterialID = materialID;
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (_options == null)
            return 0;

        for (int i = 0; i < _options.Count; i++)
        {
            if (_options[i].MaterialID == _selectedMaterialID)
            {
                return i;
            }
        }

        return 0;
    } 

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        _options = new List<OptionData>();

        _options.Add(new OptionData
        {
            MaterialID = "",
            text = "None",
        });

        foreach (var mat in MaterialManager.GetAllMaterials())
        {
            _options.Add(new OptionData
            {
                MaterialID = mat.MaterialID,
                text = mat.MaterialID,
            });
        }

        foreach (var opt in _options)
            yield return opt;
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        OptionData opt = (OptionData)data;
        ValueChanged?.Invoke(opt.MaterialID);
    }
}
