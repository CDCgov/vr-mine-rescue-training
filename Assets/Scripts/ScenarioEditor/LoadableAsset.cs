using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;
using UnityEngine.Serialization;
using System.ComponentModel;

public enum LoadableAssetSource
{
    Unknown = 0,
    Internal,
    Bundle,
    External,
}

public enum PlacementSnapMode
{
    None = 0,
    Grid,
    TileOrigin,
    RoofBolt,
}

[System.Serializable]
public class LoadablePlacementOptions
{
    [Description("Layer the asset should appear on (i.e, Mine, Object, LoadOnly, SceneControls, Ventilation, VentilationBlockers, Cables, Curtains)")]
    public LayerManager.EditorLayer EditorLayer;
    [Description("Should the asset appear in the asset library")]
    public bool ShowInAssetWindow = true;
}

[System.Serializable]
public class LoadablePhysicalProperties
{
    public float Mass;
    public float AirflowResistance;
    public float Density;
    public float Drag;
    public float AngularDrag;    
}

[System.Serializable]
public class LoadableAudioProperties
{
    [Description("Audio material type for collision sounds e.g. Stone, Wood, Dirt, Cloth, Plastic, Metal, Water")]
    public AudioMaterial AudioMaterial;
    //public bool UseVolumeOverride;
    //public float VolumeOverrideValue;
    //public bool UsePitchOverride;
    //public float PitchOverrideValue;
    public bool UseSizeOverride = false;
    public Bounds SizeOverride;
}

[System.Flags]
public enum LoadableAssetCategories : uint
{
    None                = 0,
    Object              = 0b1,
    MineTile            = 0b10,
    Ventilation         = 0b100,
    ScannedEnvironment  = 0b1000,
    Cable               = 0b10000,

    All                 = 0xFFFFFFFF
}

[CreateAssetMenu(menuName = "Scenario Editor/LoadableAsset")]
public class LoadableAsset : ScriptableObject
{
    public string AssetID;
    public LoadableAssetSource Source;

    [System.NonSerialized]
    public ExternalAssetMetadata ExternalAssetMetadata;
 
    public GameObject EditorObject;

    [FormerlySerializedAs("LogicObject")]
    public GameObject SceneObject;

    public GameObject GeometryObject;
    public List<LoadableBasePrefabData> BasePrefabs;

    public string AssetWindowName;
    public Sprite Icon;
    
    public LayerManager.EditorLayer EditorLayer;
    public bool ShowInAssetWindow = true;
    public bool AllowNavMeshModifiers = true;
    public LoadablePlacementOptions PlacementOptions;

    public LoadablePhysicalProperties PhysicalProperties;
    public LoadableAudioProperties AudioProperties;

    public LoadableAssetCategories AssetCategories;
    public HashSet<string> Tags;

    public string TooltipInfo;

    //public GameObject GetAsset(bool isEditor)
    //{
    //    if(isEditor)
    //    {
    //        return EditorObject;
    //    }
    //    else
    //    {
    //        return LogicObject;
    //    }
    //}

    public GameObject GetPrefab(bool editorMode)
    {
        if (editorMode)
            return EditorObject;
        else
            return SceneObject;
    }

    public GameObject GetBasePrefab(bool editorMode, string prefabID)
    {
        var basePrefabData = GetBasePrefabData(prefabID);
        return GetBasePrefab(basePrefabData, editorMode, prefabID);
    }

    public GameObject GetBasePrefab(LoadableBasePrefabData basePrefabData, bool editorMode, string prefabID)
    {
        if (basePrefabData == null)
            return null;

        if (editorMode)
            return basePrefabData.EditorBaseObject;
        else
            return basePrefabData.SceneBaseObject;
    }

    public LoadableBasePrefabData GetBasePrefabData(string prefabID)
    {
        if (BasePrefabs == null || BasePrefabs.Count <= 0)
            return null;

        for (int i = 0; i < BasePrefabs.Count; i++)
        {
            if (BasePrefabs[i].PrefabID == prefabID)
                return BasePrefabs[i];
        }

        return BasePrefabs[0];
    }

    //public string GetName()
    //{
    //    return DisplayName;
    //}

    public string GetAssetWindowName()
    {
        if(string.IsNullOrEmpty(AssetWindowName))
        {
            return AssetID;
        }
        else
        {
            return AssetWindowName;
        }
    }

    public Sprite GetIcon()
    {
        return Icon;
    }

    public NIOSH_EditorLayers.LayerManager.EditorLayer GetLayer()
    {
        return EditorLayer;
    }

    public string GetTooltip()
    {
        return TooltipInfo;
    }
}
