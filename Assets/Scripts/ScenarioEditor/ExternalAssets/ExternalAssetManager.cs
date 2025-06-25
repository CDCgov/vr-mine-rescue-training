using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ExternalAssetManager : SceneManagerBase
{
    public static ExternalAssetManager GetDefault(GameObject self)
    {
        return Util.GetDefaultManager<ExternalAssetManager>(self, "ExternalAssetManager", true);
    }

    //private static string _externalAssetsFolder = null;
    //public static string ExternalAssetsFolder
    //{
    //    get
    //    {
    //        if (_externalAssetsFolder != null)
    //            return _externalAssetsFolder;

    //        var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    //        var folder = Path.Combine(userprofile, "VRMine", "ExternalAssets");
    //        Directory.CreateDirectory(folder);

    //        _externalAssetsFolder = folder;
    //        return folder;
    //    }
    //}

    public SystemManager SystemManager;
    public MaterialManager MaterialManager;

    public LODLevelDefaults LODLevelDefaults;


    private Dictionary<string, ExternalAssetMetadata> _metadata;
    private Dictionary<int, LODValues> _defaultLODValues;
    //private Dictionary<string, ExternalAssetMetadata> _geometryFileMap;

    //private Dictionary<string, Material> _customMats;

    public bool IsMetadataLoaded
    {
        get
        {
            if (_metadata == null)
                return false;
            return true;
        }
    }

    public List<LODLevelData> GetDefaultLODValues(int numLODs)
    {
        if (_defaultLODValues == null)
            return GenerateLODValues(numLODs);

        if (!_defaultLODValues.TryGetValue(numLODs, out var lods))
            return GenerateLODValues(numLODs);

        return lods.DefaultValues;

    }

    public static List<LODLevelData> GenerateLODValues(int numLODs)
    {
        List<LODLevelData> lods = new List<LODLevelData>(numLODs);
        
        for (int i = 0; i < numLODs; i++)
        {
            float val = (float)(numLODs - i - 1);
            val = val / (float)numLODs;

            var lod = new LODLevelData
            {
                ScreenRelativeHeight = val,
            };

            lods.Add(lod);
        }

        return lods;
    }

    public ExternalAssetMetadata GetMetadata(string assetID)
    {
        if (_metadata == null)
            return null;

        if (_metadata.TryGetValue(assetID, out var assetData))
            return assetData;

        return null;
    }

    public IEnumerable<ExternalAssetMetadata> GetExternalAssetMetadata()
    {
        if (_metadata == null || _metadata.Count <= 0)
            yield break;

        foreach (var assetData in _metadata.Values)
        {
            yield return assetData;
        }
    }

    //public static async Task<GameObject> LoadGeometryObject(ExternalAssetMetadata assetData)
    //{
    //    await Task.Delay(150);
    //    var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    obj.name = assetData.AssetID;
    //    DontDestroyOnLoad(obj);
    //    obj.SetActive(false);

    //    return obj;
    //}

    public async Task<LoadableAsset> BuildLoadableAsset(ExternalAssetMetadata assetData, LoadableAssetManager loadableAssetManager, AudioMaterialList amList)
    {
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);

        LoadableAsset asset = ScriptableObject.CreateInstance<LoadableAsset>();
        asset.AssetID = assetData.AssetID;
        asset.AssetWindowName = assetData.AssetName;
        asset.GeometryObject = await ExternalAssetBuilder.BuildGeometryObject(assetData, loadableAssetManager);
        //asset.BasePrefabs = new LoadableBasePrefabData[1];
        //asset.BasePrefabs[0] = loadableAssetManager.GetBasePrefabData("MINE_TILE");
        //asset.EditorLayer = NIOSH_EditorLayers.LayerManager.EditorLayer.Mine;
        asset.EditorLayer = assetData.PlacementOptions.EditorLayer;
        asset.PlacementOptions = assetData.PlacementOptions;
        asset.PhysicalProperties = assetData.PhysicalProperties;
        asset.AudioProperties = await ExternalAssetBuilder.LoadAudio(assetData, amList);
        //asset.Icon = loadableAssetManager.DefaultIcon;
        asset.Icon = await ExternalAssetBuilder.LoadAssetIcon(assetData, loadableAssetManager);
        asset.AssetCategories = assetData.AssetCategories;

        asset.BasePrefabs = new List<LoadableBasePrefabData>();

        if (assetData.BasePrefabs != null)
        {
            foreach (var basePrefabID in assetData.BasePrefabs)
            {
                var basePrefab = loadableAssetManager.GetBasePrefabData(basePrefabID);
                if (basePrefab != null)
                    asset.BasePrefabs.Add(basePrefab);
            }
        }

        if (asset.BasePrefabs.Count <= 0)
        {
            asset.BasePrefabs.Add(loadableAssetManager.GetBasePrefabData("MINE_TILE"));
        }

        if (assetData.Tags != null)
        {
            asset.Tags = new HashSet<string>();
            foreach (var tag in assetData.Tags)
            {
                asset.Tags.Add(tag);
            }
        }

        asset.Source = LoadableAssetSource.External;
        asset.ExternalAssetMetadata = assetData;

        if (asset.GeometryObject == null)
            return null;


        //temporary fix - add colliders to all mesh objects
       
        var meshRend = asset.GeometryObject.GetComponentsInChildren<MeshRenderer>();

        foreach (var rend in meshRend)
        {
            if (rend.sharedMaterials == null || rend.sharedMaterials.Length <= 0)
                continue;

            //rend.sharedMaterial.DisableKeyword("DECALS_OFF");
            foreach (var mat in rend.sharedMaterials)
            {
                if (mat == null)
                    continue;

                mat.SetFloat("_SupportDecals", 0.0f);
                mat.SetFloat("_SupportDecals", 1.0f);

                HDMaterial.ValidateMaterial(mat);
            }
        }

        //if (meshFilters != null)
        //{            
        //    foreach (var filter in meshFilters)
        //    {
        //        //temp - remove all LODs
        //        if (filter.gameObject.name.Contains("LOD1") || filter.gameObject.name.Contains("LOD2"))
        //        {
        //            Destroy(filter.gameObject);
        //            continue;
        //        }

        //        var collider = filter.gameObject.AddComponent<MeshCollider>();                
        //    }
        //}

        //var collider = asset.GeometryObject.GetComponentInChildren<Collider>();
        //if (collider == null)
        //{
        //    Debug.LogWarning($"External asset {asset.AssetID} did not have a collider, adding a collider to all meshes");

        //    var meshFilters = asset.GeometryObject.GetComponentsInChildren<MeshFilter>();
        //    foreach (var filter in meshFilters)
        //    {
        //        filter.gameObject.AddComponent<MeshCollider>();
        //    }

        //}

        //put all objects containing meshes on the floor layer for Mine layer objects
        int floorLayer = LayerMask.NameToLayer("Floor");
        if (asset.EditorLayer == NIOSH_EditorLayers.LayerManager.EditorLayer.Mine)
        {
            var objs = asset.GeometryObject.GetComponentsInChildren<MeshFilter>();
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    obj.gameObject.layer = floorLayer;
                }
            }
        }

        if (assetData.ResetPivot)
        {
            var b = new Bounds();

            b = meshRend[0].bounds;

            for (int i = 1; i < meshRend.Length; i++)
            {
                b.Encapsulate(meshRend[i].bounds);

                //var mat = meshRend[i].sharedMaterial;
                //var baseTex = mat.GetTexture("baseColorTexture");
                //var normalTex = mat.GetTexture("normalTexture");
                //if (baseTex != null & normalTex != null)
                //{
                //    Debug.Log($"{asset.AssetID}: baseTex: {baseTex.width}x{baseTex.height} mipMaps: {baseTex.mipmapCount} normalTex: {normalTex.width}x{normalTex.height} mipMaps:{normalTex.mipmapCount}");
                //}
            }

            Vector3 offset = b.center * -1.0f /*+ new Vector3(0, b.extents.y, 0)*/;

            var pivotReset = new GameObject(assetData.AssetID);
            pivotReset.transform.position = Vector3.zero;
            pivotReset.transform.rotation = Quaternion.identity;
            asset.GeometryObject.transform.SetParent(pivotReset.transform);
            asset.GeometryObject.transform.position = offset;
            asset.GeometryObject.transform.rotation = Quaternion.identity;

            pivotReset.SetActive(false);
            asset.GeometryObject.SetActive(true);

            DontDestroyOnLoad(pivotReset);
            asset.GeometryObject = pivotReset;
        }

        //Dictionary<string, Material> customMats = new Dictionary<string, Material>();

        //if (assetData.CustomMaterialDefinitions != null && assetData.CustomMaterialDefinitions.Count > 0)
        //{
        //    foreach (var matData in assetData.CustomMaterialDefinitions)
        //    {
        //        if (matData.Value.Diffuse == null)
        //            continue;

        //        var diffuseFile = Path.Combine(assetData.SourceFolder, matData.Value.Diffuse);
        //        if (!File.Exists(diffuseFile))
        //            continue;

        //        var mat = Instantiate<Material>(Resources.Load<Material>("ExternalAssetMat"));
        //        var diffuseData = File.ReadAllBytes(diffuseFile);
        //        var diffuse = new Texture2D(2, 2);
        //        diffuse.LoadImage(diffuseData);

        //        mat.SetTexture("_BaseColorMap", diffuse);

        //        if (matData.Value.NormalMap != null)
        //        {
        //            var normalFile = Path.Combine(assetData.SourceFolder, matData.Value.NormalMap);
        //            if (File.Exists(normalFile))
        //            {
        //                var normalData = File.ReadAllBytes(normalFile);
        //                var normalMap = new Texture2D(4, 4, TextureFormat.DXT1, true, true);
        //                normalMap.LoadImage(normalData);
                        
        //                mat.SetTexture("_NormalMap", normalMap);
        //            }
        //        }

        //        customMats.Add(matData.Key, mat);
        //    }
        //}

      

        if (assetData.MeshMaterialOverrides != null && meshRend != null)
        {
            foreach (var ov in assetData.MeshMaterialOverrides)
            {
                Material mat;

                //if (ov.Value == "TriPlanarTest")
                //{
                //    mat = Resources.Load<Material>("TriplanarTestMat");
                //}
                //if (!customMats.TryGetValue(ov.Value, out mat))
                //{
                //    try
                //    {
                //        mat = await Addressables.LoadAssetAsync<Material>(ov.Value.ToUpper()).Task;
                //        mat = Instantiate<Material>(mat);
                //    }
                //    catch (Exception ex)
                //    {
                //        Debug.LogWarning($"Couldn't load material named { ov.Value } : {ex.Message} ");
                //        continue;
                //    }

                //    if (assetData.TextureTiling > 0)
                //    {
                //        mat.SetFloat("_Tile", assetData.TextureTiling);
                //    }

                //    if (mat == null)
                //        continue;
                //}

                if (!MaterialManager.TryFindMaterial(ov.Value, out mat))
                {
                    Debug.LogError($"Couldn't find material override {ov.Value} for {assetData.AssetID}");
                    continue;
                }

                foreach (var rend in meshRend)
                {
                    if (ov.Key == "ALL" || rend.name.Contains(ov.Key))
                    {
                        rend.sharedMaterial = mat;
                    }
                }
            }
        }

        return asset;
    }

    public void LoadMetadataFiles() 
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);
       

        Dictionary<string, ExternalAssetMetadata> metadata = new Dictionary<string, ExternalAssetMetadata>();
        Dictionary<string, ExternalAssetMetadata> geometryFileMap = new Dictionary<string, ExternalAssetMetadata>();

        foreach (var file in GetMetadataFiles(SystemManager.SystemConfig.ExternalAssetsFolder))
        {
            var assetData = LoadMetadataFile(file);

            if (assetData != null && assetData.GeometryFilename != null && assetData.GeometryFilename.Length > 0)
            {
                metadata.Add(assetData.AssetID, assetData);

                if (!geometryFileMap.ContainsKey(assetData.GeometryFilename))
                    geometryFileMap.Add(assetData.GeometryFilename, assetData);
            }
        }

        foreach (var geometryFile in GetGeometryFiles(SystemManager.SystemConfig.ExternalAssetsFolder))
        {
            if (geometryFile == null || geometryFile.Length <= 0)
                continue;

            var geometryFileNameNoPath = Path.GetFileName(geometryFile);

            if (geometryFileMap.ContainsKey(geometryFileNameNoPath))
                continue;

            var assetData = BuildDefaultMetadata(geometryFile);
            if (assetData != null)
                metadata.Add(assetData.AssetID, assetData);
        }

        //_geometryFileMap = geometryFileMap;
        _metadata = metadata;        
    }

    public void SaveMetadataFiles()
    {
        if (_metadata == null || _metadata.Count <= 0)
            return;

        foreach (var metadata in _metadata.Values)
        {
            var filename = Path.Combine(metadata.SourceFolder, metadata.SourceFile);

            if (!metadata.UseSizeOverride)
            {
                metadata.SizeOverrideCenter = default;
                metadata.SizeOverrideSize = default;
            }

            Debug.Log($"Saving metadata to {filename}");
            YAMLMetadata.Save(filename, metadata);
        }
    }

    private ExternalAssetMetadata LoadMetadataFile(string filename)
    {
        try
        {
            Debug.Log(filename);
            var assetData = YAMLMetadata.Load<ExternalAssetMetadata>(filename);
            if (assetData == null)
                return null;

            if (assetData.AssetID == null || assetData.AssetID.Length <= 0)
            {
                Debug.LogError($"External asset missing asset ID: {filename}");
            }

            assetData.SourceFolder = Path.GetDirectoryName(filename);
            assetData.SourceFile = Path.GetFileName(filename);
            
            return assetData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading metadata {filename}: {ex.Message}");
        }

        return null;
    }

    private ExternalAssetMetadata BuildDefaultMetadata(string geometryFileName)
    {                
        var baseName = Path.GetFileNameWithoutExtension(geometryFileName);
        var sourceFolder = Path.GetDirectoryName(geometryFileName);
        var sourceFile = baseName + ".yaml";
        var geometryFileNameNoPath = Path.GetFileName(geometryFileName);
        var filename = Path.Combine(sourceFolder, sourceFile);

        if (File.Exists(filename))
            return null;

        var basePrefabs = new List<string>();
        basePrefabs.Add("MINE_TILE");

        var assetData = new ExternalAssetMetadata
        {
            SourceFolder = sourceFolder,
            SourceFile = sourceFile,
            AssetID = baseName,
            AssetName = baseName,
            GeometryFilename = geometryFileNameNoPath,
            LODLevels = GetDefaultLODValues(4),
            MeshColliderName = "LOD2",
            BasePrefabs = basePrefabs,
            ResetPivot = false,
            PlacementOptions = new LoadablePlacementOptions
            {
                EditorLayer = NIOSH_EditorLayers.LayerManager.EditorLayer.Mine,
                ShowInAssetWindow = true,
            },
            IconFilename = null,
            PhysicalProperties = new LoadablePhysicalProperties
            {
                Mass = 999,
                AirflowResistance = 0,
                Density = 1,
                Drag = 0,
                AngularDrag = 0,
            },
            AudioMaterial = "Stone",
            UseSizeOverride = false,
            //SizeOverrideCenter = YAMLVec3.FromVector3(Vector3.one), 
            //SizeOverrideSize = YAMLVec3.FromVector3(Vector3.one),
            SizeOverrideCenter = default,
            SizeOverrideSize = default,
            AssetCategories = LoadableAssetCategories.ScannedEnvironment,
        };

        
        YAMLMetadata.Save(filename, assetData);

        return assetData;
    }

    
    private IEnumerable<string> GetMetadataFiles(string directory = null)
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (directory == null)
            directory = SystemManager.SystemConfig.ExternalAssetsFolder;

        var files = Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories);
        foreach(var file in files)
        {
            yield return file;
        }
    }

    private IEnumerable<string> GetGeometryFiles(string directory = null)
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (directory == null)
            directory = SystemManager.SystemConfig.ExternalAssetsFolder;

        var files = Directory.GetFiles(directory, "*.glb", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            yield return file;
        }
    }


    private void Awake()
    {
        if (LODLevelDefaults == null)
        {
            LODLevelDefaults = Resources.Load<LODLevelDefaults>("LODLevelDefaults");
        }

        if (LODLevelDefaults != null)
        {
            _defaultLODValues = new Dictionary<int, LODValues>();

            foreach (var lodValues in LODLevelDefaults.DefaultValues)
            {
                if (lodValues.DefaultValues == null || lodValues.DefaultValues.Count <= 1)
                    continue;

                if (_defaultLODValues.ContainsKey(lodValues.DefaultValues.Count))
                {
                    Debug.LogError($"More than one default LOD value set for count {lodValues.DefaultValues.Count}");
                    continue;
                }

                _defaultLODValues.Add(lodValues.DefaultValues.Count, lodValues);
            }
        }
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.Backslash)) 
        {
            SaveMetadataFiles();
        }
    }

#endif

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);


        Util.DontDestroyOnLoad(gameObject);
    }

}
