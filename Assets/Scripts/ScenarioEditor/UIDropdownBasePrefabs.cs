using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownBasePrefabs : UIDropdownBase
{
    public Placer Placer;
    public LoadableAssetManager LoadableAssetManager;

    private GameObject _selectedObject = null;
    private LoadableAsset _loadableAsset;
    private ObjectInfo _selObjInfo;

    protected override void Start()
    {
        base.Start();

        if (Placer == null)
            Placer = Placer.GetDefault();
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        Placer.SelectedObjectChanged += OnSelectedObjectChanged;

        if (Placer.SelectedObject != null)
            OnSelectedObjectChanged(Placer.SelectedObject);
    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        SelectObject(obj);
        UpdateDropdownOptions();
        UpdateDropdownSelection();
    }

    private void SelectObject(GameObject obj)
    {
        _selectedObject = null;
        _selObjInfo = null;
        _loadableAsset = null;

        if (obj == null || LoadableAssetManager == null)
            return;        

        if (!obj.TryGetComponent<ObjectInfo>(out var objInfo))
            return;

        _loadableAsset = LoadableAssetManager.FindAsset(objInfo.AssetID);
        if (_loadableAsset != null)
        {
            _selectedObject = obj;
            _selObjInfo = objInfo;

            int basePrefabCount = 0;
            if (_loadableAsset.BasePrefabs != null)
                basePrefabCount = _loadableAsset.BasePrefabs.Count;

            Debug.Log($"BasePrefabDropdown: Selected object with asset ID {_selObjInfo.AssetID} and {basePrefabCount} base prefabs");
        }
    }

    public class OptionData : TMP_Dropdown.OptionData
    {
        public LoadableBasePrefabData BasePrefabData;
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (_loadableAsset == null || _loadableAsset.BasePrefabs == null || _loadableAsset.BasePrefabs.Count <= 0 
            || _selObjInfo == null)
            return 0;

        for (int i = 0; i < _loadableAsset.BasePrefabs.Count; i++)
        {
            if (_loadableAsset.BasePrefabs[i].PrefabID == _selObjInfo.BasePrefabID)
                return i;
        }

        return 0;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        if (_loadableAsset == null || _loadableAsset.BasePrefabs == null || _loadableAsset.BasePrefabs.Count <= 0)
        {
            yield return new OptionData
            {
                BasePrefabData = null,
                text = "Default",
            };
            yield break;
        }

        foreach (var basePrefab in _loadableAsset.BasePrefabs)
        {
            yield return new OptionData
            {
                BasePrefabData = basePrefab,
                text = basePrefab.DisplayName,
            };
        }

    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (Placer == null ||
            _selObjInfo == null || _loadableAsset == null || 
            LoadableAssetManager == null)
            return;

        var customData = (OptionData)data;
        if (customData.BasePrefabData == null)
            return;

        var obj = LoadableAssetManager.ReplaceObjectBasePrefab(_selObjInfo, customData.BasePrefabData);
        Placer.SelectObject(obj);
    }
}
