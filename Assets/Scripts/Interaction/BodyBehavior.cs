using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyBehavior : MonoBehaviour, IInteractableObject, INetSync
{
    public List<GameObject> BG4Objects = new List<GameObject>();
    public GameObject Glasses;
    public string BodyName = "Fred";
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public float MaxSyncInterval = 0.1f;

    private bool _bg4Enabled = false;
    private bool _hasBeenAssessed = false;
    private StretcherController _stretcher;
    private ComponentInfo_DeadNPC _deadNPC;
    private NetworkedObject _netObj;
    private bool _needsUpdate = true;
    public bool _bg4Active = false;
    private VRNBoolData _boolData;
    private float _lastSyncTime = 0;

    public ActivationState CanActivate
    {
        get
        {
            if (!_hasBeenAssessed)
            {
                NetworkManager.LogSessionEvent(VRNLogEventType.NpcstatusUpdate, $"Condition: Dead",
                    transform.position, transform.rotation, BodyName);

                _hasBeenAssessed = true;
            }
            return ActivationState.Error;
        }
    }

    private void Awake()
    {
        _boolData = new VRNBoolData();
    }

    private void Start()
    {
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            return;
        }
        if(NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }

        if(PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        if(_deadNPC == null)
        {
            TryGetComponent<ComponentInfo_DeadNPC>(out  _deadNPC);
        }
        if(_netObj == null)
        {
            TryGetComponent<NetworkedObject>(out _netObj);
        }

        _netObj.RegisterMessageHandler(OnNetObjMessage);
    }

    private void Update()
    {
        if (_hasBeenAssessed && PlayerManager.CurrentPlayer != null)
        {
            if(Vector3.Distance(PlayerManager.CurrentPlayer.RigTransform.position, transform.position) > 9.144f) 
            {
                _hasBeenAssessed = false;
            }
        }
    }

    public bool BG4Active
    {
        get
        {
            return _bg4Enabled;
        }
    }

    public bool EnableBG4(bool state = true)
    {
        if(BG4Objects.Count == 0)
        {
            return false;
        }
        foreach (GameObject obj in BG4Objects)
        {
            obj.SetActive(state);
        }
        Glasses.SetActive(!state);
        _needsUpdate = state;
        _bg4Enabled = state;
        _bg4Active = state;
        return _bg4Enabled;
    }

    //public void AddBG4ToBody()
    //{
    //    Debug.Log($"Adding BG4 to {_deadNPC.BodyName}");
    //    if (RequestEnableBG4())
    //    {
    //        NetworkManager.DestroyObject(_netObj.uniqueID);

    //        NetworkManager.LogSessionEvent(VRNLogEventType.EquipmentAdded, null,
    //            transform.position, transform.rotation,
    //            "BG4", VRNLogObjectType.Bg4);
    //    }
    //}
    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType == "BODY_BG4")
        {
            Debug.Log($"Message received!: Turn on BG4?");
            if (!_bg4Enabled)
            {
                EnableBG4();
            }
        }
    }
    public bool RequestEnableBG4()
    {
        if (_bg4Enabled)
        {
            return false;
        }
        if (NetworkManager.IsServer)
        {
            EnableBG4();
            //_netObj.SendMessage("BODY_BG4", new VRNTextMessage());
            return true;
        }
        //EnableBG4();
        if(_netObj == null) { return false; }
        _netObj.SendMessage("BODY_BG4", new VRNTextMessage());

        return true;
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
    }

    public void OnPickedUp(Transform interactor)
    {
    }

    public void OnDropped(Transform interactor)
    {
    }

    public void OnActivated(Transform interactor)
    {
        
        //if (_stretcher == null)
        //{
        //    _stretcher = GameObject.FindObjectOfType<StretcherController>();
        //    if (_stretcher == null)
        //    {
        //        return;
        //    }
        //}

        //if (_stretcher.IsNPCActive)
        //{
        //    return;
        //}
        //_stretcher.RequestSetNPCActive(true);
        
        //if (_bg4Enabled)
        //{
        //    _stretcher.RequestSetBG4Active(true);
        //}
        //_stretcher.RequestSetBreathing(false);
        //NetworkManager.LogSessionEvent(VRNLogEventType.NpcplacedOnStretcher, null,
        //    transform.position, transform.rotation, _deadNPC.BodyName);//Custom Log Event to log puting body on stretcher?

        
        //if (_netObj != null)
        //{
        //    NetworkManager.DestroyObject(_netObj.uniqueID);
        //}
    }

    public void OnDeactivated(Transform interactor)
    {
    }
    
    public bool NeedsUpdate()
    {
        if (_boolData.BoolData != _bg4Active)
            return true;
        return false;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (!NetworkManager.IsServer)
        {
            return;
        }
        _boolData.BoolData = _bg4Active;
        writer.WriteMessage(_boolData);
        //_needsUpdate = false;
        _lastSyncTime = Time.time;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _boolData.BoolData = false;
        reader.ReadMessage(_boolData);
        //if (_boolData.BoolData != _bg4Active)
        //{
        //    EnableBG4();
        //}
        EnableBG4(_boolData.BoolData);
    }
}
