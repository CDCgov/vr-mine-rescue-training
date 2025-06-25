using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using YamlDotNet.Serialization;
using System;

//[System.Serializable]
//public struct ExternalAssetMat
//{
//    public string Diffuse;
//    public string NormalMap;
//    public string MaskMap;
//}

public enum ExternalAssetComponentType
{
    Unknown = 0,
    Audio = 1,
    Light = 2,
}

[System.Serializable]
public struct ExternalAssetComponent
{
    public ExternalAssetComponentType ComponentType;
    public string ComponentName;

    [Description("Name of the object in the hierarchy to attach to")]
    public string ObjectName;    

    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string Filename;
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string PrefabID;

    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public Dictionary<string, string> Properties;
}

//public class ExternalAssetSubmesh
//{
//    public string MeshName;
//    public int LODLevel;
//    public float LODDist;
//}


[System.Serializable]
public struct LODLevelData
{
    public float ScreenRelativeHeight;
}

[System.Serializable]
public class ExternalAssetMetadata
{
    [YamlIgnore]
    public string SourceFolder;
    [YamlIgnore]
    public string SourceFile;

    [Description("# Unique ID that identifies the asset in the scenario file and the VR-MRT")]
    public string AssetID;
    [Description("Asset name displayed to user")]
    public string AssetName;
    [Description("Filename of image to use for asset library icon")]
    public string IconFilename;

    [Description("Filename of gltf model")]
    public string GeometryFilename;
    [Description("Name of the mesh to use for a collider e.g. LOD2")]
    public string MeshColliderName;

    [Description("Screen relative height ratio for each LOD level - number should match number of LOD levels in model file")]
    public List<LODLevelData> LODLevels;

    [Description("Map of material IDs to mesh names, to override that mesh's material. Use ALL: MaterialID to override all")]
    public Dictionary<string, string> MeshMaterialOverrides;

    //public Dictionary<string, ExternalAssetMat> CustomMaterialDefinitions;

    [Description("List of base prefab IDs that this asset can use e.g. STATIC, STATIC_SOUNDABLE, INTERACT, MINE_TILE, MINE_TILE_NO_NAV_MESH")]
    public List<string> BasePrefabs;

    public LoadablePlacementOptions PlacementOptions;
    public LoadablePhysicalProperties PhysicalProperties;

    [Description("Audio material type for collision sounds (e.g. Stone, Wood, Dirt, Cloth, Plastic, Metal, Water)")]
    public string AudioMaterial;

    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
    public bool UseSizeOverride;
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
    public YAMLVec3 SizeOverrideCenter;
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
    public YAMLVec3 SizeOverrideSize;

    [Description("Category(ies) the asset belongs to (i.e., Object, MineTile, Ventilation, ScannedEnvironment, Cable, All)")]
    public LoadableAssetCategories AssetCategories;

    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    //[Description("List of tags to apply to the object")]
    public List<string> Tags;

    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
    public List<ExternalAssetComponent> Components;

    //public float TextureTiling = 0.5f;
    [Description("Should the pivot point be reset to a central location")]
    public bool ResetPivot = false;


    public ExternalAssetMetadata()
    {
       

        MeshMaterialOverrides = new Dictionary<string, string>();
        //CustomMaterialDefinitions = new Dictionary<string, ExternalAssetMat>();

        PlacementOptions = new LoadablePlacementOptions();
        PhysicalProperties = new LoadablePhysicalProperties();
        
        Components = new List<ExternalAssetComponent>();
    }

}
