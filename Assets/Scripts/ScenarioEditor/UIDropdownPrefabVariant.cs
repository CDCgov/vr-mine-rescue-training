using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TMPro.TMP_Dropdown;

public class UIDropdownPrefabVariant : UIDropdownBase
{
    public Placer Placer;
    public LoadableAssetManager LoadableAssetManager;

    private GameObject _selectedObject = null;
    private LoadableAsset _loadableAsset;
    private ObjectInfo _selObjInfo;
    private LoadableVariantSet _variantSet;


    public class VariantOptionData : TMP_Dropdown.OptionData
    {
        public LoadableAsset LoadableAsset;
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
        _variantSet = null;

        if (obj == null || LoadableAssetManager == null)
            return;

        if (!obj.TryGetComponent<ObjectInfo>(out var objInfo))
            return;

        _loadableAsset = LoadableAssetManager.FindAsset(objInfo.AssetID);
        if (_loadableAsset != null)
        {
            _selectedObject = obj;
            _selObjInfo = objInfo;
            _variantSet = LoadableAssetManager.GetVariantSet(objInfo.AssetID);
        }
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (_loadableAsset == null || _variantSet == null
            || _selObjInfo == null)
            return 0;

        for (int i = 0; i < _variantSet.LoadableVariants.Count; i++)
        {
            if (_variantSet.LoadableVariants[i].AssetID == _selObjInfo.AssetID)
                return i;
        }

        return 0;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        //var variantSet = LoadableAssetManager.GetVariantSet(_loadableAsset.AssetID);
        if (_variantSet == null)
        {
            yield return new VariantOptionData
            {
                LoadableAsset = null,
                text = "Default",
            };
            yield break;
        }
        
        foreach (var prefab in _variantSet.LoadableVariants)
        {
            yield return new VariantOptionData
            {
                LoadableAsset = prefab,
                text = prefab.AssetWindowName,
            };
        }
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (Placer == null ||
            _selObjInfo == null || _loadableAsset == null ||
            LoadableAssetManager == null)
            return;

        var customData = (VariantOptionData)data;
        if (customData.LoadableAsset == null)
            return;

        var obj = LoadableAssetManager.ReplaceObjectVariant(_selObjInfo, customData.LoadableAsset);
        Placer.SelectObject(obj);
    }

    // Start is called before the first frame update
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

}
