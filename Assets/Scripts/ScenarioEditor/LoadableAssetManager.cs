using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using NIOSH_EditorLayers;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using Unity.AI.Navigation;
using System.Collections.ObjectModel;

//public class LoadableAssetCoroutineRunner : MonoBehaviour { }

public struct DeferredMeshCollider
{
    public GeometryObjectInfo GeomObjInfo;
    public GameObject TargetObject;
    public Mesh Mesh;
}

public struct MeshColliderBakeJob : IJobParallelFor
{
    private NativeArray<int> meshIds;

    public MeshColliderBakeJob(NativeArray<int> meshIds)
    {
        this.meshIds = meshIds;
    }

    public void Execute(int index)
    {
        Physics.BakeMesh(meshIds[index], false);
    }
}

public struct LoadableLogMessageData
{
    public string Message;
    public LogType LogType;
}

//[CreateAssetMenu(fileName = "LoadableAssetManager", menuName = "VRMine/Managers/LoadableAssetManager", order = 0)]
public class LoadableAssetManager : SceneManagerBase
{
    //public LoadableAssetCollection Loadables;
    //public const string DefaultResourcePath = "Managers/LoadableAssetManager";
    public static bool EnableExternalAssetLoading = true;

    public static LoadableAssetManager GetDefault(GameObject self) 
    {
        //return Resources.Load<LoadableAssetManager>(DefaultResourcePath); 
        return Util.GetDefaultManager<LoadableAssetManager>(self, "LoadableAssetManager", true);
    }

    public ExternalAssetManager ExternalAssetManager;
    public MaterialManager MaterialManager;

    public Sprite DefaultIcon;
    public List<CableHangerData> CableHangerData;

    public ObservableCollection<LoadableLogMessageData> LoadableAssetErrors = new ObservableCollection<LoadableLogMessageData>();


    [System.NonSerialized]
    public MaterialsList RockDustMaterials;
    [System.NonSerialized]
    public bool LoadCompleted = false;

    private Dictionary<string, LoadableAsset> _loadables = null;
    private Dictionary<string, LoadableVariantSet> _variantSets = null;
    private Dictionary<string, LoadableBasePrefabData> _basePrefabData = null;
    private Dictionary<string, SkyboxData> _skyboxData = null;
    private Dictionary<string, CableHangerData> _cableHangers = null;
    private Dictionary<string, MinerProfile> _minerProfiles = null;
    private List<MineTileset> _mineTilesets = null;

    private CableHangerData _defaultCableHanger;
    private MinerProfile _defaultMinerProfile;

    private List<DeferredMeshCollider> _deferredMeshColliders;
    private AudioMaterialList _audioMaterialList;

    private List<ISaveableComponent> _saveableCompCache;
    private int _floorLayer;

    public IEnumerable<LoadableAsset> GetAllLoadableAssets()
    {
        foreach (var loadable in _loadables.Values)
            yield return loadable;
    }

    public IEnumerable<LoadableVariantSet> GetAllLoadableVariantSets()
    {
        foreach (var variant in _variantSets.Values)
            yield return variant;
    }

    public IEnumerable<LoadableAsset> GetLoadableAssetsByLayer(LayerManager.EditorLayer layer)
    {
        foreach (var loadable in _loadables.Values)
        {
            if (loadable.EditorLayer == layer)
                yield return loadable;
        }
    }

    public IEnumerable<LoadableAsset> GetLoadableAssetsByLayer(LayerManager.EditorLayer layer, LoadableAssetCategories categories)
    {
        foreach (var loadable in _loadables.Values)
        {
            if (loadable.EditorLayer == layer && ((loadable.AssetCategories & categories) > 0 || categories == LoadableAssetCategories.All) )
                yield return loadable;
        }
    }

    public IEnumerable<LoadableAsset> GetLoadableAssetsByCategory(LoadableAssetCategories categories)
    {
        foreach (var loadable in _loadables.Values)
        {
            if ((loadable.AssetCategories & categories) > 0)
                yield return loadable;
        }
    }

    public IEnumerable<SkyboxData> GetSkyboxData()
    {
        if (_skyboxData == null)
            yield break;

        foreach (var data in _skyboxData.Values)
            yield return data;
    }

    public SkyboxData FindSkyboxData(string skyboxID)
    {
        if (_skyboxData == null || skyboxID == null)
            return null;

        if (_skyboxData.TryGetValue(skyboxID, out var data))
            return data;

        return null;
    }

    public CableHangerData FindCableHanger(string cableHangerID)
    {
        if (_cableHangers == null || cableHangerID == null)
            return _defaultCableHanger;

        if (_cableHangers.TryGetValue(cableHangerID, out var cableHanger))
            return cableHanger;

        return _defaultCableHanger;
    }
    
    public MinerProfile FindMinerProfile(string minerProfileID)
    {
        if (_minerProfiles == null || minerProfileID == null)
            return _defaultMinerProfile;

        if (_minerProfiles.TryGetValue(minerProfileID, out var minerProfile))
            return minerProfile;

        return _defaultMinerProfile;
    }

    public IEnumerable<MinerProfile> GetAllMinerProfiles()
    {
        if (_minerProfiles == null)
            yield break;

        foreach (var profile in _minerProfiles.Values)
            yield return profile;
    }

    public void Awake()
    {
        _saveableCompCache = new List<ISaveableComponent>();
        _floorLayer = LayerMask.NameToLayer("Floor");
    }

    public void Start()
    {
        if (ExternalAssetManager == null)
            ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);

        Util.DontDestroyOnLoad(gameObject);

        _loadables = new Dictionary<string, LoadableAsset>();
        _variantSets = new Dictionary<string, LoadableVariantSet>();
        _basePrefabData = new Dictionary<string, LoadableBasePrefabData>();
        _mineTilesets = new List<MineTileset>();

        _audioMaterialList = Resources.Load<AudioMaterialList>("Managers/AudioMaterialList");
       
        StartCoroutine(LoadAddressables());
    }

    private IEnumerator LoadAddressables()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        yield return new WaitForEndOfFrame();

        Application.logMessageReceived += OnLogMessageReceived;

        ModalProgressBar.ShowProgressBar("Loading assets...", 0);
        try
        {
            int numAddressablesToLoad = 1;
            int numAddressablesLoaded = 0;

            yield return Util.LoadAddressablesByLabel<LoadableAsset>("LoadableAsset", (loadable) =>
            {
                if (loadable.AssetID == null || loadable.AssetID.Length <= 0)
                    return;

                if (_loadables.ContainsKey(loadable.AssetID))
                {
                    Debug.LogError($"Duplicate AssetID: {loadable.AssetID}");
                    return;
                }

                //Debug.Log($"Loaded asset {loadable.AssetID}");
                _loadables.Add(loadable.AssetID, loadable);

                numAddressablesLoaded++;
                var progress = (float)numAddressablesLoaded / (float)numAddressablesToLoad * 0.90f;
                ModalProgressBar.ShowProgressBar($"Loaded {loadable.AssetWindowName}...", progress);
            },
            (addressableCount) =>
            {
                numAddressablesToLoad = addressableCount;
            });


            Debug.Log($"Addressable LoadableAssets load complete count: {_loadables.Count}, duration: {sw.ElapsedMilliseconds} ms");

            ModalProgressBar.ShowProgressBar($"Loading Built In Materials...", 0.90f);
            yield return MaterialManager.LoadBuiltInMaterials();

            if (EnableExternalAssetLoading)
            {
                ModalProgressBar.ShowProgressBar($"Loading External Materials...", 0.90f);
                yield return MaterialManager.LoadExternalMaterials();
            }


            ModalProgressBar.ShowProgressBar($"Loading variant sets...", 0.91f);
            yield return Util.LoadAddressablesByLabel<LoadableVariantSet>("LoadableVariantSet", (variantSet) =>
            {
                if (variantSet.LoadableVariants == null || variantSet.LoadableVariants.Count <= 0)
                    return;

                foreach (var loadable in variantSet.LoadableVariants)
                {
                    if (loadable.AssetID == null || loadable.AssetID.Length <= 0)
                    {
                        Debug.LogError("Invalid/missing asset id in variant set");
                        return;
                    }
                    _variantSets.Add(loadable.AssetID, variantSet);
                }                
            });

            Debug.Log($"Addressable LoadableVariantSet load complete count: {_variantSets.Count}, duration: {sw.ElapsedMilliseconds} ms");

            ModalProgressBar.ShowProgressBar($"Loading base prefabs...", 0.92f);
            yield return Util.LoadAddressablesByLabel<LoadableBasePrefabData>("BasePrefabData", (basePrefabData) =>
            {
                if (basePrefabData == null || basePrefabData.PrefabID == null || basePrefabData.PrefabID.Length <= 0)
                    return;

                if (_basePrefabData.ContainsKey(basePrefabData.PrefabID))
                {
                    Debug.LogError($"Duplicated base prefab data, ID: {basePrefabData.PrefabID}");
                    return;
                }

                _basePrefabData.Add(basePrefabData.PrefabID, basePrefabData);
            });

            Debug.Log($"Addressable BasePrefabData load complete count: {_basePrefabData.Count}, duration: {sw.ElapsedMilliseconds} ms");

            ModalProgressBar.ShowProgressBar($"Loading mine tilesets...", 0.93f);
            yield return Util.LoadAddressablesByLabel<MineTileset>("MineTileset", (tileset) =>
            {
                _mineTilesets.Add(tileset);
            });

            Debug.Log($"Addressable MineTileset load complete count: {_mineTilesets.Count}, duration: {sw.ElapsedMilliseconds} ms");

            var defaultIconHandle = Addressables.LoadAssetAsync<Sprite>("Icon_CustomAsset");
            yield return defaultIconHandle;
            DefaultIcon = defaultIconHandle.Result;

            var rockDustMaterialsHandle = Addressables.LoadAssetAsync<MaterialsList>("RockDustMaterials");
            yield return rockDustMaterialsHandle;
            RockDustMaterials = rockDustMaterialsHandle.Result;

            if (ExternalAssetManager == null)
                ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);

            

            ModalProgressBar.ShowProgressBar("Loading skybox data...", 0.94f);
            yield return LoadSkyboxData();

            ModalProgressBar.ShowProgressBar("Loading cable hangers...", 0.94f);
            yield return LoadCableHangers();

            ModalProgressBar.ShowProgressBar("Loading miner profiles...", 0.945f);
            yield return LoadMinerProfiles();

            if (EnableExternalAssetLoading)
            {
                ModalProgressBar.ShowProgressBar("Loading external assets...", 0.95f);
                yield return LoadExternalAssets();

                ModalProgressBar.ShowProgressBar("Baking mesh colliders...", 0.96f);
                yield return BakeMeshColliders();
            }

            sw.Stop();
            Debug.Log($"Addressable LoadableAssets load complete count: {_loadables.Count}, duration: {sw.ElapsedMilliseconds} ms");
        }
        finally
        {
            ModalProgressBar.HideProgressBar();
            LoadCompleted = true;
            Application.logMessageReceived -= OnLogMessageReceived;
        }
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
            return;

        //LoadableAssetErrors.Add($"{type.ToString()}: {condition}");
        LoadableAssetErrors.Add(new LoadableLogMessageData
        {
            Message = condition,
            LogType = type,
        });
    }

    private IEnumerator LoadSkyboxData()
    {
        if (_skyboxData == null)
            _skyboxData = new Dictionary<string, SkyboxData>();

        _skyboxData.Clear();

        yield return Util.LoadAddressablesByLabel<SkyboxData>("SkyboxData", (data) =>
        {
            _skyboxData.Add(data.SkyboxID, data);
        });
    }

    private IEnumerator LoadCableHangers()
    {
        if (_cableHangers == null)
            _cableHangers = new Dictionary<string, CableHangerData>();

        _cableHangers.Clear();

        yield return Util.LoadAddressablesByLabel<CableHangerData>("CableHangerData", (data) =>
        {
            _cableHangers.Add(data.CableHangerID, data);
        });

        _defaultCableHanger = FindCableHanger("U_HANGER");

        if (_defaultCableHanger == null && _cableHangers.Count > 0)
            _defaultCableHanger = _cableHangers.Values.First();

        //create indexed array of cable hanger data
        CableHangerData = new List<CableHangerData>(_cableHangers.Count);
        
        foreach (var data in _cableHangers.Values)
        {
            CableHangerData.Add(data);
        }

        CableHangerData.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
    }

    private IEnumerator LoadMinerProfiles()
    {
        if (_minerProfiles == null)
            _minerProfiles = new Dictionary<string, MinerProfile>();

        _minerProfiles.Clear();

        yield return Util.LoadAddressablesByLabel<MinerProfile>("MinerProfile", (data) =>
        {
            _minerProfiles.Add(data.MinerProfileID, data);
        });

        _defaultMinerProfile = FindMinerProfile("COAL_MR");

        if (_defaultMinerProfile == null && _minerProfiles.Count > 0)
            _defaultMinerProfile = _minerProfiles.Values.First();
    }

    private IEnumerator LoadExternalAssets()
    {
        //Debug.Log($"TEXTURE THREADING: {Texture2D.allowThreadedTextureCreation}");
        if (ExternalAssetManager == null)
            yield break;

        if (!ExternalAssetManager.IsMetadataLoaded)
            ExternalAssetManager.LoadMetadataFiles();

        var task = BuildExternalAssetsParallel();

        while (!task.IsCompleted)
            yield return new WaitForEndOfFrame();
    }

    private IEnumerator BakeMeshColliders()
    {
        if (_deferredMeshColliders == null || _deferredMeshColliders.Count <= 0)
            yield break;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        NativeArray<int> meshIds = new NativeArray<int>(_deferredMeshColliders.Count, Allocator.Persistent);

        for (int i = 0; i < _deferredMeshColliders.Count; ++i)
        {
            meshIds[i] = _deferredMeshColliders[i].Mesh.GetInstanceID();
        }

        var job = new MeshColliderBakeJob(meshIds);
        var handle = job.Schedule(meshIds.Length, 3);

        Debug.Log($"ExternalAssets: Preparing to bake mesh colliders took {sw.Elapsed.TotalSeconds:F2}");

        while (!handle.IsCompleted)
            yield return new WaitForEndOfFrame();

        handle.Complete();
        meshIds.Dispose();

        for (int i = 0; i < _deferredMeshColliders.Count; ++i)
        {
            var geomObjInfo = _deferredMeshColliders[i].GeomObjInfo;
            var meshCollider = _deferredMeshColliders[i].TargetObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = _deferredMeshColliders[i].Mesh;

            if (geomObjInfo != null)
            {
                geomObjInfo.AddMeshCollider(meshCollider);
            }
        }

        sw.Stop();

        Debug.Log($"ExternalAssets: Baking mesh colliders took {sw.Elapsed.TotalSeconds:F2}");
    }

    public void AddDeferredMeshCollider(GeometryObjectInfo geomObjInfo, GameObject targetObject, Mesh mesh)
    {
        if (_deferredMeshColliders == null)
            _deferredMeshColliders = new List<DeferredMeshCollider>();

        var deferred = new DeferredMeshCollider()
        {
            GeomObjInfo = geomObjInfo,
            TargetObject = targetObject,
            Mesh = mesh,
        };

        _deferredMeshColliders.Add(deferred);

    }

    private async Task BuildExternalAssets()
    {
        if (ExternalAssetManager == null)
            ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();

        sw.Start();

        foreach (var assetData in ExternalAssetManager.GetExternalAssetMetadata())
        {
            try
            {
                sw2.Start();
                //Debug.Log($"Loading external asset {assetData.SourceFile}");

                var asset = await ExternalAssetManager.BuildLoadableAsset(assetData, this, _audioMaterialList);
                if (asset == null || asset.AssetID == null)
                {
                    Debug.LogError($"Failed to load asset for {assetData.SourceFile}");
                    continue;
                }

                if (_loadables.ContainsKey(asset.AssetID))
                {
                    Debug.LogError($"Error: External asset has duplicate ID {asset.AssetID}");
                    continue;
                }

                _loadables.Add(asset.AssetID, asset);

                sw2.Stop();
                Debug.Log($"Loading external asset {assetData.SourceFile} took {sw2.ElapsedMilliseconds}ms");
                sw2.Reset();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed loading external asset: {ex.Message} {ex.StackTrace}");
            }
        }

        sw.Stop();

        Debug.Log($"External Assets loading took {sw.Elapsed.TotalSeconds:F2}s");
    }

    private async Task BuildExternalAssetsParallel()
    {
        if (ExternalAssetManager == null)
            ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        sw.Start();

        var loadTasks = new List<Task<LoadableAsset>>();

        foreach (var assetData in ExternalAssetManager.GetExternalAssetMetadata())
        {
            try
            {
                Debug.Log($"Loading external asset {assetData.SourceFile}");

                var task = ExternalAssetManager.BuildLoadableAsset(assetData, this, _audioMaterialList);
                loadTasks.Add(task);

                //if (asset == null || asset.AssetID == null)
                //{
                //    Debug.LogError($"Failed to load asset for {assetData.SourceFile}");
                //    continue;
                //}

                //if (_loadables.ContainsKey(asset.AssetID))
                //{
                //    Debug.LogError($"Error: External asset has duplicate ID {asset.AssetID}");
                //    continue;
                //}

                //_loadables.Add(asset.AssetID, asset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed loading external asset: {ex.Message} {ex.StackTrace}");
            }
        }

        try
        {
            await Task.WhenAll(loadTasks);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed loading external asset: {ex.Message} {ex.StackTrace}");
        }

        int taskFailureCount = 0;

        foreach (var task in loadTasks)
        {
            if (!task.IsCompletedSuccessfully)
            {
                taskFailureCount++;
                continue;
            }

            var asset = task.Result;

            if (asset == null || asset.AssetID == null)
            {
                //Debug.LogError($"Failed to load asset for {task}");
                taskFailureCount++;
                continue;
            }

            if (_loadables.ContainsKey(asset.AssetID))
            {
                Debug.LogError($"Error: External asset has duplicate ID {asset.AssetID}");
                taskFailureCount++;
                continue;
            }

            if (taskFailureCount > 0)
            {
                Debug.LogError($"{taskFailureCount} load tasks failed");
            }

            _loadables.Add(asset.AssetID, asset);
        }

        sw.Stop();

        Debug.Log($"External Assets loading took {sw.Elapsed.TotalSeconds:F2}s");
    }

    public LoadableBasePrefabData GetBasePrefabData(string prefabID)
    {
        if (_basePrefabData == null)
            return null;

        if (_basePrefabData.TryGetValue(prefabID, out var basePrefabData))
            return basePrefabData;

        return null;
    }

    public GameObject InstantiateEditorAsset(string assetID)
    {
        return InstantiateEditorAsset(assetID, Vector3.zero, Quaternion.identity, null);
    }

    public GameObject InstantiateEditorAsset(string assetID, Vector3 pos, Quaternion rot, Transform parent)
    {
        var loadable = FindAsset(assetID);
        if (loadable == null)
            return null;

        return InstantiateEditorAsset(loadable, null, pos, rot, parent);
    }

    public GameObject InstantiateEditorAsset(LoadableAsset asset, string basePrefabID, Vector3 pos, Quaternion rot, Transform parent)
    {
        return InstantiateAsset(asset, basePrefabID, pos, rot, parent, true);
    }

    public GameObject InstantiateSceneAsset(string assetID, Vector3 pos, Quaternion rot, Transform parent)
    {
        var loadable = FindAsset(assetID);
        if (loadable == null)
            return null;

        return InstantiateSceneAsset(loadable, null, pos, rot, parent);
    }

    public GameObject InstantiateSceneAsset(LoadableAsset asset, string basePrefabID, Vector3 pos, Quaternion rot, Transform parent)
    {
        return InstantiateAsset(asset, basePrefabID, pos, rot, parent, false);
    }

    public GameObject InstantiateAsset(LoadableAsset asset, string basePrefabID, 
        Vector3 pos, Quaternion rot, Transform parent, bool editorAsset)
    {
        var prefab = asset.GetPrefab(editorAsset);
        if (prefab != null)
        {
            return InstantiatePrefab(prefab, asset, pos, rot, parent, asset.AssetID);
        }

        //build object from the selected base prefab + geometry
        return InstantiateCompositePrefab(asset, basePrefabID, pos, rot, parent, editorAsset);
    }

    private GameObject InstantiateInactive(GameObject prefab)
    {
        var wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        var obj = Instantiate<GameObject>(prefab);
        prefab.SetActive(wasActive);

        return obj;
    }

    private GameObject InstantiateInactive(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        var wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        var obj = Instantiate<GameObject>(prefab, pos, rot, parent);
        prefab.SetActive(wasActive);

        return obj;
    }

    private GameObject InstantiatePrefab(GameObject prefab, LoadableAsset asset, 
        Vector3 pos, Quaternion rot, Transform parent, string assetID)
    {
        var obj = InstantiateInactive(prefab, pos, rot, parent);

        return InitializeObject(obj, asset, null);
    }

    private GameObject InstantiateCompositePrefab(LoadableAsset asset, string basePrefabID,
        Vector3 pos, Quaternion rot, Transform parent, bool editorAsset)
    {
        if (asset == null)
            return null;

        var basePrefabData = asset.GetBasePrefabData(basePrefabID);
        var basePrefab = asset.GetBasePrefab(basePrefabData, editorAsset, basePrefabID);

        if (basePrefabData == null || basePrefab == null || asset.GeometryObject == null)
            return null;

        Debug.Log($"LoadableAssetManager: Instantiating composite {asset.AssetID} base {basePrefab.name} geom {asset.GeometryObject.name}");
        var obj = InstantiateInactive(basePrefab, pos, rot, parent);

        obj.TryGetComponent<SoundingType>(out var basePrefabSoundingType);

        var geomObj = InstantiateInactive(asset.GeometryObject);
        geomObj.transform.SetParent(obj.transform);
        geomObj.transform.localPosition = Vector3.zero;
        geomObj.transform.localRotation = Quaternion.identity;
        geomObj.transform.localScale = Vector3.one;
        geomObj.SetActive(true);

        
        if (geomObj.TryGetComponent<PermissibleMaterialSwitch>(out var permissibleMaterialSwitch)) 
        {
            permissibleMaterialSwitch.PermissibleSwap(!basePrefabData.IgnitionSource);
        }

        if (!asset.AllowNavMeshModifiers)
        {
            if (obj.TryGetComponent<NavMeshModifier>(out var navMeshModifier))
            {
                Destroy(navMeshModifier);
            }
        }    

        //geomObj.TryGetComponent<GeometryObjectInfo>(out var geomObjInfo);
        GeometryObjectInfo geomObjInfo = geomObj.GetComponentInChildren<GeometryObjectInfo>();

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            if (!rb.isKinematic)
            {
                //replace any non-convex mesh colliders with box colliders on non-kinematic 
                var meshColliders = obj.GetComponentsInChildren<MeshCollider>(true);
                foreach (var collider in meshColliders)
                {
                    if (collider.convex)
                        continue;

                    collider.gameObject.AddComponent<BoxCollider>();
                    Destroy(collider);

                }

            }
        }

        SetupGeometryObjectColliders(basePrefabData, geomObjInfo, basePrefabSoundingType);

        if (editorAsset && obj.TryGetComponent<ObjectInfo>(out var objInfo))
        {
            //check for placement anchor in geometry object direct child objects
            foreach (Transform child in geomObj.transform)
            {
                if (child.name == "PlacementAnchor")
                {
                    objInfo.PlacementAnchor = child;
                    break;
                }
            }
        }

        return InitializeObject(obj, asset, basePrefabID);
    }

    private void SetupGeometryObjectColliders(LoadableBasePrefabData basePrefabData, GeometryObjectInfo geomObjInfo, 
        SoundingType basePrefabSoundingType)
    {
        if (geomObjInfo == null || basePrefabData == null)
            return;

        if (geomObjInfo.MeshColliders != null)
        {
            foreach (var collider in geomObjInfo.MeshColliders)
            {
                SetupGeometryObjectColliders(collider, basePrefabData, geomObjInfo, basePrefabSoundingType);
            }
        }

        if (geomObjInfo.OtherColliders != null)
        {
            foreach (var collider in geomObjInfo.OtherColliders)
            {
                SetupGeometryObjectColliders(collider, basePrefabData, geomObjInfo, basePrefabSoundingType);
            }
        }
    }

    private void SetupGeometryObjectColliders(Collider collider, LoadableBasePrefabData basePrefabData, 
        GeometryObjectInfo geomObjInfo, SoundingType basePrefabSoundingType)
    {
        if (basePrefabSoundingType != null)
        {
            AddSoundingType(collider, basePrefabSoundingType);
        }

        if (basePrefabData.AllowDNI)
        {
            AddAllowDNI(collider);
        }
    }

    private void AddSoundingType(Collider collider, SoundingType baseSoundingType)
    {
        var soundingType = collider.gameObject.AddComponent<SoundingType>();
        soundingType.SoundMaterial = baseSoundingType.SoundMaterial;
        soundingType.MaterialSounds = baseSoundingType.MaterialSounds;
        soundingType.Priority = baseSoundingType.Priority;
    }

    private void AddAllowDNI(Collider collider)
    {
        if (collider.gameObject.layer == _floorLayer)
            return; //floor layer already receives DNI

        collider.gameObject.AddComponent<AllowDNI>();
    }

    private void SetPermissible()
    {

    }

    private GameObject InitializeObject(GameObject obj, LoadableAsset asset, string basePrefabID)
    {
        var objID = Guid.NewGuid();

        if (obj.TryGetComponent<ObjectInfo>(out var objInfo))
        {
            objInfo.AssetID = asset.AssetID;
            objInfo.BasePrefabID = basePrefabID;
            objInfo.InstanceID = objID;
            objInfo.AssetWindowName = asset.AssetWindowName;

            objInfo.SetDisplayName(asset.AssetWindowName);
            objInfo.InstanceName = asset.AssetWindowName;

            objInfo.editorLayer = asset.PlacementOptions.EditorLayer;
        }

        if (obj.TryGetComponent<MineLayerTile>(out var mineTile))
        {
            mineTile.ChangeModeToEdit(false);
            mineTile.ScaleToSettings();
        }

        if (!obj.TryGetComponent<PlacablePrefab>(out var placeable))
        {
            placeable = obj.AddComponent<PlacablePrefab>();
        }

        if (obj.TryGetComponent<PointOfInterest>(out var poi))
        {
            poi.ID = objID.ToString();
        }


        obj.name = asset.AssetWindowName;

        return obj;
    }

    public GameObject InstantiateAsset(SavedAsset asset, Transform parent, bool editorAsset)
    {
        LoadableAsset loadable = null;
        if (asset.AssetID == null || asset.AssetID.Length <= 0)
        {
            //old asset - use old method to get asset by display name / asset name
            //loadable = Loadables.GetLoadableAsset(asset.AssetName);
            return null;
        }
        else
        {
            loadable = FindAsset(asset.AssetID);
        }

        if (loadable == null)
        {
            Debug.LogError($"Couldn't find asset {asset.AssetID} ({asset.AssetName})");
            return null;
        }

        GameObject obj = null;
        if (editorAsset)
            obj = InstantiateEditorAsset(loadable, asset.BasePrefabID, asset.Position, asset.Rotation, parent);
        else
            obj = InstantiateSceneAsset(loadable, asset.BasePrefabID, asset.Position, asset.Rotation, parent);

        obj.transform.localScale = asset.Scale;

        return obj;
    }

    public GameObject ReplaceObjectBasePrefab(ObjectInfo objInfo, LoadableBasePrefabData newBasePrefab)
    {
        if (objInfo == null || objInfo.transform.parent == null)
            return null;

        if (!objInfo.gameObject.TryGetComponent<PlacablePrefab>(out var placeable))
            return null;

        var savedAsset = SaveObject(placeable);
        if (savedAsset == null)
            return null;

        savedAsset.BasePrefabID = newBasePrefab.PrefabID;

        var parent = objInfo.transform.parent;

        Destroy(objInfo.gameObject);
        var obj = InstantiateSavedAsset(savedAsset, parent, true);


        return obj;
    }

    public GameObject ReplaceObjectVariant(ObjectInfo objInfo, LoadableAsset newAsset)
    {
        if (objInfo == null || objInfo.transform.parent == null)
            return null;

        if (!objInfo.gameObject.TryGetComponent<PlacablePrefab>(out var placeable))
            return null;

        var savedAsset = SaveObject(placeable);
        if (savedAsset == null)
            return null;

        
        savedAsset.AssetID = newAsset.AssetID;
        savedAsset.AssetName = newAsset.AssetWindowName;
               

        var parent = objInfo.transform.parent;

        Destroy(objInfo.gameObject);
        var obj = InstantiateSavedAsset(savedAsset, parent, true);


        return obj;
    }

    public GameObject DuplicateObject(ObjectInfo objInfo, LoadableAsset newAsset)
    {
        if (objInfo == null || objInfo.transform.parent == null)
            return null;

        if (!objInfo.gameObject.TryGetComponent<PlacablePrefab>(out var placeable))
            return null;

        var savedAsset = SaveObject(placeable);
        if (savedAsset == null)
            return null;

        Vector3 updatePos = savedAsset.Position;
        //updatePos.x += 1;
        //updatePos.z += 1;

        savedAsset.AssetID = newAsset.AssetID;
        savedAsset.AssetName = newAsset.AssetWindowName;
        savedAsset.Position = updatePos;
        savedAsset.GUIDString = Guid.NewGuid().ToString();


        var parent = objInfo.transform.parent;

        
        var obj = InstantiateSavedAsset(savedAsset, parent, true);

        if (obj.TryGetComponent<ComponentInfo_MineSegment>(out var mineSegment))
        {
            mineSegment.ResetTeamstopGUID();
        }


        return obj;
    }

    public LoadableAsset FindAsset(string assetID)
    {
        if (_loadables == null || assetID == null || assetID.Length <= 0)
            return null;

        if (_loadables.TryGetValue(assetID, out var asset))
            return asset;
        else
            return null;
    }

    public LoadableVariantSet GetVariantSet(string assetID)
    {
        if (_variantSets == null)
            return null;

        if (_variantSets.TryGetValue(assetID, out var variantSet))
            return variantSet;
        else
            return null;
    }

    public SavedAsset SaveObject(PlacablePrefab obj)
    {
        List<SavedComponent> saveableComponents = new List<SavedComponent>();
        SaveObjectComponents(obj.transform, saveableComponents);

        ObjectInfo info = obj.gameObject.GetComponent<ObjectInfo>();
        if (info.InstanceID.Equals(Guid.Empty)) 
        { 
            info.InstanceID = Guid.NewGuid(); 
        }

        bool isMineTile = obj.PlacementLayer == NIOSH_EditorLayers.LayerManager.EditorLayer.Mine;
        string distinctName =  info.DisplayName;
        if (info.componentInfo_Name != null && !string.IsNullOrEmpty(info.componentInfo_Name.instanceDisplayName))
        {
            distinctName = info.componentInfo_Name.instanceDisplayName;
        }

        Vector3 scale = obj.transform.localScale;
        if (info.componentInfo_Transform.scaleExposureLevel == Inspector.ExposureLevel.Fixed)
        {
            scale = Vector3.one;
        }

        SavedAsset savedAsset = new SavedAsset
        {
            AssetID = info.AssetID,
            BasePrefabID = info.BasePrefabID,
            Position = obj.transform.position,
            Scale = scale,
            Rotation = obj.transform.rotation,
            DistinctName = distinctName,
            AssetName = obj.GetDisplayName(),
            SavedComponents = saveableComponents,
            //IsNetworked = info.isNetworked,
            GUIDString = info.InstanceID.ToString(),
            IsMineTile = isMineTile
        };

        return savedAsset;
    }

    /// <summary>
    /// Saves the component data of any ISaveableComponent monobehaviours attached
    /// to the transform provided by accepting whatever string data the ISaveableComponent implemented
    /// class gives it. This data is added to the saved object's list of saved components. 
    /// </summary>
    /// <param name="savedObject"></param>
    /// <param name="saveableComponents"></param>
    void SaveObjectComponents(Transform savedObject, List<SavedComponent> saveableComponents)
    {
        //returns all monobehaviors of children and grandchildren
        //List<MonoBehaviour> componentList = new List<MonoBehaviour>(savedObject.gameObject.GetComponents<MonoBehaviour>());
        //componentList.AddRange(savedObject.gameObject.GetComponentsInChildren<MonoBehaviour>());

        _saveableCompCache.Clear();
        savedObject.gameObject.GetComponentsInChildren<ISaveableComponent>(_saveableCompCache);

        foreach (ISaveableComponent saveable in _saveableCompCache)
        {
            //ISaveableComponent saveable = component as ISaveableComponent;
            //SavedComponent saveComponent = new SavedComponent(componentType, saveable.SaveInfo());
            SavedComponent saveComponent = new SavedComponent(saveable.SaveName(), saveable.SaveInfo());

            if (!saveableComponents.Any(x => x.GetComponentName() == saveable.SaveName())) 
            { 
                saveableComponents.Add(saveComponent); 
            }


            // Check here to make sure we aren't saving duplicate component data, as well as confirming we are actually working with a class that implements ISaveableComponent
            //string componentType = component.GetType().ToString();
            //if (component is ISaveableComponent && !saveableComponents.Any(x => x.GetComponentName() == componentType))
            //if (component is ISaveableComponent)
            //{
            //    ISaveableComponent saveable = component as ISaveableComponent;
            //    //SavedComponent saveComponent = new SavedComponent(componentType, saveable.SaveInfo());
            //    SavedComponent saveComponent = new SavedComponent(saveable.SaveName(), saveable.SaveInfo());

            //    if (!saveableComponents.Any(x => x.GetComponentName() == saveable.SaveName())) { saveableComponents.Add(saveComponent); }

            //}
        }

        _saveableCompCache.Clear();
    }

    /// <summary>
    /// Loads a saved asset's saved components into any matching found components on the provided gameobject. 
    /// Ignores any components found on the gameobject that do not implement ISaveableComponent.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="asset"></param>
    void LoadObjectComponents(GameObject obj, SavedAsset asset)
    {
        List<SavedComponent> savedComponents = asset.GetAllSavedComponents();

        //List<MonoBehaviour> componentList = new List<MonoBehaviour>(obj.GetComponents<MonoBehaviour>());
        //componentList.AddRange(obj.GetComponentsInChildren<MonoBehaviour>());

        //var componentList = obj.GetComponentsInChildren<ISaveableComponent>();

        _saveableCompCache.Clear();
        obj.GetComponentsInChildren<ISaveableComponent>(_saveableCompCache);

        for (int i = 0; i < _saveableCompCache.Count; i++)
        {
            //if (componentList[i] is ISaveableComponent)
            //{
            //ISaveableComponent component = (ISaveableComponent)componentList[i];
            //SavedComponent componentData = savedComponents.FirstOrDefault(x => x.GetComponentName() == component.GetType().ToString());

            ISaveableComponent component = _saveableCompCache[i];
            SavedComponent componentData = savedComponents.FirstOrDefault(x => x.GetComponentName() == component.SaveName());
            if (componentData == null)
                continue; //no saved data present to load

            component.LoadInfo(componentData);
        }

        _saveableCompCache.Clear();
    }

    /// <summary>
    /// Initial instantiation of a new gameobject from a prefab object and the provided SavedAsset data object. 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="asset"></param>
    public GameObject InstantiateSavedAsset(SavedAsset asset, Transform parent, bool loadEditorPrefabs)
    {
        if (asset == null)
            return null;

        var obj = InstantiateAsset(asset, parent, loadEditorPrefabs);
        if (obj == null)
        {
            Debug.LogError($"Error instantiating saved asset {asset.AssetID}");
            return null;
        }

        //obj.transform.localScale = asset.Scale;

        //instantiate prefab in inactivate state so that name can be changed before OnEnable
        //note: this is specifically needed at the moment for the teamstop POI script
        //var prefabActive = obj.activeSelf;
        //obj.SetActive(false);
        //GameObject newObj = Instantiate(obj, asset.Position, asset.Rotation, assetHolder.transform);
        //newObj.transform.localScale = asset.Scale;

        //if (asset.IsMineTile)
        //{
        //    Debug.Log($"What is the tile scale? {obj.transform.localScale}");
        //}
        obj.name = asset.DistinctName;
        ObjectInfo info = obj.GetComponent<ObjectInfo>();
        info.componentInfo_Name.instanceDisplayName = asset.DistinctName;
        info.InstanceID = new Guid(asset.GUIDString);

        if (obj.TryGetComponent<PointOfInterest>(out var poi))
        {
            poi.ID = asset.GUIDString;
        }


        //restore prefab active state
        //obj.SetActive(prefabActive);

        //TODO: temporary fix for teamstops - don't activate until after component is loaded
        //teamstops need to have their name set prior to activation
        if (obj.name != "Team Stop" && info.AssetID != "TEAMSTOP")
        {
            //activate instantiated prefab
            obj.SetActive(true);
        }        

        LoadObjectComponents(obj, asset);

        // Force placed object to display as sometimes they instantiate disabled by default.
        obj.SetActive(true);

        PlacablePrefab placeable = obj.GetComponent<PlacablePrefab>();

        if (placeable == null)
        {
            placeable = obj.AddComponent<PlacablePrefab>();
        }
        placeable.SetPlaced();

        if (asset.IsMineTile && obj.TryGetComponent<MineLayerTile>(out var mineLayerTile))
        {
            //MineLayerTile mineLayerTile = obj.GetComponent<MineLayerTile>();
            //newObj.GetComponent<MineLayerTile>().SetReturnPoint(placeable.transform.position);
            mineLayerTile.SetReturnPoint(placeable.transform.position);
           
        }

        if (obj.TryGetComponent<NetworkedObject>(out var netObj))
        {
            netObj.uniqueID = info.InstanceID;
        }

        return obj;
    }

    public MineTileset GetMineTileset(MineSettings.TileSet tileset)
    {
        if (_mineTilesets == null || _mineTilesets.Count <= 0)
            return null;

        foreach (var t in _mineTilesets)
        {
            if (t.GetTilesetType() == tileset)
                return t;
        }

        return null;
    }
}
