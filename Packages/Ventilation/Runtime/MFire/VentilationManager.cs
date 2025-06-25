using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFireProtocol;
using System.Threading.Tasks;
using System.IO;
//using LiteNetLib;
using UnityEngine.AddressableAssets;
using Google.Protobuf;
using UnityEngine.VFX;

public enum VentilationProvider
{
    StaticVentilation,
    MFIRE,
    None,
}


public class VentilationManager : SceneManagerBase
{

    //public static VentilationManager GetDefault()
    //{
    //    var obj = GameObject.Find("VentilationManager");
    //    if (obj != null)
    //    {
    //        return obj.GetComponent<VentilationManager>();
    //    }

    //    return null;
    //}

    public static VentilationManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<VentilationManager>("VentilationManager", true);
    }

    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public SelectedObjectManager SelectedObjectManager;
    //public StaticVentilationManager StaticVentilationManager;

    //public VentilationProvider VentilationProvider;    
    public MineAtmosphere DefaultAtmosphere;

    //[System.NonSerialized]
    public VentUIObj SelectedVentUIObj
    {
        get
        {
            return _selectedVentUIObj;
        }
        set
        {
            _selectedVentUIObj = value;
            SelectedVentObjChanged?.Invoke(value);
        }
    }

    public event Action<VentGraph> VentGraphReset;
    public event Action<VentUIObj> SelectedVentObjChanged;
    public event Action<VentGraph> VentilationWillUpdate;
    public event Action VentilationUpdated;
    public event Action<bool> VentVisualizationChanged;
    

    private VentilationControl _ventilationControl;

    public float VentUIYOffset = 3.0f;
    public GameObject VisualizationPrefab;
    public GameObject _ventUIAirwayPrefab;
    public GameObject _ventUIJunctionPrefab;
    public GameObject _ventUIFirePrefab;

    public event System.Action VentVisChanged;

    public bool IsVisEnabledDM
    {
        get { return _visEnabledDM; }
        set
        {
            if (value != _visEnabledDM)
            {
                _visEnabledDM = value;
                RaiseVentVisChanged();
            }
        }
    }

    public bool IsVisEnabledPlayer
    {
        get { return _visEnabledPlayer; }
        set
        {
            if (value != _visEnabledPlayer)
            {
                _visEnabledPlayer = value;
                RaiseVentVisChanged();
            }
        }
    }

    public bool IsVisEnabledSpectator
    {
        get { return _visEnabledSpectator; }
        set
        {
            if (value != _visEnabledSpectator)
            {
                _visEnabledSpectator = value;
                RaiseVentVisChanged();
            }
        }
    }

    private GameObject _ventVisualization;
    private VentUIObj _selectedVentUIObj;

    private string _visPrefabAddress = null;
    private string _visDataAddress = null;
    private bool _visEnabledDM = false;
    private bool _visEnabledPlayer = false;
    private bool _visEnabledSpectator = false;

    public bool IsUIVisible
    {
        get
        {
            if (_uiParent == null || !_uiParent.activeInHierarchy)
                return false;
            else
                return true;
        }
    }

    public bool IsVentVisualizationVisible
    {
        get
        {
            if (_ventVisualization == null)
                return false;
            else
                return _ventVisualization.activeSelf;
        }
    }

    public Action VentilationReady;
    //public event Action<Vector3, MineAtmosphere> ReceivedMineAtmosphere;
    //public Action SimulationChanged;

    private GameObject _uiParent = null;

    private HashSet<String> _ventVisualizers = new HashSet<string>();
    //private VentilationControl _ventControl;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);

        if (NetworkManager == null)
        {
            return;
        }

        SelectedObjectManager.SelectionChanged += OnSelectedObjectChanged;

        NetworkManager.gameObject.tag = "Manager";
        NetworkManager.RegisterHandler(VRNPacketType.VentilationUpdate, VentilationUpdateHandler);
        //NetworkManager.RegisterHandler(VRNPacketType.RequestMineAtmosphere, RequestMineAtmosphereHandler);
        //NetworkManager.RegisterHandler(VRNPacketType.SendMineAtmosphere, SendMineAtmosphereHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendVentGraph, SendVentGraphHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SetVentVisualization, SetVentVisualizationHandler);

        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkManager.ClientRequestedWorldState += OnClientRequestedWorldState;

        InitializeSceneVentilation();

    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        if (obj != null && obj.TryGetComponent<VentUIObj>(out var ventObj))
        {
            SelectedVentUIObj = ventObj;
        }
    }

    private void OnClientRequestedWorldState(int clientID)
    {
        
    }

    public VentilationProvider GetVentilationProvider()
    {
        if (_ventilationControl == null)
            return VentilationProvider.None;
        else
            return _ventilationControl.VentilationProvider;
    }

    private void OnDestroy()
    {
        if (NetworkManager != null)
        {
            NetworkManager.UnregisterHandler(VRNPacketType.VentilationUpdate, VentilationUpdateHandler);
            //NetworkManager.UnregisterHandler(VRNPacketType.RequestMineAtmosphere, RequestMineAtmosphereHandler);
            //NetworkManager.UnregisterHandler(VRNPacketType.SendMineAtmosphere, SendMineAtmosphereHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.SendVentGraph, SendVentGraphHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.SetVentVisualization, SetVentVisualizationHandler);

            NetworkManager.ClientRequestedWorldState -= OnClientRequestedWorldState;
        }

        if (SelectedObjectManager != null)
            SelectedObjectManager.SelectionChanged -= OnSelectedObjectChanged;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        InitializeSceneVentilation();
    }

    void InitializeSceneVentilation()
    {
        _ventilationControl = FindObjectOfType<VentilationControl>();
        if (_ventilationControl != null)
        {
            _ventilationControl.VentilationUpdated += OnVentilationUpdated;
        }
    }

    private void OnVentilationUpdated()
    {
        if (NetworkManager.IsServer)
        {
            var graph = GetVentilationGraph();
            if (graph != null)
            {
                //send out the ventilation data
                VRNVentGraph vrnGraph = new VRNVentGraph();
                graph.SaveTo(vrnGraph);

                NetworkManager.SendNetMessage(VRNPacketType.SendVentGraph, vrnGraph, reliable: true);
            }
        }

        VentilationUpdated?.Invoke();

        if (_uiParent != null)
            _ventilationControl.VentGraph.UpdateUIObjects(_uiParent.transform, VentUIYOffset);
    }

    public bool IsVentilationReady
    {
        get
        {
            if (_ventilationControl == null)
                return false;

            return _ventilationControl.IsVentilationReady;
        }
    }


    public bool AutoAdvanceEnabled
    {
        get
        {
            if (_ventilationControl == null)
                return false;

            return _ventilationControl.AutoAdvanceEnabled;
        }
        set
        {
            if (_ventilationControl != null)
                _ventilationControl.AutoAdvanceEnabled = value;
        }
    }

    void RaiseVentVisChanged()
    {
        SendVentVisState();
        VentVisChanged?.Invoke();
    }

    public VentilationControl GetVentilationControl()
    {
        return _ventilationControl;
    }


    public VentGraph GetVentilationGraph()
    {
        if (_ventilationControl == null)
            return null;

        return _ventilationControl.VentGraph;
    }

    public void AddVentControl(VentControl control)
    {
        if (_ventilationControl != null)
        {
            _ventilationControl.VentGraph.AddVentControl(control);
            if (control.LinkedAirway != null)
                control.LinkedAirway.UpdateAirway();
        }
    }

    public void RaiseVentilationWillUpdate()
    {
        VentilationWillUpdate?.Invoke(_ventilationControl.VentGraph);
    }

    public void RaiseVentGraphReset()
    {
        VentGraphReset?.Invoke(_ventilationControl.VentGraph);
    }

    public GameObject GetVisualizationObject()
    {
        return _ventVisualization;
    }

    public void AddVentVisualizer(string name)
    {
        _ventVisualizers.Add(name);
        ShowVentVisualization(true);
    }

    public void RemoveVentVisualizer(string name)
    {
        _ventVisualizers.Remove(name);
        if (_ventVisualizers.Count <= 0)
            ShowVentVisualization(false);
    }

    public void SetVisData(string visDataAddress)
    {
        if (visDataAddress != null && visDataAddress.Length <= 0)
            visDataAddress = null;

        if (_visDataAddress != visDataAddress)
        {

            _visDataAddress = visDataAddress;
            RaiseVentVisChanged();
        }

        //if (_visPrefabAddress != null && _visDataAddress != null && _visPrefabAddress.Length > 0 && _visDataAddress.Length > 0)
        //    ShowVentVisualization(true, _visDataAddress, _visPrefabAddress);
    }

    public void SetVisPrefab(string visPrefabAddress)
    {
        if (visPrefabAddress != null & visPrefabAddress.Length <= 0)
            visPrefabAddress = null;

        if (_visPrefabAddress != visPrefabAddress)
        {
            if (_ventVisualization != null)
            {
                Destroy(_ventVisualization);
                _ventVisualization = null;
            }

            _visPrefabAddress = visPrefabAddress;

            RaiseVentVisChanged();
        }

        //if (_visPrefabAddress != null && _visDataAddress != null && _visPrefabAddress.Length > 0 && _visDataAddress.Length > 0)
        //    ShowVentVisualization(true, _visDataAddress, _visPrefabAddress);

        
    }

    private void SendVentVisState()
    {
        if (!NetworkManager.IsServer)
            return;

        VRNVentVisualization ventVis = new VRNVentVisualization();
        ventVis.ToggleAction = VRNToggleAction.ToggleOn;
        ventVis.VisEnabledDM = _visEnabledDM;
        ventVis.VisEnabledPlayer = _visEnabledPlayer;
        ventVis.VisEnabledSpectator = _visEnabledSpectator;
        ventVis.VisEffectPrefab = _visPrefabAddress == null ? "" : _visPrefabAddress;
        ventVis.VisData = _visDataAddress == null ? "" : _visDataAddress; ;

        NetworkManager.SendNetMessage(VRNPacketType.SetVentVisualization, ventVis);

        //also update local state
        UpdateVentVisualization(ventVis);
    }

    public async void ShowVentVisualization(bool bShow, string visDataAddress, string visPrefabAddress)
    {
        if (!bShow)
        {
            ShowVentVisualization(false);
        }

        Debug.Log($"VentVis: Loading vent data {visPrefabAddress}");

        var data = await Addressables.LoadAssetAsync<VentVisualizationData>(visDataAddress).Task;
        if (data == null)
        {
            Debug.LogError($"Couldn't load vent vis data {visDataAddress}");
            return;
        }

        ShowVentVisualization(bShow, data, visPrefabAddress);
    }

    public async void ShowVentVisualization(bool bShow, VentVisualizationData data = null, 
        string visPrefabAddress = "VentVisEffect\\DMVisualization")
    {
        
        if (bShow)
        {

            //if (VisualizationPrefab == null)
            //    return;

            if (_visPrefabAddress != visPrefabAddress && _ventVisualization != null)
            {
                Destroy(_ventVisualization);
                _ventVisualization = null;
            }

            _visPrefabAddress = visPrefabAddress;

            if (_ventVisualization == null)
            {
                Debug.Log($"VentVis: Loading vent vis effect {visPrefabAddress}");

                var prefab = await Addressables.LoadAssetAsync<GameObject>(_visPrefabAddress).Task;
                if (prefab == null)
                {
                    Debug.LogError($"Couldn't find vent vis prefab {_visPrefabAddress}");
                    return;
                }

                if (_visPrefabAddress != visPrefabAddress)
                {
                    Debug.LogWarning($"Vent vis prefab changed while loading from {visPrefabAddress} to {_visPrefabAddress}");
                    return;
                }

                _ventVisualization = Instantiate<GameObject>(prefab);

                if (PlayerManager.CurrentPlayer != null)
                {
                    _ventVisualization.transform.SetParent(PlayerManager.CurrentPlayer.HeadTransform, false);
                    _ventVisualization.transform.localPosition = Vector3.zero;
                }
            }
        }

        if (_ventVisualization != null)
        {
            if (data != null)
            {
                var partControl = _ventVisualization.GetComponent<VentParticleControl>();
                if (partControl != null)
                {
                    partControl.VisualizationData = data;
                }

                var vfx = _ventVisualization.GetComponent<VisualEffect>();
                if (vfx != null)
                {
                    //gas data is [x:methane, y:contam, z:temp]

                    Vector3 gasSelect;

                    switch (data.VisualizationParameter)
                    {
                        case VentVisualizationParameter.Methane:
                            gasSelect = new Vector3(1, 0, 0);
                            break;

                        case VentVisualizationParameter.Temperature:
                            gasSelect = new Vector3(0, 0, 1);
                            break;

                        case VentVisualizationParameter.Contaminant:
                        default:
                            gasSelect = new Vector3(0, 1, 0);
                            break;
                    }

                    vfx.SetGradient("ContamGradient", data.ColorGradient);
                    vfx.SetVector3("GasSelect", gasSelect);
                }
            }

            _ventVisualization.SetActive(bShow);
        }

        try
        {
            Debug.Log($"Vent Visualization Changed: {bShow}");
            VentVisualizationChanged?.Invoke(bShow);
        } catch (System.Exception) { }

    }

    public void DestroyVentUI()
    {
        if (_uiParent == null)
            return;

        Destroy(_uiParent);
        _uiParent = null;
    }

    public async void ShowVentUI(bool show)
    {
        if (_ventilationControl == null ||
            _ventilationControl.VentilationProvider != VentilationProvider.MFIRE ||
            _ventilationControl.VentGraph == null)
            return;

        //if (_ventUIAirwayPrefab == null)
        //{
        //    _ventUIAirwayPrefab = await Addressables.LoadAssetAsync<GameObject>("VentUIAirwayPrefab").Task;
        //}
        //if (_ventUIJunctionPrefab == null)
        //{
        //    _ventUIJunctionPrefab = await Addressables.LoadAssetAsync<GameObject>("VentUIJunctionPrefab").Task;
        //}

        if (show)
        {
            if (_uiParent == null)
            {
                _uiParent = new GameObject("VentUIParent");

                _ventilationControl.VentGraph.CreateUIObjects(_uiParent.transform, _ventUIJunctionPrefab,
                    _ventUIAirwayPrefab, _ventUIFirePrefab, VentUIYOffset);
            }

            _uiParent.SetActive(true);
        }
        else
        {
            if (_uiParent != null)
                _uiParent.SetActive(false);
        }
    }

    public VentJunction FindClosestJunction(Vector3 worldPos)
    {
        if (_ventilationControl == null || _ventilationControl.VentGraph == null)
            return null;

        return _ventilationControl.VentGraph.FindClosestJunction(worldPos);
    }

    public VentAirway FindClosestAirway(Vector3 worldPos)
    {
        if (_ventilationControl == null || _ventilationControl.VentGraph == null)
            return null;

        return _ventilationControl.VentGraph.FindClosestAirway(worldPos);
    }

    /// <summary>
	/// Retrieve mine atmosphere information at the specified world position
	/// </summary>
	/// <param name="worldPos"></param>
	/// <returns></returns>
	public bool GetMineAtmosphere(Vector3 worldPos, out MineAtmosphere mineAtmosphere, bool useRaycast = false)
    {
        if (_ventilationControl == null)
        {
            mineAtmosphere = new MineAtmosphere();
            return false;
        }

        return _ventilationControl.GetMineAtmosphere(worldPos, out mineAtmosphere, useRaycast);

    }

    //public void RequestMineAtmosphere(Vector3 worldPos)
    //{
    //    if (NetworkManager.IsServer)
    //    {
    //        MineAtmosphere atm;
    //        if (GetMineAtmosphere(worldPos, out atm))
    //        {
    //            ReceivedMineAtmosphere?.Invoke(worldPos, atm);
    //        }
    //        else
    //        {
    //            ReceivedMineAtmosphere?.Invoke(worldPos, DefaultAtmosphere);
    //        }
    //    }
    //    else
    //    {
    //        VRNRequestMineAtmosphere msg = new VRNRequestMineAtmosphere();
    //        msg.Position = worldPos.ToVRNVector3();

    //        NetworkManager.SendNetMessage(VRNPacketType.RequestMineAtmosphere, msg);

    //    }
    //}

    public float GetSimulationTime()
    {
        if (_ventilationControl == null)
            return 0;

        return _ventilationControl.GetSimulationTime();
    }

    public void AdvanceSimulation()
    {
        if (_ventilationControl == null)
            return;

        _ventilationControl.AdvanceSimulation();
    }

    public void ResetSimulation()
    {
        if (_ventilationControl == null)
            return;

        _ventilationControl.ResetSimulation();
    }

    //protected async Task InitializeMFIRE()
    //{
    //    if (_serverControl == null)
    //    {
    //        Debug.LogError($"MFIRE: Couldn't find MFIRE Server Control");
    //        return;
    //    }

    //    _serverControl.ServerConnection.SendMFireCmd(new MFCResetSimulation());
    //    var engineState = await _serverControl.GetEngineState();

    //    if (engineState == null)
    //    {
    //        Debug.Log("MFIRE: Couldn't connect to MFIRE server");
    //        return;
    //    }
    //    else
    //    {
    //        Debug.Log($"MFIRE: Connected to MFIRE, engine state {engineState.ToString()}");
    //    }

    //    _mineNetwork.InitializeMFireNetwork();

    //    _serverControl.MFireSimulationUpdated += OnMFIREUpdated;

    //    _mfireInitialized = true;
    //}

    //private void OnMFIREUpdated() 
    //{
    //    Debug.Log("MFIRE: Received sim update");
    //    //SimulationChanged?.Invoke();
    //} 

    private void SetVentVisualizationHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //ignore if we are the server
        if (NetworkManager.IsServer)
            return;

        //var setVentVis = VRNVentVisualization.Parser.ParseDelimitedFrom(recvStream);
        var setVentVis = new VRNVentVisualization();
        reader.ReadMessage(setVentVis);

        UpdateVentVisualization(setVentVis);
    }

    private void UpdateVentVisualization(VRNVentVisualization setVentVis)
    {

        if (!setVentVis.VisEnabledDM && NetworkManager.IsServer)
        {
            //we are DM & DM visualization is disabled
            ShowVentVisualization(false);
            return;
        }
        else if (!setVentVis.VisEnabledSpectator && PlayerManager.CurrentPlayer == null)
        {
            //we are a specatator & spectator visualization is disabled
            ShowVentVisualization(false);
            return;
        }
        else if (!setVentVis.VisEnabledPlayer && !NetworkManager.IsServer && PlayerManager.CurrentPlayer != null)
        {
            //we are a player & player visualization is disabled
            ShowVentVisualization(false);
            return;
        }



        switch (setVentVis.ToggleAction)
        {
            case VRNToggleAction.ToggleOn:

                if (setVentVis.VisEffectPrefab != null && setVentVis.VisEffectPrefab.Length > 0)
                    SetVisPrefab(setVentVis.VisEffectPrefab);
                if (setVentVis.VisData != null && setVentVis.VisData.Length > 0)
                    SetVisData(setVentVis.VisData);

                if (_visPrefabAddress != null && _visDataAddress != null && 
                    _visPrefabAddress.Length > 0 && _visDataAddress.Length > 0)
                    ShowVentVisualization(true, _visDataAddress, _visPrefabAddress);
                 
                //if (_visPrefabAddress != null && _visDataAddress != null)
                //    ShowVentVisualization(true);

                break;

            case VRNToggleAction.ToggleOff:
                ShowVentVisualization(false);
                break;

            default:
            case VRNToggleAction.ToggleSwitch:
                ShowVentVisualization(!IsVentVisualizationVisible);
                break;
        }

    }

    private void SendVentGraphHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var graph = VRNVentGraph.Parser.ParseDelimitedFrom(recvStream);
        var graph = new VRNVentGraph();
        reader.ReadMessage(graph);

        if (_ventilationControl != null)
        {
            _ventilationControl.LoadVentGraphData(graph);
        }
    }

    public void ClearVentGraph()
    {
        if (_ventilationControl == null)
            return;

        _ventilationControl.ClearVentGraph();
    }

    public void LoadVentilationState(SessionVentilationState ventState)
    {
        if (ventState == null || ventState.VentGraph == null)
            return;

        if (_ventilationControl != null)
        {
            _ventilationControl.LoadVentGraphData(ventState.VentGraph);
        }
    }

    //private void SendMineAtmosphereHandler(VRNHeader header, CodedInputStream reader, int clientID)
    //{
    //    //var req = VRNSendMineAtmosphere.Parser.ParseDelimitedFrom(recvStream);
    //    var req = new VRNSendMineAtmosphere();
    //    reader.ReadMessage(req);
    //    var worldPos = req.Position.ToVector3();
    //    MineAtmosphere atm = new MineAtmosphere();
    //    atm.Oxygen = req.Oxygen;
    //    atm.CarbonMonoxide = req.CarbonMonoxide;
    //    atm.HydrogenSulfide = req.HydrogenSulfide;
    //    atm.Methane = req.Methane;

    //    //Debug.Log($"Received mine atmosphere {worldPos.ToString()}");

    //    ReceivedMineAtmosphere?.Invoke(worldPos, atm);
    //}

    //private void RequestMineAtmosphereHandler(VRNHeader header, CodedInputStream reader, int clientID)
    //{
    //    if (!NetworkManager.IsServer)
    //        return;

    //    //var req = VRNRequestMineAtmosphere.Parser.ParseDelimitedFrom(recvStream);
    //    var req = new VRNRequestMineAtmosphere();
    //    reader.ReadMessage(req);
    //    var worldPos = req.Position.ToVector3();

    //    MineAtmosphere atm;
    //    if (!GetMineAtmosphere(worldPos, out atm))
    //        atm = DefaultAtmosphere;

    //    VRNSendMineAtmosphere msg = new VRNSendMineAtmosphere();
    //    msg.Position = req.Position;
    //    msg.Oxygen = atm.Oxygen;
    //    msg.CarbonMonoxide = atm.CarbonMonoxide;
    //    msg.Methane = atm.Methane;
    //    msg.HydrogenSulfide = atm.HydrogenSulfide;

    //    //Debug.Log($"Sending mine atmosphere {msg.Position.ToVector3()}");

    //    //NetworkManager.SendNetMessage(VRNPacketType.SendMineAtmosphere, msg, peer: fromPeer);
    //    NetworkManager.SendNetMessage(VRNPacketType.SendMineAtmosphere, msg);
    //}

    void VentilationUpdateHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        
    }

}
