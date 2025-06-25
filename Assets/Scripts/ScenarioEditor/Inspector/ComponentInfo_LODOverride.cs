using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MinerColorChanger;

public class ComponentInfo_LODOverride : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    [InspectableStringProperty("LOD Override", "Name of the LOD group to use at all times in place of normal LOD changes")]
    public string LODOverrideID
    {
        get
        {
            return _lodOVerrideID;
        }
        set
        {
            _lodOVerrideID = value;

            OverrideLOD(_lodOVerrideID);
        }
    }

    public string ComponentInspectorTitle => "LOD Override";

    private string _lodOVerrideID;

    public void Start()
    {

        if (_lodOVerrideID != null && _lodOVerrideID.Length > 0)
            OverrideLOD(_lodOVerrideID);

    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        _lodOVerrideID = component.GetParamValueString("LODOverrideID", null);
    }

    public string[] SaveInfo()
    {
        if (_lodOVerrideID == null)
            _lodOVerrideID = "";

        return new string[]
        {
            "LODOverrideID|" + _lodOVerrideID,
        };
    }

    public string SaveName()
    {
        return "LODOverride";
    }

    private void ClearOverride()
    {
        OverrideLOD(null);
    }

    private void OverrideLOD(string lodOVerrideID)
    {
        //var lodGroup = gameObject.GetComponentInChildren<LODGroup>();
        //if (lodGroup == null)
        //    return;

        //if (lodOVerrideID == null || lodOVerrideID == "")
        //    lodGroup.enabled = true;
        //else
        //    lodGroup.enabled = false;

        bool enableLODGroup = false;

        if (lodOVerrideID == null || lodOVerrideID == "")
            enableLODGroup = true;

        var lodGroups = gameObject.GetComponentsInChildren<LODGroup>();
        foreach (var lodGroup in lodGroups)
        {
            lodGroup.enabled = enableLODGroup;
        }

        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (renderers == null)
            return;

        foreach (var rend in renderers)
        {
            if (lodOVerrideID == null || lodOVerrideID == "" ||
                rend.gameObject.name.Contains(lodOVerrideID))
            {
                rend.enabled = true;
            }
            else
            {
                rend.enabled = false;
            }
        }
    }

}
