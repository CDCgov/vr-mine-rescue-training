using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;


// TODO split this out into a controller and factory methods so UI can later control the factory
public class ScenarioSaveLoad : MonoBehaviour
{
    public static ScenarioSaveLoad Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = FindObjectOfType<ScenarioSaveLoad>();
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
    private static ScenarioSaveLoad _instance;

    public static SavedScenario ActiveScenario
    {
        get
        {
            if (ScenarioSaveLoad.Instance == null)
                return null;

            return Instance.activeScenario;
        }
    }

    public static GlobalMineParameters Settings
    {
        get
        {
            if (_instance != null && _instance._settings != null)
                return _instance._settings;

            if (_defaultSettings == null)
                _defaultSettings = new GlobalMineParameters();

            return _defaultSettings;
        }
        set
        {
            if (_instance == null)
                return;

            _instance._settings = value;
        }
    }

    //public static MinerProfile CurrentMinerProfile
    //{
    //    get
    //    {
    //        if (Settings == null)
    //            return null;

            
    //        return Settings.MinerProfile;
    //    }
    //}

    private static GlobalMineParameters _defaultSettings = null;

    public static bool IsScenarioEditor
    {
        get
        {
            if (Instance == null)
                return false;

            //TODO: Replace this with a scene specific data class
            return SceneManager.GetSceneByName("BAH_ScenarioEditor").isLoaded;
        }
    }

    public static void ClearActiveScenario()
    {
        if (Instance == null)
            return;

        Instance.activeScenario = null;
    }

    public LoadableAssetManager LoadableAssetManager;
    public SystemManager SystemManager;

    public VolumeProfile DefaultVolumeProfile;

    public GameObject assetHolder;
    public GameObject geometryHolder;

    public MineLayerTileManager MineLayerTileManager;
    public ScenarioLoadUIManager loadManager;
    public Action onLoadComplete;
    public Action onLoadStart;

    public event Action MineSettingsChanged;

    public Bounds MineBounds { get; set; }

    public bool IsSceneLightingEnabled
    {
        get { return _sceneLightingEnabled; }
    }

    public event Action SceneLightingChanged;
    public event Action ScenarioChanged;    


    public void RaiseScenarioChanged()
    {
        try
        {
            ScenarioChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in ScenarioChanged event handler: {ex.Message} {ex.StackTrace}");
        }
    }

    private List<Light> _scenarioEditorLights = new List<Light>();
    private Volume _skyVolume;
    private GameObject _sceneLighting = null;
    private VolumeProfile _sceneVolumeProfile = null;
    private bool _sceneLightingEnabled = false;
    private GlobalMineParameters _settings;

    //private string SCENARIO_FILE_PATH;
    //private string SCENARIO_DESTINATION_KEY = "Save_Destination";
    SavedScenario activeScenario;
    //static string workingScenarioName;
    //AssetLoader loader;
    bool isBusy;
    int loadedAssetCount;

    Placer _scenePlacer;

    private int assetCount;

    private string _workingScenarioName = null;
    private bool _loadInProgress = false;

    public string WorkingScenarioName
    {
        get
        {
            if (_workingScenarioName == null)
                return "";

            return _workingScenarioName;
        }
        private set
        {
            if (value != _workingScenarioName)
            {
                Debug.Log($"WorkingScenarioName changed to {value}");
                _workingScenarioName = value;
                WorkingScenarioNameChanged?.Invoke(value);
            }
        }
    }

    public bool IsLoadInProgress
    {
        get { return _loadInProgress; }
    }

    //public bool IsScenarioEditor
    //{
    //    get
    //    {
    //        //TODO: Replace this with a scene specific data class
    //        return SceneManager.GetSceneByName("BAH_ScenarioEditor").isLoaded;
    //    }
    //}

    public event Action<string> WorkingScenarioNameChanged;

    //[SerializeField] private MaterialsList rockDustMaterials;  // TODO could use its own class to manage setting global parameters
    //private HierarchyContainer hierarchy;
    //[SerializeField]Button initializeSceneButton;
    // Read objects in the scene and create save text for them
    // Read save text and instantiate objects from them

    // use data container objects for placable assets
    // look at Alex's mine generation code to see how we want to store mine tiles
    // likely either store global generation parameters and regenerate THEN add modifications
    // OR
    // we store positions of tiles and tile types and add them that way
    // will also need to have like a set of parameters per script required in the scene for configuration purposes
    // will need to figure out how to handle that properly since on load each component needs to be handled specially unless they have a proper constructor
    // and I can feed them parameters params whatever


    /*
     * Categories for asset storage in scene
     *      Curtains
     *      Wires
     *      Interactables
     *      Non-Interactables
     *      Scene-Controllers?
     *      Ventilation?
    */


    // Need to refactor asset bundle loader to take in commands and load a prefab from a bundle like LoadFromBundle(string bundleName, string assetName) {}

    // Create a container object for each functional type like placables, curtains, wires, global parameters, anything that has a unique 
    // instantiation process. When scene is saved each category game object's children are scanned for their needed parameters which are then
    // serialized into json. When the scene is loaded, each container object is then fed to a scene factory object which instantiates the needed
    // object in the scene and sets it up with all needed paramters.

    public void ClearWorkingScenarioName()
    {
        WorkingScenarioName = null;
    }

    public void SetSkyboxID(string skyboxID)
    {
        if (_settings == null)
            return;

        _settings.SkyboxID = skyboxID;
        SetupLighting(skyboxID);
        EnableSceneLighting(_sceneLightingEnabled);
    }

    //public async Task<bool> IsScenarioCloseAllowed()
    //{
    //    var result = await ModalYesNoCancel.ShowDialog("Do you want to save the scenario?", "Save", "Don't Save", DialogResult.No);
    //    if (result == DialogResult.No)
    //        return true;
    //    else
    //        return false;
    //}

    //public void IsScenarioCloseAllowed(Action<bool> callback)
    //{
    //    ModalYesNoCancel.ShowDialog("Do you want to save the scenario?", "Save", "Don't Save", DialogResult.Yes, callback);        
    //}

    public bool IsScenarioDirty
    {
        get { return true; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        if (_settings == null)
            _settings = new GlobalMineParameters();

        if (MineLayerTileManager == null)
            MineLayerTileManager = MineLayerTileManager.GetDefault();

        SceneManager.sceneLoaded += OnSceneLoaded;


        //if (PlayerPrefs.HasKey(SCENARIO_DESTINATION_KEY))
        //{
        //    SCENARIO_FILE_PATH = PlayerPrefs.GetString(SCENARIO_DESTINATION_KEY);
        //}
        //else
        //{
        //    PlayerPrefs.SetString(SCENARIO_DESTINATION_KEY, Application.persistentDataPath + "/Scenarios/");
        //}

        //if (string.IsNullOrEmpty(SCENARIO_FILE_PATH))
        //{
        //    SCENARIO_FILE_PATH = Application.persistentDataPath + "/Scenarios/";
        //}
        _scenePlacer = FindObjectOfType<Placer>();
        //hierarchy = GameObject.Find("HierarchyWindow").GetComponent<HierarchyContainer>();
        //if(hierarchy)onLoadComplete += hierarchy.Reinitialize;

        //if (rockDustMaterials == null)
        //{
        //    rockDustMaterials = Resources.FindObjectsOfTypeAll<MaterialsList>()[0]; // FIXME placeholder will cause problems if we create other materialslist SOs
        //}
        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        var lights = FindObjectsOfType<Light>();

        _scenarioEditorLights.Clear();
        
        foreach (var light in lights)
        {
            if (light.name == "CapLamp")
                continue;

            _scenarioEditorLights.Add(light);
        }

        _skyVolume = null;
        var skyObj = GameObject.Find("Sky and Fog Volume");
        if (skyObj != null)
        {
            _skyVolume = skyObj.GetComponent<Volume>();
        }

        _sceneLightingEnabled = false;

        

        if (IsScenarioEditor)
        {
            if (_settings.SkyboxID == null || _settings.SkyboxID.Length <= 0)
            {
                SetSkyboxID("UNDERGROUND");
            }
            else
            {
                SetupLighting(_settings.SkyboxID);
                EnableSceneLighting(false);
            }
        }


        //Debug.Log("Scene Loaded");
        ScenarioChanged?.Invoke();
    }

    private void Start()
    {
        if (MineLayerTileManager == null)
            MineLayerTileManager = MineLayerTileManager.GetDefault();
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (_scenePlacer == null)
            _scenePlacer = Placer.GetDefault();
    }

    private void OnEnable()
    {
        //loader = FindObjectOfType<AssetLoader>();

        if (MineLayerTileManager == null)
            MineLayerTileManager = MineLayerTileManager.GetDefault();

        //if (loader == null) { loader = gameObject.AddComponent<AssetLoader>(); }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public string GetScenarioFilePath()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        
        return SystemManager.SystemConfig.ScenariosFolder;                
    }

    public async Task SaveCurrentScenario(string scenarioName, bool setWorkingScenarioName = true)
    {
        if (isBusy) { return; }
        isBusy = true;

        ModalProgressBar.ShowProgressBar("Saving Scenario....", 0.1f);

        if (assetHolder == null) { assetHolder = GameObject.Find("Assets"); }

        //string filePath = SCENARIO_FILE_PATH + "/" + scenarioName;
        string filePath = Path.Combine(GetScenarioFilePath(), scenarioName + ".json");

        activeScenario = new SavedScenario();
        activeScenario.SetScenarioName(scenarioName);
        activeScenario.GlobalSettings = _settings;

        //MineBuilderUI mineUI = FindObjectOfType<MineBuilderUI>(); // TODO replace with a better method of getting rock dust levels
        //float rockDustLvl = 0;
        //if (mineUI != null)
        //{
        //    rockDustLvl = mineUI.GetRockDustLevel();
        //}
        ////activeScenario.InitializeGlobalParameters(rockDustLvl);
        ////activeScenario.GlobalSettings.MineSettings.rockDustLvl = (int)rockDustLvl;
        //SetRockDustLevel((int)rockDustLvl);

        List<PlacablePrefab> placedObjects = new List<PlacablePrefab>(FindObjectsOfType<PlacablePrefab>());
        foreach (PlacablePrefab obj in placedObjects)
        {
            if (obj == null || obj.gameObject == null || obj.GetIsIgnoreSave())
                continue;

            try
            {
                var savedAsset = LoadableAssetManager.SaveObject(obj);
                activeScenario.AddAssetToList(savedAsset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScenarioSaveLoad: Error saving object {obj.name} - {ex.Message} : {ex.StackTrace}");
            }
        }

        //Add bolt graph?
        //SavedAsset boltAsset = new SavedAsset()

        //Debug.Log("SAVING SCeNARIO AT: " + filePath);
        //JSONSerializer.Serialize<SavedScenario>(activeScenario, filePath);
        ModalProgressBar.ShowProgressBar("Saving Scenario....", 0.25f);

        var mapData = await VectorMineMap.BuildBaseMapData(Color.black, 0.3f, _settings.MineMapGridSize);
        var mapDataString = VectorMineMap.SaveMapToString(mapData);

        Debug.Log($"ScenarioSaveLoad: Saved map to string, length {mapDataString.Length}");

        activeScenario.MapData = mapDataString;

        activeScenario.SaveScenario(filePath);

        if (loadManager == null) { FindObjectOfType<ScenarioLoadUIManager>(); }
        if (loadManager != null) { loadManager.GetCustomScenarioData(); }

        ModalProgressBar.ShowProgressBar("Saving Scenario....", 0.75f);

        //await Task.Delay(1000);

        //var mapData = await VectorMineMap.BuildBaseMapData(Color.black, 0.3f);
        //var mapDataFilename = Path.Combine(GetScenarioFilePath(), scenarioName + ".map");

        //VectorMineMap.SaveMapToFile(mapData, mapDataFilename);

        isBusy = false;
        if (setWorkingScenarioName)
            WorkingScenarioName = scenarioName;

        ModalProgressBar.HideProgressBar();
    }

    //public void LoadScenarioFromFile(string scenarioName, bool scenarioEditorMode, bool setWorkingScenarioName = true)
    //{
    //    Debug.Log("MADE IT TO THE SAVE LOAD SCRIPT");
    //    LoadScenarioFromFile(scenarioName, setWorkingScenarioName, true);
    //}

    public void ChangeSaveDestination(string destination)
    {
        //string fixedText = destination.Replace(@"\", "/");
        //if (JSONFileManagement.CheckForValidDirectory(fixedText))
        //{
        //    PlayerPrefs.SetString(SCENARIO_DESTINATION_KEY, fixedText);
        //    SCENARIO_FILE_PATH = fixedText;
        //}
        //else
        //{
        //    Debug.LogWarning("Directory at: " + fixedText + " DOES NOT EXIST.");
        //}
    }

    public void LoadScenarioFromFile(string scenarioName, bool loadEditorPrefabs, bool setWorkingScenarioName)
    {
        try
        {
            if (isBusy) { return; }
            loadedAssetCount = 0;
            assetCount = 0;
            onLoadStart?.Invoke();
            if (assetHolder == null)
            {
                assetHolder = GameObject.Find("Assets");
                if (assetHolder == null)
                {
                    assetHolder = new GameObject("Assets");
                }

            }
            isBusy = true;
            _loadInProgress = true;

            //string filePath = SCENARIO_FILE_PATH + "/" + scenarioName;
            if (Path.GetExtension(scenarioName) != ".json")
                scenarioName += ".json";

            string filePath = Path.Combine(GetScenarioFilePath(), scenarioName);
            //string json = JSONFileManagement.LoadJSONAsString(filePath);
            //SavedScenario savedScenario = JSONSerializer.Deserialize<SavedScenario>(json);

            SavedScenario savedScenario = SavedScenario.LoadScenario(filePath);

            activeScenario = savedScenario;
            if (activeScenario.GlobalSettings == null)
                activeScenario.GlobalSettings = new GlobalMineParameters();

            activeScenario.GlobalSettings.MapDataFile = Path.GetFileNameWithoutExtension(scenarioName) + ".map";

            if (activeScenario.GlobalSettings.MineScale == Vector3.zero)
            {
                //to support old scenario files, override the missing settings with whatever we have loaded
                activeScenario.GlobalSettings = _settings;
            }

            _settings = activeScenario.GlobalSettings;
            if (_settings.MineSettings == null)
                _settings.MineSettings = new MineSettings();

            var mineNetwork = GameObject.FindObjectOfType<MineNetwork>();
            if (mineNetwork != null)
            {
                Debug.Log($"ScenarioSaveLoad: Setting mine scale to {_settings.MineScale}");
                mineNetwork.SceneTileScale = _settings.MineScale;
            }


            if (setWorkingScenarioName)
                WorkingScenarioName = activeScenario.GetScenarioName();

            ClearScene();


            List<SavedAsset> savedAssetList = new List<SavedAsset>(savedScenario.GetSavedAssets());
            assetCount = savedAssetList.Count;
            Debug.Log($"ScenarioSaveLoad: Asset count: {assetCount}");

            List<SavedAsset> mineTiles = new List<SavedAsset>();
            List<SavedAsset> networkedAssets = new List<SavedAsset>();
            List<SavedAsset> unnetworkedAssets = new List<SavedAsset>();

            SetRockDustLevel(_settings.RockDustLevel);
            //ApplyRockDustLevel(Settings.MineSettings.rockDustLvl);

            Debug.Log($"ScenarioSaveLoad Global Params Loaded");

            foreach (SavedAsset asset in savedAssetList)
            {
                if (asset.IsMineTile)
                    mineTiles.Add(asset);
                else if (asset.IsNetworked)
                    networkedAssets.Add(asset);
                else
                    unnetworkedAssets.Add(asset);
            }

            Debug.Log($"ScenarioSaveLoad: MineTiles:{mineTiles.Count} Networked:{networkedAssets.Count} Other:{unnetworkedAssets.Count}");

            InstantiateSavedAssetList(mineTiles, loadEditorPrefabs);

            if (MineLayerTileManager == null)
                MineLayerTileManager = MineLayerTileManager.GetDefault();

            if (MineLayerTileManager != null)
                MineLayerTileManager.RebuildTileConnections();
            else
                Debug.LogWarning($"No Mine layer tile manager found");

            InstantiateSavedAssetList(unnetworkedAssets, loadEditorPrefabs);
            InstantiateSavedAssetList(networkedAssets, loadEditorPrefabs);

            SetupLighting(_settings.SkyboxID);

            if (!IsScenarioEditor)
                EnableSceneLighting(true);

            ForceObjectsEnabled();

            isBusy = false;
            _loadInProgress = false;
            loadedAssetCount = assetCount;

            //Check if scene config asset loaded, if not it's an old scenario file that didn't have one before
            //ComponentInfo_MineSceneConfig config = FindObjectOfType<ComponentInfo_MineSceneConfig>();
            //if (config == null)
            //{
            //    LoadableAsset asset = LoadableAssetManager.FindAsset("SCENECONFIG");
            //    GameObject go = LoadableAssetManager.InstantiateEditorAsset(asset.AssetID, Vector3.zero, Quaternion.identity, assetHolder.transform);
            //    config = go.GetComponent<ComponentInfo_MineSceneConfig>();
            //    config.DefaultLoad();
            //    go.SetActive(true);
            //}


            try
            {
                onLoadComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScenarioSaveLoad: Exception invoking LoadComplete {ex.Message} {ex.StackTrace}");
            }

        }
        finally
        {
            isBusy = false;
            _loadInProgress = false;
        }
        //assetCount = savedAssetList.Count - networkedAssets.Count;
        //StartCoroutine(WaitForLoading(savedAssetList.Count - networkedAssets.Count, networkedAssets));
    }

    private void InstantiateSavedAssetList(List<SavedAsset> assetList, bool loadEditorPrefabs)
    {
        foreach (SavedAsset asset in assetList)
        {
            //GameObject prefab = loader.GetPlaceableAsset(asset.AssetName);
            //if (prefab != null)
            //{
            try
            {
                var obj = LoadableAssetManager.InstantiateSavedAsset(asset, assetHolder.transform, loadEditorPrefabs);
                if (asset.IsMineTile)
                {
                    bool editorOpen = false;
                    for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                    {
                        string name = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
                        if (name == "BAH_ScenarioEditor")
                        {
                            editorOpen = true;

                        }
                    }
                    if (!editorOpen)
                    {
                        MineLayerTile mineLayerTile = obj.GetComponent<MineLayerTile>();
                        //Debug.Log($"Local scale: {obj.transform.localScale}");
                        //mineLayerTile.SpawnBolts();
                        StartCoroutine(DelayedBoltSpawn(mineLayerTile));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ScenarioSaveLoad: Error instantiating saved asset {asset.AssetID} {ex.Message} {ex.StackTrace}");
            }
            //}

            loadedAssetCount++;
        }

    }

    private void SetupLighting(string skyboxID)
    {
        if (LoadableAssetManager == null)
            return;

        var skyboxData = LoadableAssetManager.FindSkyboxData(skyboxID);
        if (skyboxData == null)
        {
            Debug.LogWarning($"ScenarioSaveLoad: Couldn't find skybox ID: {skyboxID}");
            return;
        }

        _sceneVolumeProfile = skyboxData.VolumeProfile;        

        if (_sceneLighting != null)
        {
            Destroy(_sceneLighting);
            _sceneLighting = null;
        }    

        if (skyboxData.LightPrefab != null)
        {
            _sceneLighting = Instantiate<GameObject>(skyboxData.LightPrefab);
        }
        
    }

    public void EnableSceneLighting(bool enable)
    {
        if (_skyVolume != null)
        {
            if (_sceneVolumeProfile != null && enable)
                _skyVolume.profile = _sceneVolumeProfile;
            else if (DefaultVolumeProfile != null && !enable)
                _skyVolume.profile = DefaultVolumeProfile;
        }

        if (_sceneLighting != null)
            _sceneLighting.SetActive(enable);

        if (_scenarioEditorLights != null)
        {
            foreach (var light in _scenarioEditorLights)
            {
                if (!light.TryGetComponent<DMLight>(out var dmlight))
                    light.enabled = !enable;
            }
        }

        _sceneLightingEnabled = enable;

        SceneLightingChanged?.Invoke();

    }


    public float CheckLoadStatus()
    {
        if (assetCount <= 0)
            return 0;

        return (float)loadedAssetCount / (float)assetCount;
    }

    //IEnumerator WaitForLoading(int numberToWaitFor, List<SavedAsset> networkedAssets)
    //{

    //    while (loadedAssetCount < numberToWaitFor)
    //    {
    //        yield return null;
    //    }
    //    isBusy = false;
    //    //ForceObjectsEnabled();

    //    yield return new WaitForSeconds(0.25f);
    //    LoadNetworkedAssets(networkedAssets);
    //    yield return new WaitForSeconds(0.25f);

    //    ForceObjectsEnabled();
    //    //var button = GameObject.Find("Continue_Button");
    //    //if (button != null && button.TryGetComponent(out Button buttonUI)){ buttonUI.onClick.Invoke(); }
    //}

    //private void ApplyRockDustLevel(int rockDustLevel)
    //{
    //    MineBuilderUI mineUI = FindObjectOfType<MineBuilderUI>(); // TODO replace with a better method of getting rock dust levels
    //    if (mineUI != null)
    //    {
    //        mineUI.SetRockDust(rockDustLevel);
    //    }

    //    //LoadableAssetManager.RockDustMaterials.SetRockdust((float)rockDustLevel / 100.0f);

    //    //foreach (Material m in LoadableAssetManager.RockDustMaterials.GetAllMaterials())
    //    //{
    //    //    m.SetFloat("_Rockdust", rockDustLevel / 100.0f);
    //    //}
    //}

    public void RaiseMineSettingsChanged()
    {
        MineSettingsChanged?.Invoke();
    }


    public void SetRockDustLevel(float rockDustLevel)
    {
        _settings.RockDustLevel = rockDustLevel;
        _settings.MineSettings.rockDustLvl = (int)(rockDustLevel * 100.0f);
        MineSettingsChanged?.Invoke();
    }

    public void EnableCornerCurtain(bool enable)
    {
        _settings.MineSettings.EnableCornerCurtains = enable;
        MineSettingsChanged.Invoke();
    }

    public void SetBoltSpacing(float roofBoltSpacing)
    {
        _settings.MineSettings.BoltSpacing = roofBoltSpacing;
        MineSettingsChanged?.Invoke();
    }

    public void SetBoltOffset(float roofBoltOffset)
    {
        _settings.MineSettings.BoltRibOffset = roofBoltOffset;
        MineSettingsChanged?.Invoke();
    }

    public void SetBoltSpacingAndOffset(float roofBoltSpacing, float roofBoltOffset)
    {
        _settings.MineSettings.BoltSpacing = roofBoltSpacing;
        _settings.MineSettings.BoltRibOffset = roofBoltOffset;
        MineSettingsChanged?.Invoke();
    }

    

    //void LoadNetworkedAssets(List<SavedAsset> assets)
    //{
    //    foreach (SavedAsset asset in assets)
    //    {
    //        //GameObject prefab = loader.GetPlaceableAsset(asset.AssetName);
    //        //if (prefab == null) { loadedAssetCount++; continue; }
    //        InstantiateSavedAsset(asset);
    //    }
    //    onLoadComplete?.Invoke();
    //}

    void ForceObjectsEnabled()
    {


        foreach (PlacablePrefab obj in Resources.FindObjectsOfTypeAll(typeof(PlacablePrefab)) as PlacablePrefab[])
        {
            obj.gameObject.SetActive(true);
            //Debug.Log("ForcedObjectEnabled");

        }
    }


    IEnumerator DelayedBoltSpawn(MineLayerTile mineLayerTile)
    {
        yield return 0;
        //mineLayerTile.SpawnBolts();
    }

   

    void ClearScene()
    {
        Debug.Log("CLEARING SCENE");
        if (_scenePlacer != null)
        {
            _scenePlacer.DeselectObject();
        }

        List<PlacablePrefab> placedObjects = new List<PlacablePrefab>(FindObjectsOfType<PlacablePrefab>());
        int placedObjectsCount = placedObjects.Count;
        for (int i = 0; i < placedObjectsCount; i++)
        {
            var obj = placedObjects[i].gameObject;
            if (obj == null)
                continue;

            obj.transform.SetParent(null, false);

            Destroy(obj);
        }
    }

    private void Update()
    {
        if (IsScenarioEditor)
        {
            if (_scenePlacer == null)
                _scenePlacer = Placer.GetDefault();

            if (_scenePlacer != null && _scenePlacer.IsInputLocked)
                return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (IsScenarioEditor)
                EnableSceneLighting(!_sceneLightingEnabled);
        }
    }

    //public string GetWorkingScenarioName()
    //{
    //    if (!string.IsNullOrEmpty(workingScenarioName))
    //    {
    //        return workingScenarioName;
    //    }
    //    else
    //    {
    //        return String.Empty;
    //    }

    //}
}
