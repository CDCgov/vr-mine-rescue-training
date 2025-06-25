using NIOSH_EditorLayers;
using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manager script that handles initializing a template scene and applying created scenario data.
/// Called from a load scenario option OR from play button in scenario editor scene | can plug into current load system check for custom or prebuilt
/// After call, additive load required template scene depending on vent system in use
/// Load scenario data if needed
/// Initialize required scripts and make needed attachments
/// Run scenario either multiplayer (just from load) or singleplayer (from load or from editor)
/// 
/// 
/// </summary>
public class ScenarioInitializer : MonoBehaviour
{
    SceneLoadManager sceneLoadManager;
    public static ScenarioInitializer Instance;
    [SerializeField] Button continueButton;
    public Action onCustomScenarioLoad;
    //public HierarchyContainer hierarchy;

    public bool IsReturningFromPlayMode;
    public bool GenerateMineOnStart;
    public string ScenarioToLoad = null;

    private void OnEnable()
    {
        if (Instance == null) 
            Instance = this;

        else if(Instance != this) 
        { 
            Debug.Log("DESTROYING A SCENARIOINIT"); 
            StopAllCoroutines();
            this.enabled = false;
            Destroy(this.gameObject);            
            return;
        }
        
        //if (ScenarioSaveLoad.Instance == null) { GameObject.FindObjectOfType<ScenarioSaveLoad>(); }
        //if (ScenarioSaveLoad.Instance == null) { gameObject.AddComponent<ScenarioSaveLoad>(); }
        if (ScenarioSaveLoad.Instance != null)
        {
            ScenarioSaveLoad.Instance.onLoadComplete += LoadCompleted;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnDisable()
    {
        if (ScenarioSaveLoad.Instance != null)
            ScenarioSaveLoad.Instance.onLoadComplete -= LoadCompleted;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "BAH_ScenarioEditor")
        {
            StartCoroutine(StartEditor());
            //StartEditor();
        }
    }

    IEnumerator StartEditor()
    {
        //yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();

        MineBuilderUI builderUI = FindObjectOfType<MineBuilderUI>();
        if (IsReturningFromPlayMode)
        {
            GenerateMineOnStart = false;
            IsReturningFromPlayMode = false;      
            DelayLoadEditor("PlayModeSave", false);
        }
        else if (ScenarioToLoad != null && ScenarioToLoad.Length > 0)
        {
            DelayLoadEditor(ScenarioToLoad, true);
            ScenarioToLoad = null;            
        }
        else
            ScenarioSaveLoad.Instance.ClearWorkingScenarioName();

        if (GenerateMineOnStart)
        {
            ScenarioSaveLoad.Instance.ClearWorkingScenarioName();
            builderUI.GenerateMine(true);
        }
        else
        {
            //builderUI.CancelBuild();
            //builderUI.GenerateMine(false);
            builderUI.SkipMineGeneration();
        }
    }

    bool activateReturnButton = false;
    //public IEnumerator LoadCustomScenario(string fileName)
    //{
    //    activateReturnButton = false;
    //    if (SceneManager.GetSceneByName("ScenarioTemplateMFIRE").IsValid())
    //    {
    //        yield return SceneManager.UnloadSceneAsync("ScenarioTemplateMFIRE");
    //    }
    //    if (sceneLoadManager == null) { sceneLoadManager = FindObjectOfType<SceneLoadManager>(); }
    //    DelayedLoad(fileName);
    //}

    public void ReturnToEditor()
    {
        StopAllCoroutines();
        if (SceneManager.GetSceneByName("VRWaitingRoom").IsValid())
        {
            SceneManager.UnloadSceneAsync("VRWaitingRoom");
        }
        if (SceneManager.GetSceneByName("PlayModeMainScene").IsValid())
        {
            SceneManager.UnloadSceneAsync("PlayModeMainScene");
        }
        ClearAllManagers();
        SceneManager.LoadScene("BAH_ScenarioEditor");
        //StartCoroutine(DelayLoadEditor("PlayModeSave", false));
        Debug.Log("DelayLoadEditor_Fire from ReturnToEditor()");

    }

    public void LoadScenarioFromMainMenu(string scenarioName)
    {
        //IsReturningFromPlayMode = true;
        //SceneManager.LoadScene("BAH_ScenarioEditor");
        //StartCoroutine(DelayLoadEditor(scenarioName, true));
        IsReturningFromPlayMode = false;
        ScenarioToLoad = scenarioName;
        SceneManager.LoadScene("BAH_ScenarioEditor");
    }

    IEnumerator EstablishDMScene(string fileName)
    {
        activateReturnButton = true;
        yield return SceneManager.LoadSceneAsync("PlayModeMainScene");
        //yield return new WaitForSeconds(0.5f);
        //if (sceneLoadManager == null) { sceneLoadManager = FindObjectOfType<SceneLoadManager>(); }
        //sceneLoadManager.LoadWaitingRoom = false;
        DelayedLoad(fileName);
        Debug.Log("DelayLoad_Fire from EstalbishDMSScene()");
    }

    void DelayLoadEditor(string scenarioName, bool setWorkingScenarioName)
    {
        Debug.Log("DelayLoadEditor_Start");

        //yield return new WaitForSeconds(0.25f);
        ScenarioSaveLoad.Instance.LoadScenarioFromFile(scenarioName, true, setWorkingScenarioName);

    }

    public void PlayModeLoad(string fileName)
    {
        if (SceneManager.GetSceneByName("CustomScenarioBlank").IsValid())
        {
            SceneManager.UnloadSceneAsync("CustomScenarioBlank");
        }
        if (SceneManager.GetSceneByName("ScenarioTemplateMFIRE").IsValid())
        {
            SceneManager.UnloadSceneAsync("ScenarioTemplateMFIRE");
        }
        if (!SceneManager.GetSceneByName("PlayModeMainScene").IsValid())
        {
            StartCoroutine(EstablishDMScene(fileName));
            //EstablishDMScene(fileName);
        }
        else
        {
            if (sceneLoadManager == null) { sceneLoadManager = FindObjectOfType<SceneLoadManager>(); }
            activateReturnButton = false;
            sceneLoadManager.LoadWaitingRoom = true;
            DelayedLoad(fileName);
            Debug.Log("DelayLoad_Fire from LoadCustomScenario()");
        }
        IsReturningFromPlayMode = true;
    }

    public async Task PlayLoadedScenario()
    {
        LayerManager.Instance.ChangeLayer(LayerManager.EditorLayer.Mine);
        await ScenarioSaveLoad.Instance.SaveCurrentScenario("PlayModeSave", false);    //TODO replace with some kind of validation and way to persist save name during editing.
        ClearAllManagers();
        PlayModeLoad("PlayModeSave");
    }

    async void DelayedLoad(string fileName)
    {
        if (SceneManager.GetSceneByName("VRWaitingRoom").IsValid())
        {
            SceneManager.UnloadSceneAsync("VRWaitingRoom");
        }

        if (sceneLoadManager == null) { sceneLoadManager = FindObjectOfType<SceneLoadManager>(); }
        await sceneLoadManager.LoadScene("CustomScenario:" + fileName);
        //yield return new WaitForSeconds(0.5f);
        //ScenarioSaveLoad.Instance.LoadScenarioFromFile(fileName, false);
        if (sceneLoadManager.ReturnToEditorButton != null)
        {
            sceneLoadManager.ReturnToEditorButton.gameObject.SetActive(activateReturnButton);
            //sceneLoadManager.ReturnToEditorButton.interactable = activateReturnButton;
            sceneLoadManager.ReturnToEditorButton.image.color = activateReturnButton ? Color.white : Color.clear;
        }
        Debug.Log("DelayLoad_Start");
    }

    private Bounds ComputeVentVFXBounds()
    {
        Bounds vfxBounds = new Bounds();
        bool boundsInitialized = false;

        var mineSegments = FindObjectsOfType<MineSegment>();
        int count = 0;

        foreach (var mineSeg in mineSegments)
        {
            if (mineSeg == null || !mineSeg.IncludeInMap)
                continue;

            if (!mineSeg.TryGetComponent<BoxCollider>(out var collider))
                continue;

            if (!boundsInitialized)
            {
                vfxBounds = collider.bounds;
                boundsInitialized = true;
            }
            else
            {
                vfxBounds.Encapsulate(collider.bounds);
            }

            count++;
            
        }

        Debug.Log($"ScenarioInitializer computed VFX bounds using {count} segments {vfxBounds}");

        return vfxBounds;
    }

    async void LoadCompleted()
    {
        onCustomScenarioLoad?.Invoke();

        //var h = GameObject.Find("HierarchyWindow");
        var cm = GameObject.Find("ContextMenu");

        if (cm) { cm.SetActive(false); }
        //if (h)
        //{
        //    hierarchy = h.GetComponent<HierarchyContainer>();
        //    if (hierarchy) hierarchy.Reinitialize();
        //}

        VentilationControl control = FindObjectOfType<VentilationControl>();
        if(control!= null)
        {
            //recompute ventilation VFX bounds
            control.VFXBounds = ComputeVentVFXBounds();
            await control.InitializeVentilation();
        }

        var staticVentManager = FindObjectOfType<StaticVentilationManager>();
        if (staticVentManager != null)
        {
            staticVentManager.LoadStaticZones();
        }


    }

    private void ClearAllManagers()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("Manager");
        foreach (GameObject obj in managers)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
    }
}