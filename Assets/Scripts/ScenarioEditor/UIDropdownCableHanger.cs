using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownCableHanger : UIDropdownBase
{
    public LoadableAssetManager LoadableAssetManager;
    public ComponentInspector_Cable CableInspector;

    public class OptionData : TMP_Dropdown.OptionData
    {
        public CableHangerData Data;
        public int Index;

        public OptionData(CableHangerData data, int index)
        {
            Data = data;
            Index = index;
            this.text = data.Name;
        }
    }

    protected override void Start()
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        if (CableInspector == null)
            CableInspector = transform.GetComponentInParent<ComponentInspector_Cable>();

        if (CableInspector == null)
        {
            Debug.LogError($"Couldn't find CableInspector on UIDropdownCableHanger");
        }

        base.Start();
    }

    protected override int GetCurrentSelectedIndex()
    {
        if (CableInspector == null)
            return 0;

        var selectedID = CableInspector.TargetComponentInfo.CableHangerID;

        for (int i = 0; i < LoadableAssetManager.CableHangerData.Count; i++)
        {
            if (selectedID == LoadableAssetManager.CableHangerData[i].CableHangerID)
            {
                return i;
            }
        }

        return 0;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        if (CableInspector == null)
            yield break;

        for (int i = 0; i < LoadableAssetManager.CableHangerData.Count; i++)
        {
            yield return new OptionData(LoadableAssetManager.CableHangerData[i], i);
        }
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        if (CableInspector == null)
            return;

        OptionData cableOption = (OptionData)data;

        CableInspector.TargetComponentInfo.SetHangerType(cableOption.Data.CableHangerID);
    }
}
