using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using YamlDotNet.Serialization;
using System.IO;

public class SceneLoadManager : MonoBehaviour
{

    public const string GameObjectName = "SceneLoadManager";
    public const string WaitingRoomScene = "VRWaitingRoom";

    public static SceneLoadManager GetDefault(GameObject self)
    {
        var manager = self.GetDefaultManager<SceneLoadManager>("SceneLoadManager", true);        
        manager.tag = "Manager";

        //var obj = GameObject.Find(GameObjectName);
        //if (obj == null)
        //{
        //    obj = new GameObject(GameObjectName);
        //    obj.tag = "Manager";
        //}
        //var manager = obj.GetComponent<SceneLoadManager>();
        //if (manager == null)
        //    manager = obj.AddComponent<SceneLoadManager>();

        return manager;
    }

    public SystemManager SystemManager;
    public NetworkManager NetworkManager;
    public SceneFadeManager SceneFadeManager;
    public LoadableAssetManager LoadableAssetManager;

    [UnityEngine.Serialization.FormerlySerializedAs("returnToEditorButton")]
    public Button ReturnToEditorButton;

    public bool LoadWaitingRoom = false;

    public bool IsLoadInProgress
    {
        get { return _loadInProgress; }
    }

    public event Action SceneUnloading;
    public event Action SceneChanged;
    public event Action SceneLoadFinalized;
    public event Action EnteredSimulationScene;

    public string ScenarioName { get; private set; }

    public bool InSimulationScene
    {
        get
        {
            return _sceneLoaded;
        }
    }

    public bool InWaitingRoom
    {
        get
        {
            return _inWaitingRoom;
        }
    }

    public bool LoadInProgress
    {
        get
        {
            return _loadInProgress;
        }
    }


    private AsyncOperation _sceneLoad = null;
    private AsyncOperationHandle? _addrSceneLoad = null;
    private AsyncOperation _sceneUnload = null;
    private string _sceneLoadName;
    private string _customSceneFileName;

    private bool _sceneLoaded = false;
    private Scene _activeScene;
    //private bool _sceneReadyToActivate = false;
    //private bool _allowActivation = false;


    private string _loadedScene = null;
    private string _expectedScene = null;
    private bool _loadInProgress = false;
    private bool _requestedWorldState = false;
    private bool _inWaitingRoom = false;
    private bool _customScenarioLoadStarted = false;
    private Dictionary<string, ScenarioData> _scenarioData;
    private bool _initialized = false;
    private NavMeshRuntimeGenerator _navMeshGenerator = null;

    // Start is called before the first frame update
    async void Start()
    {
        _scenarioData = new Dictionary<string, ScenarioData>();

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (SceneFadeManager == null)
            SceneFadeManager = SceneFadeManager.GetDefault(gameObject);
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        _initialized = true;

        if (ReturnToEditorButton != null)
        {
            //ReturnToEditorButton.gameObject.SetActive(false);
            //ReturnToEditorButton.interactable = false;
            //ReturnToEditorButton.image.color = Color.clear;
        }
        //NetworkManager.SceneLoadCommand.AddListener(OnLoadSceneCommand);
        NetworkManager.SceneLoadCommand += OnLoadSceneCommand;
        NetworkManager.SimStateChanged += OnSimStateChanged;
        NetworkManager.DisconnectedFromServer += OnDisconnectedFromServer;

        SceneManager.sceneLoaded += OnSceneLoaded;

        //Debug.Log($"SceneLoadManager::Start {Application.isPlaying.ToString()}");

        var loadScenarioDataCoroutine = Util.LoadAddressablesByLabel<ScenarioData>("ScenarioData", (data) =>
        {
            _scenarioData.Add(data.SceneName, data);
            //Debug.Log($"SceneLoadManager: Found scenario data {data.SceneName}");
        });
        StartCoroutine(loadScenarioDataCoroutine);


        Util.DontDestroyOnLoad(gameObject);

        await Task.Delay(3000);

        if (_loadedScene == null && LoadWaitingRoom && !_loadInProgress)
        {
            Debug.Log("SceneLoadManager: Loading waiting room");
            await LoadScene(WaitingRoomScene);
        }
        else
        {
            Debug.Log("SceneLoadManager: Not loading waiting room");
        }
    }

    public ScenarioData FindScenarioData(string sceneName)
    {
        if (_scenarioData.TryGetValue(sceneName, out var scenarioData))
            return scenarioData;

        return null;
    }

    private void OnDisconnectedFromServer()
    {
        Debug.Log("SceneLoadManager: Disconnected from server");
        _requestedWorldState = false;

        if (LoadWaitingRoom)
            _ = LoadScene(WaitingRoomScene);
        else if (!_loadInProgress)
            _ = UnloadActiveScene();
        else
            _expectedScene = null;
    }

    private async void OnSimStateChanged(VRNSimState obj)
    {
        Debug.Log($"SceneLoadManager: SimState changed scene: {obj.ActiveScene}");
        if (obj.ActiveScene != null && obj.ActiveScene.Length > 0 && _loadedScene != obj.ActiveScene)
        {
            await LoadScene(obj.ActiveScene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SceneManager: SceneLoaded {scene.name}");

        if (mode != LoadSceneMode.Additive)
            return;

        if (scene.name == WaitingRoomScene)
        {

        }

        //if (_sceneLoaded)
        //{
        //    await UnloadActiveScene();
        //}

        Debug.Log($"Scene Loaded : {scene.name}");
        _activeScene = scene;
        _sceneLoaded = true;
        SceneManager.SetActiveScene(_activeScene);
        //if (!string.IsNullOrEmpty(_customSceneFileName))
        //{
        //    ScenarioSaveLoad.Instance.LoadScenarioFromFile(_customSceneFileName, false);
        //}

        if (scene.name == _expectedScene && !_loadInProgress)
        {
            HandleSceneLoadCompleted();
        }

        
    }

    private void HandleSceneLoadCompleted()
    {
        NetworkManager.SendClientLoadedScene();

        if (!_requestedWorldState)
        {
            NetworkManager.SendRequestWorldState();
            _requestedWorldState = true;
        }

        SceneChanged?.Invoke();
        EnteredSimulationScene?.Invoke();
    }

    async void OnLoadSceneCommand(VRNLoadScene loadScene)
    {
        Debug.Log($"Received Command: Load Scene {loadScene.SceneName}");
        if (_sceneLoad != null && _sceneLoadName == loadScene.SceneName)
        {
            _sceneLoad.allowSceneActivation = loadScene.AllowSceneActivation;
            return;
        }

        await LoadScene(loadScene.SceneName);
    }
    public void ReturnToEditor()
    {
        ScenarioInitializer scenarioInit = ScenarioInitializer.Instance;
        if(scenarioInit == null ) { scenarioInit = this.gameObject.AddComponent<ScenarioInitializer>(); }
        scenarioInit.ReturnToEditor();
    }

    public static bool IsSceneNameValid(string sceneName)
    {
        if (IsSceneCustomScenario(sceneName, out var customSceneFileName))
        {
            if (Path.GetExtension(customSceneFileName) != ".json")
                customSceneFileName += ".json";

            var systemManager = SystemManager.GetDefault();

            string filePath = Path.Combine(systemManager.SystemConfig.ScenariosFolder, customSceneFileName);
            if (File.Exists(filePath))
                return true;
            else
                return false;
        }
        else
        {
            return true;
            //var scenarioData = FindScenarioData(sceneName);
            //if (scenarioData == null)
            //    return false;
            //else
            //    return true;
        }
    }

    private static bool IsSceneCustomScenario(string sceneName, out string customSceneFileName)
    {
        customSceneFileName = String.Empty;

        if (!sceneName.Contains("CustomScenario:"))
            return false;

        customSceneFileName = sceneName.Split(':')[1];

        return true;
    }

    private void StartSceneLoad(string sceneName)
    {
        //if (_addrSceneLoad != null)
        //{
        //    Addressables.Release((AsyncOperationHandle)_addrSceneLoad);
        //}

        ScenarioSaveLoad.Settings.ResetSettings();
        ScenarioSaveLoad.ClearActiveScenario();

        _navMeshGenerator = null;
        _sceneLoad = null;
        _addrSceneLoad = null;

        _customSceneFileName = String.Empty;
        if (_sceneLoadName.Contains("CustomScenario:"))
        {
            _customSceneFileName = sceneName.Split(':')[1];
            _sceneLoad = SceneManager.LoadSceneAsync("ScenarioTemplateMFIRE", LoadSceneMode.Additive);
            ScenarioName = System.IO.Path.GetFileNameWithoutExtension(_customSceneFileName);
            _sceneLoad.allowSceneActivation = true;
        }
        else
        {
            ScenarioName = sceneName;

            var scenarioData = FindScenarioData(sceneName);
            if (scenarioData == null || !scenarioData.AddressableScene)
            {
                _sceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                _sceneLoad.allowSceneActivation = true;
            }
            else
            {
                Debug.Log($"Loading addressable scene: {sceneName}");
                _addrSceneLoad = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }
    }

    //private void StartSceneLoad(string sceneName, string fileName)
    //{

    //    if(!string.IsNullOrEmpty(fileName))
    //    {
    //        ScenarioSaveLoad.Instance.LoadScenarioFromFile(fileName, true, false);
    //    }
    //}

    private float CheckLoadProgress()
    {
        if (_sceneLoad != null)
        {
            if (_sceneLoad.isDone || _sceneLoad.progress >= 1)
            {
                return FinalizeSceneLoad();
            }
            else
                return _sceneLoad.progress;
        }
        else if (_addrSceneLoad != null)
        {
            var handle = (AsyncOperationHandle)_addrSceneLoad;
            if (handle.IsDone)
            {
                //Addressables.Release(handle);
                //_addrSceneLoad = null;
                return FinalizeSceneLoad();
            }
            else
            {
                return handle.PercentComplete;
            }
        }

        return -1;
    }

    private float FinalizeSceneLoad()
    {
        float loadStatus = 1;

        //finish loading scene _sceneLoadName
        //load will continue polling until 1 or greater is returned


        if (_sceneLoadName.Contains("CustomScenario:") && !string.IsNullOrEmpty(_customSceneFileName))
        {
            if (!_customScenarioLoadStarted)
            {
                _customScenarioLoadStarted = true;
                ScenarioSaveLoad.Instance.LoadScenarioFromFile(_customSceneFileName, false, false);

                ApplySceneSettings(ScenarioSaveLoad.Settings);
            }

            loadStatus = ScenarioSaveLoad.Instance.CheckLoadStatus();

            if (!ScenarioSaveLoad.Instance.IsLoadInProgress && loadStatus < 1)
            {
                Debug.LogError($"SceneLoadManager: Custom scenario load failed before loading entire scenario");
                loadStatus = 1;
            }
        }
        else
        {
            //check for scenario configuraiton override
            var mineSceneConfig = FindObjectOfType<MineSceneConfiguration>();
            if (mineSceneConfig != null)
            {
                ScenarioSaveLoad.Settings = mineSceneConfig.MineParameters;
            }
            else
            {
                ScenarioSaveLoad.Settings = new GlobalMineParameters();
            }

            var ventilationManager = VentilationManager.GetDefault(gameObject);
            if (ventilationManager != null)
                ventilationManager.DefaultAtmosphere = MineAtmosphere.NormalAtmosphere;

            Debug.Log($"SceneLoadManager: Loaded built in scene, bg4 duration: {ScenarioSaveLoad.Settings.BG4DurationMinutes}");
        }

        var profileID = ScenarioSaveLoad.Settings.MinerProfileID;
        ScenarioSaveLoad.Settings.MinerProfile = LoadableAssetManager.FindMinerProfile(profileID);


        if (SystemManager.GraphicsConfig.LODLevelDebugView)
        {
            EnableLODLevelDebugView();
        }

        try
        {
            SceneLoadFinalized?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in scene load finalize: {ex.Message} {ex.StackTrace}");
        }

        return loadStatus;
    }
    
    private void ApplySceneSettings(GlobalMineParameters parameters)
    {
        var ventilationManager = VentilationManager.GetDefault(gameObject);

        ventilationManager.DefaultAtmosphere = parameters.StaticAtmosphere;

        if (!parameters.UseMFire)
        {
            var ventControl = FindObjectOfType<VentilationControl>();
            if (ventControl != null)
            {
                ventControl.VentilationProvider = VentilationProvider.StaticVentilation;
            }
        }
    }

    private void EnableLODLevelDebugView()
    {
        List<Material> mats = new List<Material>();

        var baseMat = Resources.Load<Material>("LODLevelDebugMat");

        Material[] lodMats = new Material[4];
        lodMats[0] = CreateLODLevelDebugMat(baseMat, Color.red);
        lodMats[1] = CreateLODLevelDebugMat(baseMat, Color.green);
        lodMats[2] = CreateLODLevelDebugMat(baseMat, Color.blue);
        lodMats[3] = CreateLODLevelDebugMat(baseMat, Color.yellow);

        var lodGroups = FindObjectsByType<LODGroup>(FindObjectsSortMode.None);

        foreach (var lodGroup in lodGroups)
        {
            var lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length && i < lodMats.Length; i++)
            {
                var lod = lods[i];
                foreach (var rend in lod.renderers)
                {
                    if (rend == null)
                        continue;

                    mats.Clear();
                    rend.GetSharedMaterials(mats);

                    for (int j = 0; j < mats.Count; j++)
                    {
                        mats[j] = lodMats[i];
                    }

                    rend.SetSharedMaterials(mats);
                    mats.Clear();

                    //rend.sharedMaterial = lodMats[i];
                }
            }
        }
    }

    private Material CreateLODLevelDebugMat(Material baseMat, Color color)
    {
        var mat = Instantiate<Material>(baseMat);
        mat.color = color;
        mat.SetColor("_BaseColor", color);

        return mat;
    }


    public async Task LoadScene(string name)
    {
        while (!_initialized)
        {
            await Task.Delay(10);
        }

        _expectedScene = name;
        _customScenarioLoadStarted = false;
        _requestedWorldState = false;

        if (_loadInProgress)
        {
            Debug.Log($"SceneLoadManager: Load in progress, expected scene changed to {_expectedScene}");
            return;
        }


        if (_sceneLoad != null)
            return;

        _loadInProgress = true;
        _loadedScene = null;

        Debug.Log($"SceneLoadManager: Loading scene {name}....");

        SceneUnloading?.Invoke();

        await SceneFadeManager.FadeOut();

        if (name != _expectedScene)
        {
            _loadInProgress = false;
            Debug.Log($"SceneLoadManager: Expected scen changed from {name} to {_expectedScene} while waiting, restarting load");
            return; // expected scene changed while waiting
        }

        try
        {
            await UnloadActiveScene();

            if (name != _expectedScene)
            {
                _loadInProgress = false;
                Debug.Log($"SceneLoadManager: Expected scen changed from {name} to {_expectedScene} while waiting, restarting load");
                return; // expected scene changed while waiting
            }

            _loadedScene = name;            
            _sceneLoadName = name;

            StartSceneLoad(_sceneLoadName);
           
            var playerName = SystemManager.SystemConfig.MultiplayerName;
            if (playerName == null || playerName.Length <= 0)
                playerName = SystemInfo.deviceName;

            var clientState = new VRNClientState
            {
                PlayerName = playerName,
                SceneLoadState = VRNSceneLoadState.Loading,
                SceneName = _sceneLoadName,
            };

            NetworkManager.SendVRClientState(clientState);

            while (true && gameObject != null)
            {
                var sceneLoadProgress = CheckLoadProgress();
                //if (_sceneLoad == null)
                //    break;

                if (sceneLoadProgress < 0)
                    break;

                //if (_sceneLoad.isDone)
                if (sceneLoadProgress >= 1)
                {
                    //clientState = new VRNClientState
                    //{
                    //    PlayerName = playerName,
                    //    SceneLoadState = VRNSceneLoadState.Active,
                    //    SceneName = _sceneLoadName,
                    //};

                    //NetworkManager.SendVRClientState(clientState);
                    _sceneLoad = null;
                    //_addrSceneLoad = null;
                    break;
                }
               

                await Task.Delay(50);
            }

            _navMeshGenerator = FindObjectOfType<NavMeshRuntimeGenerator>();
            if (_navMeshGenerator != null)
            {
                _navMeshGenerator.StartNavMeshGeneration();

                //while (!_navMeshGenerator.NavMeshReady)
                //{
                //    await Task.Delay(50);
                //}
            }


            clientState = new VRNClientState
            {
                PlayerName = playerName,
                SceneLoadState = VRNSceneLoadState.Active,
                SceneName = _sceneLoadName,
            };

            NetworkManager.SendVRClientState(clientState);

        }
        finally
        {
            if (_sceneLoad != null)
            {
                Debug.LogError($"Error - _sceneLoad not null after LoadScene {_sceneLoad.ToString()}");
                _sceneLoad = null;
            }

            await Task.Delay(600);
            await SceneFadeManager.FadeIn();

            if (name == WaitingRoomScene)
                _inWaitingRoom = true;
            else
                _inWaitingRoom = false;

            await Task.Delay(500);

            _loadInProgress = false;

            HandleSceneLoadCompleted();
        }

    }

    async Task UnloadActiveScene()
    {
        if (!_sceneLoaded || !_activeScene.isLoaded)
        {
            Debug.Log("No scene to unload!");
            return;
        }

        Debug.Log($"Unloading Scene");

        try
        {

            _sceneUnload = SceneManager.UnloadSceneAsync(_activeScene);
            while (_sceneUnload != null && !_sceneUnload.isDone)
            {
                await Task.Yield();
            }
            _sceneLoaded = false;
            _sceneUnload = null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error unloading scene: {ex.Message}");

            _sceneUnload = null;
            _sceneLoaded = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_sceneLoad == null && _expectedScene != null && _expectedScene != _loadedScene && !_loadInProgress)
        {
            //_loadedScene = _expectedScene;
            //await Task.Delay(1500);
            //if (_sceneLoad == null && _expectedScene != _loadedScene)
            //await LoadScene(_expectedScene, true);

            _ = LoadScene(_expectedScene);
        }

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
