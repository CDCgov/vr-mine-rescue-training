using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

//public enum MineRescuerXRRole
//{
//    Captain,
//    GasMan,
//    MapMan,
//    SecondGasMan,
//    TailCaptain,
//    VRTech,
//    Default
//}

public class ConfigureMinerXRRig : MonoBehaviour
{
    public PlayerDominantHand Handedness = PlayerDominantHand.RightHanded;
    //public MineRescuerXRRole Role = MineRescuerXRRole.Default;

    public PlayerManager PlayerManager;
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public LoadableAssetManager LoadableAssetManager;

    public MinerColorChanger ColorChanger;
    public PlayerColorManager PlayerColorManager;
    public PlayerRoleManager PlayerRoleManager;
    public SocketManager SocketManager;
    public SystemManager SystemManager;
    //public SimpleLocomotion SimpleLocomotion;

    public CustomXRInteractor LeftInteractor;
    public CustomXRInteractor RightInteractor;

    public GameObject LeftHandPencil;
    public GameObject RightHandPencil;

    public Camera HeadCamera;

    /*
	//Map
	public GameObject SpawnMapPrefab;

	//Equipment
	public GameObject GasMeterPrefab;
	public GameObject RadioPrefab;
	public GameObject ChalkPrefab;
	public GameObject SoundingStickPrefab;
    public GameObject SentinelPrefab;
	
	//New Socket Transforms
    /*
	public Transform ChalkSocketTransform;
	public Transform SoundStickSocketTransform;
	public Transform RadioSocketTransform;
	public Transform GasMeterSocketTransform;
    public Transform LeftChalkSocketTransform;
    public Transform RightSoundStickSocketTransform;
    public Transform SentinelSocketTransform;

    public GameObject ChalkPouchLeftObject;
    public GameObject ChalkPouchRightObject;
    */
#if DISSONANCE
    public DissonanceTest DissonanceSoundToggle;
#endif
    public XRHapticShake XRHapticShake;

    private List<Guid> _equippedItems;
    private VRNPlayerRole _assignedRole;
    private VRNPlayerRole _configuredRole = VRNPlayerRole.UnknownRole;
    private bool _roleConfigurationInProgress = false;

    //private int PlayerID = -1;
    //private int ClientID = -1;
    private bool _initialized = false;
    private bool _destroyed = false;
    private bool _equipmentSpawned = false;


    //private PencilSnapBehavior _pencilSnapBehavior;
    //private GameObject _pencilObj;
    //private PencilSnapBehavior _rhPencil;
    //private PencilSnapBehavior _lhPencil;
    private bool _debugMode = false;
    private PlatformType _platform;
    //private VRDebugUIController VRDebugUIController;

    private PlayerEquipmentSocket[] _playerEquipmentSockets;

    private void Awake()
    {
        //if (SimpleLocomotion == null)
        //{
        //    SimpleLocomotion = GetComponent<SimpleLocomotion>();
        //}
        //VRDebugUIController = GameObject.FindObjectOfType<VRDebugUIController>();
        _equippedItems = new List<Guid>();
    }
    private IEnumerator Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();
        if (PlayerRoleManager == null)
            PlayerRoleManager = PlayerRoleManager.GetDefault();
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        _playerEquipmentSockets = GetComponentsInChildren<PlayerEquipmentSocket>();

        if (XRHapticShake == null)
        {
            XRHapticShake = gameObject.GetComponent<XRHapticShake>();
        }
        PlayerManager.PlayerIDAssigned += OnPlayerIDAssigned;

        //NetworkManager.PlayerRoleChanged += ConfigurePlayerRole;

        SceneManager.activeSceneChanged += OnSceneChanged;
        PlayerManager.PlayerRoleAssigned += OnPlayerRoleAssigned;
        //NetworkManager.ClientIDAssigned += OnClientIDAssigned;
        NetworkManager.SceneIDChanged += OnSceneIDChanged;


        yield return new WaitForSecondsRealtime(0.5f);

        if (PlayerManager.CurrentPlayer.PlayerObject != null)
        {
            Debug.LogError($"ConfigureMinerXR: CurrentPlayer PlayerObject already exists!");
            Destroy(PlayerManager.CurrentPlayer.PlayerObject);
        }
        PlayerManager.CurrentPlayer.PlayerObject = this.gameObject;



        //      var playerInfo = GetComponent<PlayerInfo>();
        //if (playerInfo == null)
        //{
        //	Debug.LogError("ConfigureMinerXRRig: couldn't find PlayerInfo component");
        //	enabled = false;
        //	return;
        //}

        Debug.Log($"ConfigureMinerXR: starting with player ID {PlayerManager.CurrentPlayer.PlayerID} and role {PlayerManager.CurrentPlayer.PlayerRole.ToString()}");

        _initialized = true;

        OnPlayerIDAssigned(PlayerManager.CurrentPlayer.PlayerID);
        Handedness = PlayerManager.CurrentPlayer.PlayerDominantHand;

        if (NetworkManager.ActiveSceneID >= 0)
            ConfigurePlayerRole(PlayerManager.CurrentPlayer.PlayerRole);

        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);

#if DISSONANCE
        if (DissonanceSoundToggle == null)
        {
            DissonanceSoundToggle = GameObject.FindObjectOfType<DissonanceTest>();
        }
#endif
        
        
    }

    private void OnDestroy()
    {
        _destroyed = true;
        Debug.Log($"ConfigureMinerXRRig: OnDestroy");
        if (PlayerManager != null)
        {
            PlayerManager.UnregisterPlayerMessageHandler(OnPlayerMessage);
            PlayerManager.PlayerRoleAssigned -= OnPlayerRoleAssigned;
            PlayerManager.PlayerIDAssigned -= OnPlayerIDAssigned;
        }

        SceneManager.activeSceneChanged -= OnSceneChanged;

        if (NetworkManager != null)
        {
            //NetworkManager.ClientIDAssigned -= OnClientIDAssigned;
            NetworkManager.SceneIDChanged -= OnSceneIDChanged;
        }

        ClearEquippedItems();
    }

    private void OnSceneIDChanged(int obj)
    {
        _configuredRole = VRNPlayerRole.UnknownRole;
        ConfigurePlayerRole(_assignedRole);
    }

    void ConfigureSockets()
    {
        if (PlayerManager == null || PlayerManager.CurrentPlayer == null || PlayerManager.CurrentPlayer.PlayerID < 0)
            return;

        CustomXRSocket[] sockets = GetComponentsInChildren<CustomXRSocket>();
        foreach (CustomXRSocket socket in sockets)
        {
            //string newID = $"{PlayerManager.CurrentPlayer.PlayerID}_{socket.SocketID}";
            string newID = $"{PlayerManager.CurrentPlayer.PlayerID}_{socket.name}";
            //SocketManager.UnregisterSocket(socket.SocketID);
            //socket.SocketID = newID;
            //socket.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
            //socket.RegisterWithSocketManager();
            //socket.AssignSocketName(socket.name + PlayerManager.CurrentPlayer.PlayerID.ToString());

            //socket.name = socket.SocketID;
            socket.SetSocketID(newID);
        }
    }



    private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        Debug.Log(messageType + " message type? " + msg.PlayerID + " msg playerID, " + PlayerManager.CurrentPlayer.PlayerID + " playerID");
        switch (messageType)
        {
            case VRNPlayerMessageType.PmSetDominantHand:
                if (msg.PlayerMessageDataCase == VRNPlayerMessage.PlayerMessageDataOneofCase.IntData)
                {
                    PlayerDominantHand dominantHand = (PlayerDominantHand)msg.IntData;
                    Debug.Log($"ConfigureMinerXRRig: PlayerMessage - Setting dominant hand to {dominantHand}");
                    SetPlayerHandedness(dominantHand);
                }
                break;
            case VRNPlayerMessageType.PmForceLinkLine:
                if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
                {
                    return;
                }
                if (msg.BoolData)
                {
                    //ForcePlayerToLinkline();
                }
                else
                {
                    //ForcePLayerDisconnectLinkline();
                }
                break;

#if DISSONANCE
            case VRNPlayerMessageType.PmDeafenPlayer:
                if (DissonanceSoundToggle == null)
                {
                    DissonanceSoundToggle = GameObject.FindObjectOfType<DissonanceTest>();
                }
                if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
                {
                    return;
                }
                if (msg.BoolData)
                {
                    DissonanceSoundToggle.DM_DeafenPlayer();
                }
                else
                {
                    DissonanceSoundToggle.DM_UnDeafenPlayer();
                }
                break;
            case VRNPlayerMessageType.PmMutePlayer:
                if (DissonanceSoundToggle == null)
                {
                    DissonanceSoundToggle = GameObject.FindObjectOfType<DissonanceTest>();
                }
                if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
                {
                    return;
                }
                if (msg.BoolData)
                {
                    DissonanceSoundToggle.DM_MutePlayer();
                }
                else
                {
                    DissonanceSoundToggle.DM_UnMutePlayer();
                }
                break;
#endif
        }
    }

    private void OnPlayerRoleAssigned(VRNAssignPlayerRole msg)
    {
        Debug.Log($"ConfigureMinerXR: received role assign for {msg.PlayerID} role {msg.Role.ToString()}, our ID {PlayerManager.CurrentPlayer.PlayerID}");
        if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
            return;

        Handedness = msg.LeftHandDominant ? PlayerDominantHand.LeftHanded : PlayerDominantHand.RightHanded;

        _assignedRole = msg.Role;
        _equipmentSpawned = false; //force respawn
        //await Task.Delay(750);

        if (NetworkManager.ActiveSceneID >= 0)
            ConfigurePlayerRole(msg.Role);
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        //ClearEquippedItems();
        ConfigurePlayerRole(_assignedRole);
    }

    private void OnPlayerIDAssigned(int playerID)
    {
        //Role = MineRescuerXRRole.Default;
        //ColorChanger.MinerColor = PlayerColorManager.GetPlayerColor(playID);
        //ColorChanger.UpdateMiner();
        //Debug.Log(PlayerID + ", " + PlayerColorManager.GetPlayerColor(playID)); //PlayerColorManager.GetPlayerColor(player.PlayerID);

        Debug.Log($"ConfigureMinerXR: Assigned player ID {playerID}");

        //PlayerID = playerID;
        ConfigureSockets();
    }

    private void EnableHandedSockets()
    {
        foreach (var socket in _playerEquipmentSockets)
        {
            bool enable = true;
            if (Handedness == PlayerDominantHand.LeftHanded && !socket.LeftHanded ||
                Handedness == PlayerDominantHand.RightHanded && !socket.RightHanded)
                enable = false;

            socket.gameObject.SetActive(enable);
            if (socket.AssociatedVisual != null && socket.AssociatedVisual.gameObject != null)
                socket.AssociatedVisual.gameObject.SetActive(enable);
        }
    }

    private Transform GetEquipSpawnTransform(VRNPlayerEquipmentType equip)
    {
        foreach (var socket in _playerEquipmentSockets)
        {
            if (socket == null || socket.gameObject == null)
                continue;

            if (Handedness == PlayerDominantHand.LeftHanded && !socket.LeftHanded ||
                Handedness == PlayerDominantHand.RightHanded && !socket.RightHanded)
                continue;

            if (socket.EquipmentType == equip)
                return socket.transform;
        }

        Debug.LogError($"ConfigureMinerXR: Couldn't find spawn point for {equip} on {gameObject.name}");
        return null;
    }

    private async Task SpawnMap(CustomXRInteractor mapHand, CustomXRInteractor pencilHand)
    {
        //ControllerBehaviors pencilHandController = RightInteractor.GetComponent<ControllerBehaviors>();
        //pencilHandController.HideController();
        //pencilHandController.PencilActive = true;

        //if (_pencilObj != null)
        //    _pencilObj.SetActive(false);

        //var pencilXform = pencilHand.transform.Find("PencilObject");
        //if (pencilXform == null)
        //{
        //    Debug.LogError($"Interactor {pencilHand.name} missing PencilObject");
        //    return;
        //}
        //_pencilObj = pencilXform.gameObject;
        //_pencilObj.SetActive(true);
        //_pencilSnapBehavior = _pencilObj.GetComponent<PencilSnapBehavior>();

        var pencilSnap = pencilHand.transform.GetComponentInChildren<PencilSnapBehavior>(true);
        if (pencilSnap == null)
        {
            Debug.LogError($"Interactor {pencilHand.name} missing PencilObject");
            return;
        }

        pencilSnap.gameObject.SetActive(true);
        pencilHand.AlwaysShowLineRenderer = true;
        ControllerBehaviors mapHandController = mapHand.GetComponent<ControllerBehaviors>();
        mapHandController.HideController();
        mapHand.AllowGrip = false;

        //ControllerBehaviors mapHandController = LeftInteractor.GetComponent<ControllerBehaviors>();
        //mapHandController.HideController();
        //mapHandController.DisableInteractor();
        Vector3 rotationVal;
        if(Handedness == PlayerDominantHand.RightHanded)
        {
            rotationVal = new Vector3(-90, 0, -90);
            //ControllerBehaviors leftmapHandController = LeftInteractor.GetComponent<ControllerBehaviors>();
            //leftmapHandController.HideController();
            //leftmapHandController.DisableInteractor();
        }
        else
        {
            rotationVal = new Vector3(-90, 0, 90);
            //ControllerBehaviors rightapHandController = RightInteractor.GetComponent<ControllerBehaviors>();
            //rightapHandController.HideController();
            //rightapHandController.DisableInteractor();
        }

        await SpawnMap(mapHand, pencilHand, new Vector3(0.015f, 0, 0.05f), rotationVal, pencilSnap);
    }

    private async void ConfigurePlayerRole(VRNPlayerRole role)
    {
        Debug.Log($"ConfigureMinerXR: ConfigurePlayerRole called - {role}");

        _assignedRole = role;
        var sceneID = NetworkManager.ActiveSceneID;

        //don't start the role configuration process if one is already running 
        //Note: further cleanup to more quickly terminate a running role configuration should be added
        if (_roleConfigurationInProgress)
            return;

        //don't respawn the same role's equipment
        if (_assignedRole == _configuredRole && _equipmentSpawned)
            return;
        
        _configuredRole = role;

        _roleConfigurationInProgress = true;

        try
        {
            Debug.Log($"ConfigureMinerXR: configuring for role {role.ToString()}");
            await Task.Delay(1000);

            if (!_initialized)
                return;

            if (ColorChanger != null)
            {
                ColorChanger.MinerColor = PlayerColorManager.GetPlayerColor(role);
                ColorChanger.UpdateMiner();
            }
            ClearEquippedItems();
            LeftInteractor.AlwaysShowLineRenderer = false;
            RightInteractor.AlwaysShowLineRenderer = false;
            //EnableHands();

            if (_configuredRole == VRNPlayerRole.UnknownRole)
            {
                _equipmentSpawned = true; //unknown role has no equipment
                return;
            }

            var roleData = PlayerRoleManager.GetPlayerRoleData(role);
            if (roleData == null)
                return;

            Debug.Log($"ConfigMiner: Spawning {roleData.Equipment.Count} items for {role}");

            //var sceneConfig = MineSceneConfiguration.CurrentScene;
            var settings = ScenarioSaveLoad.Settings;
            var minerProfile = LoadableAssetManager.FindMinerProfile(settings.MinerProfileID);

            foreach (var equip in roleData.Equipment)
            {
                if (sceneID != NetworkManager.ActiveSceneID)
                    throw new Exception("Scene ID changed during ConfigurePlayerRole equipment spawning");

                //if (sceneConfig != null && sceneConfig.DisabledEquipmentList != null)
                //{
                if (minerProfile.DisabledEquipment != null && minerProfile.DisabledEquipment.Contains(equip))
                    continue;
                //}

                try
                {
                    //special processing for map, for now
                    if (equip == VRNPlayerEquipmentType.Map)
                    {
                        if (Handedness == PlayerDominantHand.RightHanded)
                        {
                            await SpawnMap(LeftInteractor, RightInteractor);                            
                        }
                        else
                        {
                            await SpawnMap(RightInteractor, LeftInteractor);
                        }
                        continue;
                    }

                    var xform = GetEquipSpawnTransform(equip);
                    if (xform == null)
                        continue;


                    string equipAsset = PlayerRoleManager.GetEquipmentAsset(equip);
                    Debug.Log($"ConfigMiner: Spawning equipment {equip} asset:{equipAsset}");
                    var result = await SpawnEquipment(equipAsset, xform);
                    if (!result)
                        throw new Exception($"Failed to spawn equipment {equip}");

                    
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error spawning equipment {equip.ToString()} - {ex.Message} :: {ex.StackTrace}");
                }

                if (_destroyed)
                    return;
            }

            if (minerProfile.AddedEquipment != null && minerProfile.AddedEquipment.Count > 0 && !roleData.BlockAdditionalEquipment)
            {
                foreach (var equip in minerProfile.AddedEquipment)
                {
                    var xform = GetEquipSpawnTransform(equip);
                    if (xform == null)
                        continue;
                    string equipAsset = PlayerRoleManager.GetEquipmentAsset(equip);
                    Debug.Log($"ConfigMiner: Spawning equipment {equip} asset:{equipAsset}");
                    var result = await SpawnEquipment(equipAsset, xform);
                    if (!result)
                        throw new Exception($"Failed to spawn equipment {equip}");
                }
            }

            _equipmentSpawned = true;
            Debug.Log($"ConfigureMinerXR: Done spawning equipment");
        }
        catch (System.Exception ex)
        {
            Debug.Log($"ConfigureMinerXR: Error configuring player role: {ex.Message} {ex.StackTrace}");
            await Task.Delay(1000);
            _configuredRole = VRNPlayerRole.UnknownRole;
        }
        finally
        {
            _roleConfigurationInProgress = false;
        }

    }

    private void EnableHands()
    {
        LeftHandPencil.SetActive(false);
        RightHandPencil.SetActive(false);
        ControllerBehaviors lhControlBehavior = LeftInteractor.GetComponent<ControllerBehaviors>();
        ControllerBehaviors rhControlBehavior = RightInteractor.GetComponent<ControllerBehaviors>();
        //if (!lhControlBehavior.InteractorActive)
        //{
        //    lhControlBehavior.EnableInteractor();
        //}
        //if (!rhControlBehavior.InteractorActive)
        //{
        //    rhControlBehavior.EnableInteractor();
        //}

        LeftInteractor.AllowGrip = true;
        RightInteractor.AllowGrip = true;

        lhControlBehavior.PencilActive = false;
        lhControlBehavior.ShowController();
        rhControlBehavior.PencilActive = false;
        rhControlBehavior.ShowController();
        //if(_rhPencil != null)
        //{
        //    _rhPencil.OnReturnPencil();
        //}
        //if (_lhPencil != null)
        //{
        //    _lhPencil.OnReturnPencil();
        //}

        //if (_pencilSnapBehavior != null)
        //    _pencilSnapBehavior.OnReturnPencil();


    }

    private async Task SpawnMap(CustomXRInteractor mapHand, CustomXRInteractor pencilHand, 
        Vector3 localPositionVector, Vector3 eulerAngleRotation, 
        PencilSnapBehavior pencilSnap)
    {

        var mapGuid = Guid.NewGuid();
        _equippedItems.Add(mapGuid);
        var mapAsset = PlayerRoleManager.GetEquipmentAsset(VRNPlayerEquipmentType.Map);
        var mapNetObj = await NetworkedObjectManager.SpawnObject(mapAsset, mapGuid, Vector3.zero, Quaternion.identity, ownerExclusive: true);

        //GameObject map = Instantiate(SpawnMapPrefab, hand);
        var map = mapNetObj.gameObject;
        map.transform.SetParent(mapHand.transform);
        map.transform.localPosition = localPositionVector;
        map.transform.localEulerAngles = eulerAngleRotation;

        MappingVisualHandler mapVis = map.GetComponent<MappingVisualHandler>();
        mapVis.Controller = pencilSnap.transform;
        mapVis.SetUICanvasCamera(HeadCamera);
        mapVis.PencilBehavior = pencilSnap;
        pencilSnap.SnapPointTransform = mapVis.PencilSocketTransform;
        mapVis.SetMapHandedness(Handedness);

        CustomXRInteractable mapInteractable = map.GetComponent<CustomXRInteractable>();
        mapInteractable.IsGrabbable = true;
        ControllerBehaviors mapControlBehavior;
        if(mapHand.TryGetComponent<ControllerBehaviors>(out mapControlBehavior))
        {
            mapControlBehavior.HideController();
        }

        //StartCoroutine(DelayedEquip(mapHand, mapInteractable, map, mapNetObj));
        mapHand.SetHeldInteractable(mapInteractable);

        //mapHand.SetHeldInteractable(mapInteractable);
        //AdjustMapBoard adjust = map.GetComponent<AdjustMapBoard>();
        //adjust.OnPickedUp(mapHand.transform);

        //DO AN OVERRIDE HERE WHERE IT FORCES A GRAB OF THE MAP -BDM

        //if(Handedness == PlayerDominantHand.RightHanded)
        //{
        //    CustomXRInteractor linteractor = LeftInteractor.GetComponent<CustomXRInteractor>();
        //    if (linteractor != null)
        //    {
        //        //linteractor.HeldObject = map.GetComponent<CustomXRInteractable>();
        //        linteractor.SetHeldInteractable(map.GetComponent<CustomXRInteractable>());
        //        //mapInteractable.InteractorTransform = RightInteractor.transform;
        //    }
        //}
        //else
        //{
        //    CustomXRInteractor rinteractor = RightInteractor.GetComponent<CustomXRInteractor>();
        //    if (rinteractor != null)
        //    {
        //        //rinteractor.HeldObject = map.GetComponent<CustomXRInteractable>();
        //        rinteractor.SetHeldInteractable(map.GetComponent<CustomXRInteractable>());
        //        //mapInteractable.InteractorTransform = LeftInteractor.transform;
        //    }
        //}

        //Uncomment this one too if this didnt' work
        //_equippedItems.Add(mapNetObj);
    }

    ///// <summary>
    ///// This is a delay to address a potential race condition affecting the map board spawn.
    ///// </summary>
    ///// <param name="mapHand"></param>
    ///// <param name="mapInteractable"></param>
    ///// <param name="map"></param>
    ///// <param name="mapNetObj"></param>
    ///// <returns></returns>
    //IEnumerator DelayedEquip(CustomXRInteractor mapHand, CustomXRInteractable mapInteractable, GameObject map, NetworkedObject mapNetObj)
    //{
    //    yield return 0;
    //    mapHand.SetHeldInteractable(mapInteractable);
    //    //AdjustMapBoard adjust = map.GetComponent<AdjustMapBoard>();
    //    //adjust.OnPickedUp(mapHand.transform);
    //    //_equippedItems.Add(mapNetObj);
    //}

    private async Task<bool> SpawnEquipment(string assetRef, Transform socket)
    {
        
        if (PlayerManager.CurrentPlayer == null || PlayerManager.CurrentPlayer.PlayerID < 0)
            return false;

        CustomXRSocket xrSock = socket.GetComponent<CustomXRSocket>();
        if (xrSock != null)
        {
            xrSock.EnableObjectScan = false;
        }

        var equipGuid = Guid.NewGuid();
        _equippedItems.Add(equipGuid);

        var netObj = await NetworkedObjectManager.SpawnObject(assetRef, equipGuid,
            Vector3.zero, Quaternion.identity, spawnActivated: true, ownerExclusive: true);
        if (_destroyed || netObj == null || gameObject == null)
        {
            NetworkedObjectManager.DestroyObject(equipGuid);
            _equippedItems.Remove(equipGuid);
            return false;
        }

        Debug.Log($"ConfigMiner: Equipment spawned asset: {assetRef} id: {netObj.uniqueID}");

        //_equippedItems.Add(netObj);

        //GameObject spawn = Instantiate(equipment, socket);
        GameObject spawn = netObj.gameObject;


        spawn.transform.localPosition = Vector3.zero;
        spawn.transform.localEulerAngles = Vector3.zero;


        Rigidbody rb = spawn.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        CustomXRInteractable grab = spawn.GetComponent<CustomXRInteractable>();
        grab.IsGrabbable = true;
        if (xrSock != null && grab != null)
        {
            SocketManager.RequestSocketAssignment(xrSock.SocketID, grab.GetComponent<NetworkedObject>().uniqueID);
        }
        //NetSyncGrabState _grabState = spawn.GetComponent<NetSyncGrabState>();
        //if (_grabState != null)
        //{
        //    _grabState.Grabbed = false;
        //}

        var playerIDifaces = spawn.GetComponents<IRequiresPlayerID>();
        if (playerIDifaces != null)
        {
            foreach (var playerIDiface in playerIDifaces)
            {
                playerIDiface.SetPlayerID(PlayerManager.CurrentPlayer.PlayerID);
            }
        }

//        if (isCommsObject)
//        {
//            grab.onSelectEnter.AddListener(OnGrabRadioCallback);
//            grab.onSelectExit.AddListener(OnDropRadioCallback);

//#if DISSONANCE
//            if (DissonanceSoundToggle != null)
//                DissonanceSoundToggle.RadioInteractable = grab;
//#endif
//        }

        return true;
    }

//    IEnumerator SocketItemCall(CustomXRSocket sock, CustomXRInteractable item)
//    {
//        yield return new WaitForSeconds(1);
//        sock.RequestSocketItem(item);
//        item.CurrentOwner = sock;
//        sock.SocketedInteractable = item;
//    }

//    void OnGrabRadioCallback()
//    {
//#if DISSONANCE
//        if (DissonanceSoundToggle != null)
//            DissonanceSoundToggle.UnDeafenPlayer();
//#endif
//    }

//    void OnDropRadioCallback()
//    {
//#if DISSONANCE
//        if (DissonanceSoundToggle != null)
//            DissonanceSoundToggle.DeafenPlayer();
//#endif
//    }


    public void ClearEquippedItems()
    {
        if (gameObject != null)
            EnableHands();

        if (_equippedItems == null)
            return;

        foreach (var item in _equippedItems)
        {
            if (item == null)
                continue;

            NetworkedObjectManager.DestroyObject(item);
            //Destroy(item);
        }
        _equippedItems.Clear();
    }

    private void SetGrabButton(XRController controller, InputHelpers.Button button)
    {
        controller.selectUsage = button;
    }
    private void SetActivateButton(XRController controller, InputHelpers.Button button)
    {
        controller.activateUsage = button;
    }


    private void Update()
    {
        if (!_initialized)
            return;

        bool needsConfigured = _configuredRole != _assignedRole || !_equipmentSpawned;
        if (needsConfigured && !_roleConfigurationInProgress && NetworkManager.ActiveSceneID >= 0)
            ConfigurePlayerRole(_assignedRole);

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            //SetMinerRole(MineRescuerXRRole.Captain);
            //AssignPlayer(1);
            PlayerManager.CurrentPlayer.PlayerID = 1;
            Debug.Log($"Keyboard shortcut hit: {PlayerManager.CurrentPlayer.PlayerID}");
            ConfigurePlayerRole(VRNPlayerRole.Captain);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            //SetMinerRole(MineRescuerXRRole.GasMan);
            //AssignPlayer(2);
            ConfigurePlayerRole(VRNPlayerRole.GasMan);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            //SetMinerRole(MineRescuerXRRole.MapMan);
            //AssignPlayer(3);
            ConfigurePlayerRole(VRNPlayerRole.MapMan);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            //SetMinerRole(MineRescuerXRRole.SecondGasMan);
            //AssignPlayer(4);
            ConfigurePlayerRole(VRNPlayerRole.SecondGasMan);
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            //SetMinerRole(MineRescuerXRRole.TailCaptain);
            //AssignPlayer(5);
            ConfigurePlayerRole(VRNPlayerRole.TailCaptain);
        }
        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            //SetMinerRole(MineRescuerXRRole.VRTech);
            //AssignPlayer(0);
            ConfigurePlayerRole(VRNPlayerRole.Vrtech);
        }
       
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            switch (Handedness)
            {
                case PlayerDominantHand.RightHanded:
                    SetPlayerHandedness(PlayerDominantHand.LeftHanded);
                    break;
                case PlayerDominantHand.LeftHanded:
                    SetPlayerHandedness(PlayerDominantHand.RightHanded);
                    break;
                default:
                    SetPlayerHandedness(PlayerDominantHand.RightHanded);
                    break;
            }
        }

        //if (Input.GetKeyUp(KeyCode.L))
        //{
        //    ForcePlayerToLinkline();
        //}
        //if (Input.GetKeyUp(KeyCode.O))
        //{
        //    ForcePLayerDisconnectLinkline();
        //}
    }

    public void SetPlayerHandedness(PlayerDominantHand playerDominantHand)
    {
        Handedness = playerDominantHand;
        PlayerManager.CurrentPlayer.PlayerDominantHand = playerDominantHand;
        Debug.Log("Player handedness set to: " + Handedness.ToString());
        _equipmentSpawned = false;
        ConfigurePlayerRole(_assignedRole);
    }

    //public bool DebugMode
    //{
    //    get => _debugMode;
    //    set
    //    {
    //        _debugMode = value;
    //        Debug.Log("Setting debug mode: " + _debugMode);
    //        if (SimpleLocomotion != null)
    //        {
    //            SimpleLocomotion.UseJoystick = _debugMode;
    //            Debug.Log("Setting simple locomotion: " + _debugMode);
    //        }            
    //        if(VRDebugUIController != null)
    //        {
    //            VRDebugUIController.DebugMode = _debugMode;
    //            Debug.Log("Setting VR Debug UI: " + _debugMode);
    //        }
    //    }
    //}

    //public void ForcePlayerToLinkline()
    //{
    //    float SearchRadius = 2;
    //    Collider[] cols = Physics.OverlapSphere(transform.position, SearchRadius);
    //    foreach (Collider col in cols)
    //    {
    //        LinkLineObject llobj = col.GetComponent<LinkLineObject>();
    //        if (llobj == null)
    //        {
    //            continue;
    //        }

    //        if (llobj.LinkLineInteract.CurrentOwner != null)
    //        {
    //            return;
    //        }

    //        CustomXRSocket[] sockets = gameObject.GetComponentsInChildren<CustomXRSocket>();
    //        CustomXRSocket target = null;
    //        float distance = Mathf.Infinity;
    //        foreach (CustomXRSocket sock in sockets)
    //        {
    //            if (!sock.EnableObjectScan)
    //            {
    //                continue;
    //            }
    //            if (sock.SocketedInteractable == null && llobj.tag == sock.tag)
    //            {
    //                float distCompare = Vector3.Distance(transform.position, sock.transform.position);
    //                if (distCompare < distance)
    //                {
    //                    target = sock;
    //                    distance = distCompare;
    //                }
    //            }
    //        }

    //        if (target == null)
    //        {
    //            Debug.Log("There is no nearby link line point");
    //        }
    //        CustomXRInteractable customXRInteractable = llobj.GetComponent<CustomXRInteractable>();
    //        customXRInteractable.ChangeOwnership(target, false);
    //        NetworkedObject netObj = llobj.GetComponent<NetworkedObject>();
    //        netObj.RequestOwnership();
    //        target.SocketedInteractable = customXRInteractable;
    //        llobj.transform.parent = target.transform;
    //        llobj.transform.localPosition = Vector3.zero;
    //        llobj.transform.localRotation = Quaternion.identity;
    //        llobj.LinkLineInteract.CurrentOwner = target;
    //        llobj.OnLinkLineActivated();

    //        NetSyncGrabInteractable grab = llobj.GetComponent<NetSyncGrabInteractable>();
    //        grab.LockPosition = true;
    //        Debug.Log("Linked to: " + target.name);
    //        break;
    //    }
    //}

    //public void ForcePLayerDisconnectLinkline()
    //{
    //    Debug.Log("Detach hotkey hit");
    //    CustomXRSocket[] sockets = gameObject.GetComponentsInChildren<CustomXRSocket>();
    //    foreach (CustomXRSocket sock in sockets)
    //    {
    //        if (sock.SocketedInteractable == null)
    //        {
    //            continue;
    //        }
    //        LinkLineObject llobj = sock.SocketedInteractable.GetComponent<LinkLineObject>();
    //        if (llobj == null)
    //        {
    //            continue;
    //        }
    //        CustomXRInteractable customXRInteractable = llobj.GetComponent<CustomXRInteractable>();
    //        customXRInteractable.ChangeOwnership(null, false);
    //        llobj.LinkLineInteract.ReturnToParent();
    //        llobj.ReturnToStart();
    //        llobj.OnLinkLineDeactivated();
    //        NetSyncGrabInteractable grab = llobj.GetComponent<NetSyncGrabInteractable>();
    //        grab.LockPosition = false;
    //        Debug.Log("Forced drop of link line");
    //    }
    //}
}
