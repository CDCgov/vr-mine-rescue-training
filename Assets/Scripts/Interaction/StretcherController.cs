using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StretcherController : MonoBehaviour, ISelectableObjectAction, INetSync
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public SceneLoadManager SceneLoadManager;

    public Bounds SpawnAreaBounds;
    public GameObject UnconciousNPC;
    public GameObject NPCBGFour;
    public GameObject SafetyGlasses;
    public GameObject CapLamp;
    public List<TextTexture> NameProjectors;

    public List<AssetReference> EquipmentSpawnList;
    public Quaternion EquipmentStartRotation;
    public NPCStretcherBreathing NPCStretcherBreathing;
    public Animator StretcherAnimator;
    public NPCTriggerDeath NPCTriggerDeath;

    private NetworkedObject _netObj;
    private List<CustomXRSocket> _sockets;
    private bool _npcActive = false;
    private bool _bg4Active = false;
    private bool _isAlive = true;
    private string _name = "";
    private bool _needsUpdate = true;
    private ResetObjectsAction _resetObjects;
    private VRNStretcherData _stretcherData;
    private VRNBoolData _boolData;
    private VRNTextMessage _textMessage;
    private PlayerRepresentation _cachedPlayer;

    public string SelectableActionName
    {
        get
        {
            return "Turn off NPC";
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.Period))
    //    {
    //        SetNPCActiveState(!_npcActive);
    //    }
    //    if (Input.GetKeyUp(KeyCode.Comma))
    //    {
    //        SetBG4ActiveState(!_bg4Active);
    //    }
    //}

    void Awake()
    {
        _stretcherData = new VRNStretcherData();
        _boolData = new VRNBoolData();
        _textMessage = new VRNTextMessage();
    }

    private IEnumerator Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        TryGetComponent<NetworkedObject>(out _netObj);
        if(StretcherAnimator == null)
        {
            StretcherAnimator = GetComponentInChildren<Animator>();
        }
        if(NPCTriggerDeath == null)
        {
            NPCTriggerDeath = GetComponentInChildren<NPCTriggerDeath>();
        }
        NPCTriggerDeath.OnDeath += NPCTriggerDeath_OnDeath;
        NPCTriggerDeath.IsActive = false;

        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnNetObjMessage);

        TryGetComponent<ResetObjectsAction>(out _resetObjects);

        _sockets = new List<CustomXRSocket>();
        GetComponentsInChildren<CustomXRSocket>(_sockets);
        foreach (CustomXRSocket socket in _sockets)
        {
            Debug.Log($"Stretcher socket found: {socket.name}");

        }
        if (NPCStretcherBreathing == null)
        {
            NPCStretcherBreathing = GetComponentInChildren<NPCStretcherBreathing>();
        }
        if (!NetworkManager.IsServer || EquipmentSpawnList == null || EquipmentSpawnList.Count <= 0 || 
            NetworkManager.IsPlaybackMode)
            yield break;

        while (SceneLoadManager.IsLoadInProgress)
            yield return new WaitForEndOfFrame();


        Vector3 startPos = new Vector3(SpawnAreaBounds.center.x, SpawnAreaBounds.center.y,
            SpawnAreaBounds.min.z);
        startPos = transform.TransformPoint(startPos);

        Task<NetworkedObject>[] taskList = new Task<NetworkedObject>[EquipmentSpawnList.Count];
        Guid[] equipGuids = new Guid[EquipmentSpawnList.Count];

        for (int i = 0; i < EquipmentSpawnList.Count; i++)
        {
            var equipRef = EquipmentSpawnList[i];
            equipGuids[i] = Guid.NewGuid();
            taskList[i] = NetworkedObjectManager.SpawnObject(equipRef.RuntimeKey.ToString(),
                equipGuids[i], startPos + new Vector3(0, 0, 0.5f * i), EquipmentStartRotation, false);
        }

        while (!SocketSpawnedEquipment(taskList, equipGuids))
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"StretcherController: Done socketing spawned equipment");
    }

    private void NPCTriggerDeath_OnDeath()
    {
        if (NetworkManager.IsServer)
        {
            SetBreathing(false);
        }
    }

    private void OnDestroy()
    {
        NPCTriggerDeath.OnDeath -= NPCTriggerDeath_OnDeath;
    }

    //returns true when done socketing spawned equipment
    bool SocketSpawnedEquipment(Task<NetworkedObject>[] taskList, Guid[] equipGuids)
    {
        bool spawnCompleted = true;

        for (int i = 0; i < taskList.Length; i++)
        {
            var task = taskList[i];
            if (task == null)
                continue;

            //only complete when all tasks are null
            spawnCompleted = false;

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"StretcherController: {name} couldn't spawn equipment successfully");
                foreach (var guid in equipGuids)
                {
                    NetworkedObjectManager.DestroyObject(guid);
                }
                return true; //abort spawn process
            }

            if (task.IsCompleted)
            {
                taskList[i] = null;
                SocketSpawnedEquipment(task.Result);                
            }
        }

        return spawnCompleted;
    }

    void SocketSpawnedEquipment(NetworkedObject equipNetObj)
    {
        if (!equipNetObj.TryGetComponent<CustomXRInteractable>(out var interactable))
        {
            Debug.LogError($"StretcherController: Spawned equipment {equipNetObj.name} is not an interactable!");
            return;
        }

        foreach (var socket in _sockets)
        {
            if (socket.SocketedInteractable != null)
                continue;

            if ((socket.AllowedInteractableTypes & interactable.InteractableType) <= 0)
                continue;

            socket.RequestSocketItem(interactable);

            if (_resetObjects != null)
            {
                _resetObjects.AddObjectToResetList(interactable.transform);
            }

            return;
        }

        Debug.LogWarning($"StretcherController: Couldn't find socket for item {interactable.name}");
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (!NetworkManager.IsServer)
            return;

        _boolData.BoolData = false;
        _textMessage.Message = "";
        //_textMessage.Message = "";
        //if (messageType != "SET_STRETCHER_NAME")
        //{
        //    reader.ReadMessage(_boolData);
        //}
        //else
        //{
        //    reader.ReadMessage(_textMessage);
        //}


        //reader.ReadMessage(_boolData);
        switch (messageType)
        {
            case "SET_NPC":
                Debug.Log($"Set NPC message received?!");
                reader.ReadMessage(_boolData);
                SetNPCActive(_boolData.BoolData);
                break;
            case "SET_BG4":
                reader.ReadMessage(_boolData);
                SetBG4Active(_boolData.BoolData);
                break;
            case "SET_BREATH":
                reader.ReadMessage(_boolData);
                SetBreathing(_boolData.BoolData);
                break;
            case "SET_STRETCHER_CAP":
                reader.ReadMessage(_boolData);
                SetNPCCapLamp(_boolData.BoolData);
                break;
            case "SET_STRETCHER_NAME":
                reader.ReadMessage(_textMessage);
                SetNPCName(_textMessage.Message);
                break;
        }

        //if(messageType == "NPC_SYNC")
        //{
        //    Debug.Log($"Received NPC command! {msg.FloatData} and processed as {msg.FloatData != 0}");
        //    UnconciousNPC.SetActive(msg.FloatData != 0);

        //    if(msg.FloatData != 0)
        //    {
        //        ClearAndDisableSockets();
        //        _npcActive = true;
        //    }
        //    else
        //    {
        //        EnableSockets();
        //        _npcActive = false;
        //    }
        //}

        //if (messageType == "BG4_SYNC")
        //{
        //    NPCBGFour.SetActive(msg.FloatData != 0);
        //    _bg4Active = msg.FloatData != 0;
        //}
    }

    public void RequestSetNPCActive(bool state)
    {
        if (NetworkManager.IsServer)
        {
            SetNPCActive(state);
            return;
        }

        if (_netObj == null)
            return;
        _boolData.BoolData = state;
        _netObj.SendMessage("SET_NPC", _boolData);
    }

    

    private void SetNPCActive(bool state)
    {
        if (NetworkManager.IsServer)
        {
            if (state)
            {
                ClearAndDisableSockets();
            }
            else
            {
                EnableSockets();
            }
        }


        UnconciousNPC.SetActive(state);
        NPCTriggerDeath.IsActive = state;
        StretcherAnimator.SetBool("Breathing", state);
        _npcActive = state;
        _needsUpdate = true;
        
        
        //VRNFloatData fData = new VRNFloatData();
        //fData.FloatData = state ? 1 : 0;
        //if (state)
        //{
        //    ClearAndDisableSockets();
        //}
        //else
        //{
        //    EnableSockets();
        //}
        //if (_netObj != null)
        //{
        //    _netObj.SendMessage("NPC_SYNC", fData);
        //}
    }
        
    public void RequestSetNPCParameters(string npcName, bool isCapLampOn)
    {
        if (NetworkManager.IsServer)
        {
            SetNPCCapLamp(isCapLampOn);
            //SetNPCName(npcName);
            return;
        }

        if (_netObj == null)
            return;

        _boolData.BoolData = isCapLampOn;
        _netObj.SendMessage("SET_STRETCHER_CAP", _boolData);

        VRNTextMessage vRNTextMessage = new VRNTextMessage();
        vRNTextMessage.Message = npcName;
        _netObj.SendMessage("SET_STRETCHER_NAME", vRNTextMessage);
    }

    public void RequestSetNPCName(string npcName)
    {
        if (NetworkManager.IsServer)
        {
            SetNPCName(npcName);
            return;
        }

        if(_netObj == null)
            return;
        _textMessage.Message = npcName;
        _netObj.SendMessage("SET_STRETCHER_NAME", _textMessage);
    }
    private void SetNPCName(string name) 
    {
        Debug.Log($"Setting NPC Name!: {name}");
        if (name != null && NameProjectors != null)
        {
            foreach (var proj in NameProjectors)
            {
                proj.Text = name;
                proj.UpdateTexture();
            }
        }
        NPCTriggerDeath.NPCName = name;
        _name = name;
        _needsUpdate = true;
    }
    private void SetNPCCapLamp(bool isCapLampOn)
    {
        if (CapLamp != null)
        {
            CapLamp.SetActive(isCapLampOn);
        }
    }

    //The BG4 should be a child object, so it can safely be enabled/disabled but will only appear if the root (NPC) is active
    public void RequestSetBG4Active(bool state)
    {
        if (NetworkManager.IsServer)
        {
            SetBG4Active(state);
            return;
        }

        if (_netObj == null)
            return;
        _boolData.BoolData = state;
        _netObj.SendMessage("SET_BG4", _boolData);
    }

    private void SetBG4Active(bool state)
    {
        NPCBGFour.SetActive(state);
        NPCTriggerDeath.HasBG4 = state;
        //VRNFloatData fData = new VRNFloatData();
        //fData.FloatData = state ? 1 : 0;
        _bg4Active = state;
        _needsUpdate = true;
        //if (_netObj != null)
        //{
        //    _netObj.SendMessage("BG4_SYNC", fData);
        //}
        SafetyGlasses.SetActive(!state);
    }

    public void RequestSetBreathing(bool state)
    {
        if (NetworkManager.IsServer)
        {
            SetBreathing(state);
            return;
        }
        if (_netObj == null)
        {
            return;
        }
        _boolData.BoolData = state;
        _netObj.SendMessage("SET_BREATH", _boolData);
        //SetBreathing(state);
    }

    public void SetBreathing(bool state)
    {
        //NPCStretcherBreathing.PerformBreathing = state;
        //NPCStretcherBreathing.IsAlive = state;
        //NPCStretcherBreathing.SetAliveState(state);
        //if (_isAlive != state)
        //{
        //    _isAlive = state;
        //    NPCStretcherBreathing.SetAliveState(state);
        //    if (NetworkManager.IsServer)
        //    {
        //        _needsUpdate = true;
        //    }
        //}
        _isAlive = state;
        if(NPCStretcherBreathing != null)
            NPCStretcherBreathing.SetAliveState(state);
        if(StretcherAnimator != null)
            StretcherAnimator.SetBool("Breathing", state);
        
        _needsUpdate = true;
    }

    private void ClearAndDisableSockets()
    {
        if (!NetworkManager.IsServer)
        {
            return;
        }
        foreach(CustomXRSocket socket in _sockets)
        {
            socket.EnableObjectScan = false;
            CustomXRInteractable interactable = socket.SocketedInteractable;
            if(interactable != null)
            {
                //This was causing a NetGrab bug
                //if (interactable.tag == "Blanket")
                //{
                //    NetworkedObject blanketObject = interactable.GetComponent<NetworkedObject>();
                //    if (blanketObject != null)
                //    {
                //        NetworkManager.DestroyObject(blanketObject.uniqueID);
                //    }
                //    continue;
                //}
                if(interactable.CompareTag("Blanket"))
                {
                    //StartCoroutine(DelayedBlanketDestroy(interactable));
                    NetworkedObject blanketObject;
                    if(interactable.TryGetComponent<NetworkedObject>(out blanketObject))
                    {
                        NetworkManager.DestroyObject(blanketObject.uniqueID);
                    }                    
                }
                NetworkedObject nObj = interactable.GetComponent<NetworkedObject>();
                nObj.RequestOwnership();
                Vector3 newPosition = interactable.transform.position;
                newPosition.x += 1.5f;
                interactable.transform.position = newPosition;
                
                interactable.ChangeOwnership(null);
                interactable.InteractableRigidbody.isKinematic = false;
            }
        }
    }

    private void EnableSockets()
    {
        foreach (CustomXRSocket socket in _sockets)
        {
            socket.EnableObjectScan = true;
        }
    }

    public void PerformSelectableObjectAction()
    {
        if (_netObj != null)
        {
            _netObj.RequestOwnership();
        }

        if (_npcActive)
        {
            //if (string.IsNullOrEmpty(_name))
            //{
            //    _name = "Blank";
            //}
            NetworkManager.LogSessionEvent(VRNLogEventType.NpcremovedFromStretcher, null,
                transform.position, transform.rotation, _name);

            RequestSetBG4Active(false);
            RequestSetNPCActive(false);
            RequestSetBreathing(true);
        }
        else
        {
            RequestSetNPCActive(true);
            RequestSetBG4Active(true);
        }
        
    }

    public bool IsNPCActive => _npcActive;    

    public bool GetNPCBG4State()
    {
        return _bg4Active;
    }

    /// <summary>
    /// Done due to some error that occurs with the script that attempts to continue the Grab command on the blanket?
    /// </summary>
    IEnumerator DelayedBlanketDestroy(CustomXRInteractable interactable)
    {
        yield return new WaitForSeconds(0.5f);
        NetworkedObject blanketObject = interactable.GetComponent<NetworkedObject>();
        if (blanketObject != null)
        {
            NetworkManager.DestroyObject(blanketObject.uniqueID);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(SpawnAreaBounds.center, SpawnAreaBounds.size);

        Gizmos.matrix = Matrix4x4.identity;
    }

    public bool NeedsUpdate()
    {
        return _needsUpdate;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _stretcherData.Bg4Active = _bg4Active;
        _stretcherData.NpcActive = _npcActive;
        _stretcherData.NpcAlive = _isAlive;
        _stretcherData.NpcName = _name;

        writer.WriteMessage(_stretcherData);
        _needsUpdate = false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _stretcherData.Bg4Active = false;
        _stretcherData.NpcActive = false;
        _stretcherData.NpcAlive = false;
        _stretcherData.NpcName = "Blank";

        reader.ReadMessage(_stretcherData);

        if (_stretcherData.NpcActive != _npcActive)
            SetNPCActive(_stretcherData.NpcActive);
        if (_stretcherData.Bg4Active != _bg4Active)
            SetBG4Active(_stretcherData.Bg4Active);
        if (_stretcherData.NpcAlive != _isAlive)
            SetBreathing(_stretcherData.NpcAlive);
        if(_stretcherData.NpcName != _name)
            SetNPCName(_stretcherData.NpcName);
        //SetBreathing(_stretcherData.NpcAlive);
    }
}
