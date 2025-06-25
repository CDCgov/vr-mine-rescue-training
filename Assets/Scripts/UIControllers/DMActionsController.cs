using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class DMActionsController : MonoBehaviour, IMinimizableWindow
{
    public NetworkManager NetworkManager;
    public TeleportManager TeleportManager;
    public PlayerManager PlayerManager;

    public POIManager POIManager;
    public GameObject ActionButtonPrefab;
    public RectTransform ActionButtonsParent;
    public Slider DMLightSlider;

    public Light DMLight;

    public Toggle DMMuteToggle;
    public Toggle DMDeafenToggle;
    public Button MutePlayers;
    public Button UnmutePlayers;
    public Button DeafenPlayers;
    public Button UndeafenPlayers;

#if DISSONANCE
    public Dissonance.DissonanceComms DissonanceComms;

#endif

    private AsyncOperation _sceneLoad = null;
    private string _sceneLoadName;

    private Scene _activeScene;
    private Dictionary<string, GameObject> _actionButtons;

    public event Action<string> TitleChanged;
    private float _dmLightMaxIntensity = 100;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _actionButtons = new Dictionary<string, GameObject>();

        //AddActionButton("Load VRDemoScene", () => { LoadScene("VRDemoScene2"); });
        //AddActionButton("Load Construct", () => { LoadScene("TheConstruct"); });
        //AddActionButton("Activate Scene", CompleteSceneLoad);

        //SceneManager.activeSceneChanged += OnSceneChanged;
        //POIManager.POIAdded += OnPOIAdded;
        //POIManager.POIRemoved += OnPOIRemoved;

        DMLightSlider.onValueChanged.AddListener(LightSliderChanged);
        DMMuteToggle.onValueChanged.AddListener(DMMuteToggleChanged);
        DMDeafenToggle.onValueChanged.AddListener(DMDeafenToggleChanged);
        MutePlayers.onClick.AddListener(MuteAllPlayersButton);
        UnmutePlayers.onClick.AddListener(UnmuteAllPlayersButton);
        DeafenPlayers.onClick.AddListener(DeafenAllPlayersButton);
        UndeafenPlayers.onClick.AddListener(UndeafenAllPlayersButton);

        if (DMLight != null)
        {
            _dmLightMaxIntensity = DMLight.intensity * 2;
            DMLightSlider.value = 0.5f;
        }

#if DISSONANCE
        if(DissonanceComms == null)
        {
            DissonanceComms = GameObject.FindObjectOfType<Dissonance.DissonanceComms>();
        }
#endif
    }

    private void OnDestroy()
    {
        //SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void DMMuteToggleChanged(bool value)
    {
#if DISSONANCE
        DissonanceComms.IsMuted = value;
#endif
    }
    private void DMDeafenToggleChanged(bool value)
    {
#if DISSONANCE
        DissonanceComms.IsDeafened = value;
#endif
    }

    private void MuteAllPlayersButton()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.MuteEnabled = true;
        }

        //UIToggleMuteButton[] uIToggleMuteButtons = GameObject.FindObjectsOfType<UIToggleMuteButton>();
        //foreach(UIToggleMuteButton toggle in uIToggleMuteButtons)
        //{
        //    toggle.UIToggle.isOn = true;
        //}
    }
    private void UnmuteAllPlayersButton()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.MuteEnabled = false;
        }
        //UIToggleMuteButton[] uIToggleMuteButtons = GameObject.FindObjectsOfType<UIToggleMuteButton>();
        //foreach (UIToggleMuteButton toggle in uIToggleMuteButtons)
        //{
        //    toggle.UIToggle.isOn = false;
        //}
    }
    private void DeafenAllPlayersButton()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.DeafenEnabled = true;
        }
        //UIToggleDeafenButton[] uIToggleMuteButtons = GameObject.FindObjectsOfType<UIToggleDeafenButton>();
        //foreach (UIToggleDeafenButton toggle in uIToggleMuteButtons)
        //{
        //    toggle.UIToggle.isOn = true;
        //}
    }
    private void UndeafenAllPlayersButton()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.DeafenEnabled = false;
        }
        //UIToggleDeafenButton[] uIToggleMuteButtons = GameObject.FindObjectsOfType<UIToggleDeafenButton>();
        //foreach (UIToggleDeafenButton toggle in uIToggleMuteButtons)
        //{
        //    toggle.UIToggle.isOn = false;
        //}
    }

    private void LightSliderChanged(float value)
    {
        if (DMLight != null)
        {
            DMLight.intensity = _dmLightMaxIntensity * value;
        }
    }

    //private void OnPOIRemoved(PointOfInterest poi)
    //{
    //    if (poi.POIType != POIType.CameraPosition)
    //        return;

    //    Debug.Log($"RemovePOI :{poi.name}");
    //    GameObject obj;
    //    if (_actionButtons.TryGetValue(poi.name, out obj))
    //    {
    //        Destroy(obj);
    //        _actionButtons.Remove(poi.name);
    //    }
    //}

    //private void OnPOIAdded(PointOfInterest poi)
    //{
    //    //Removing POIs from action bar, now handled in map
    //    //if (poi.POIType != POIType.CameraPosition)
    //    //    return;

    //    //Debug.Log($"AddPOI :{poi.name}");

    //    //if (poi is VRPointOfInterest)
    //    //{
    //    //    //extra vr scaling
    //    //}

    //    //AddActionButton(poi.name, () => { TeleportToPOI(poi); });
    //}

    //private void OnSceneChanged(Scene arg0, Scene arg1)
    //{
    //    //Commenting out to remove POIs
    //    //var pois = POIManager.GetPOIs();

    //    //foreach (var poi in pois)
    //    //{
    //    //    if (poi.POIType != POIType.CameraPosition)
    //    //        continue;


    //    //    if (poi is VRPointOfInterest)
    //    //    {
    //    //        //extra vr scaling
    //    //    }

    //    //    AddActionButton(poi.name, () => { TeleportToPOI(poi); });
    //    //}
    //}

    private void TeleportToPOI(PointOfInterest poi)
    {
        //NetworkManager.SendTeleportAll(poi.name, Time.time);
        TeleportManager.TeleportToPOI(poi);
    }

    void AddActionButton(string text, UnityAction action)
    {
        if (_actionButtons.ContainsKey(text))
            return;

        var obj = Instantiate<GameObject>(ActionButtonPrefab);
        var btn = obj.GetComponent<Button>();
        var txt = obj.GetComponentInChildren<Text>();

        txt.text = text;
        btn.onClick.AddListener(action);

        obj.transform.SetParent(ActionButtonsParent);

        _actionButtons[text] = obj;
    }

    void LoadScene(string name)
    {

        //_sceneLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        //_sceneLoad.allowSceneActivation = false;
        _sceneLoadName = name;
        NetworkManager.SendLoadScene(name, false);
    }

    void CompleteSceneLoad()
    {
        //if (_sceneLoad != null)
        //{
        //    _sceneLoad.completed += (op) => { _activeScene = SceneManager.GetSceneByName(_sceneLoadName); };
        //    _sceneLoad.allowSceneActivation = true;

        //}

        if (_sceneLoadName == null)
            return;

        NetworkManager.SendLoadScene(_sceneLoadName, true);
    }

    void UnloadActiveScene()
    {
        //if (_activeScene == null)
        //{
        //    Debug.Log("No scene to unload!");
        //    return;
        //}

        //SceneManager.UnloadSceneAsync(_activeScene);

        
    }

    // Update is called once per frame
    void Update()
    {
        //if (_sceneLoad != null)
        //{
        //    if (_sceneLoad.isDone)
        //    {
        //        Debug.Log("Scene load completed");
                
        //    }
        //    else
        //    {
        //        Debug.Log($"Scene Load Progress: {_sceneLoad.progress:F2}");
        //    }
        //}
    }

    public string GetTitle()
    {
        return "Actions";
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void AssignTaskbarButton(Button button)
    {

    }
}
