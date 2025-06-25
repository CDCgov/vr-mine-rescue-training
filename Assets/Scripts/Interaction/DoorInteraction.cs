using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;
using System;
using UnityEngine.AI;
using System.Threading.Tasks;
using System.Text;
using UnityEditor;

public enum DoorRotationAxis
{
    YAxis,
    XAxis,
    ZAxis
}

public class DoorInteraction : Interactable, INetSync, IInteractableObject, ISelectableObjectAction, ISelectableObject
{
    public NetworkManager NetworkManager;

    public Vector3 TargetRotation;
    public Vector3 StartingRotation;
    public DoorRotationAxis Axis = DoorRotationAxis.YAxis;
    public float DoorOpeningDuration = 1;
    public float DoorClosingDuration = 1;
    public AudioClip DoorOpenSfx;
    public AudioClip DoorCloseSfx;
    public Knock Knock;
    public float OpenDelay = 0;
    public float CloseDelay = 0;
    public AudioSource PurgePlayer;
    public List<Knock> KnockScripts;
    public string DoorName = "RA Door";
    public PlayerManager PlayerManager;
    public Collider DoorTrigger;

    public event Action<bool> DoorStateChanged;
    
    public bool IsDoorOpen
    {
        get
        {
            return _bOpening;
        }
    }

    public string SelectableActionName => "Open/Close Door";

    private Vector3 _baseRotation;
    private Vector3 _startRotation;
    private bool _bOpening = false;
    private bool _animate = false;
    private float _initTime;
    private AudioSource _sfxPlayer;
    private int nInteractCalled = 0;
    private float _openDelayTimer = 0;
    private float _closeDelayTimer = 0;
    private int _activatorPlayerID = -1;
    private float _doorState = 0.0f;
    private NetworkedObject _netObj;
    private VRNActivationState _activationState;
    private float updateTime = 0.5f;

    private void Start()
    {
        _baseRotation = transform.localRotation.eulerAngles;
        _sfxPlayer = GetComponent<AudioSource>();

        _netObj = GetComponent<NetworkedObject>();
        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnDoorMessage);

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _activationState = new VRNActivationState();
        _activationState.Activated = _bOpening;
        _activationState.ActivatationLevel = 1;

        if (DoorTrigger)
        {
            DoorTrigger.gameObject.SetActive(_bOpening);
        }

        if (_bOpening)
            SetDoorState(_bOpening, false);
    }

    private void OnDoorMessage(string messageType, CodedInputStream reader)
    {        
        if (_animate)
        {
            return;
        }
        if (messageType == "OPEN")
        {
            _activationState.Activated = true;
            _activationState.ActivatationLevel = 2;
            SetDoorState(true);
            return;
        }
        if(messageType == "CLOSED")
        {
            _activationState.Activated = false;
            _activationState.ActivatationLevel = 1;
            _bOpening = false;
            SetDoorState(false);
        }
    }

    private void Update()
    {
        if (_animate)
        {
            if (_bOpening)
            {
                if (Time.time > _openDelayTimer)
                {
                    //Quaternion rot = Quaternion.Euler(Vector3.Lerp(_startRotation, TargetRotation, (Time.time - _initTime) / DoorOpeningDuration));
                    //transform.localRotation = rot;
                    //This is the solve for the shortfall of doing 3D Lerp where it will always find the shortest number of steps between angles (this caused off axis rotations in some orientations)
                    Vector3 vec = StartingRotation;
                    switch (Axis)
                    {
                        case DoorRotationAxis.YAxis:
                            vec.y = Mathf.Lerp(StartingRotation.y, TargetRotation.y, (Time.time - _initTime) / DoorOpeningDuration);
                            break;
                        case DoorRotationAxis.XAxis:
                            vec.x = Mathf.Lerp(StartingRotation.x, TargetRotation.x, (Time.time - _initTime) / DoorOpeningDuration);
                            break;
                        case DoorRotationAxis.ZAxis:
                            vec.z = Mathf.Lerp(StartingRotation.z, TargetRotation.z, (Time.time - _initTime) / DoorOpeningDuration);
                            break;
                        default:
                            break;
                    }
                    transform.localEulerAngles = vec;
                    if (transform.localRotation.eulerAngles == TargetRotation || ((Time.time - _initTime) / DoorOpeningDuration) > 1)
                    {
                        _animate = false;
                        _activationState.ActivatationLevel = 2;
                    }
                }
            }
            else
            {
                if (Time.time > _closeDelayTimer)
                {
                    //Quaternion rot = Quaternion.Euler(Vector3.Lerp(_startRotation, _baseRotation, (Time.time - _initTime) / DoorClosingDuration));
                    //transform.localRotation = rot;
                    Vector3 vec = TargetRotation;
                    switch (Axis)
                    {
                        case DoorRotationAxis.YAxis:
                            vec.y = Mathf.Lerp(TargetRotation.y, StartingRotation.y, (Time.time - _initTime) / DoorClosingDuration);
                            break;
                        case DoorRotationAxis.XAxis:
                            vec.x = Mathf.Lerp(TargetRotation.x, StartingRotation.x, (Time.time - _initTime) / DoorClosingDuration);
                            break;
                        case DoorRotationAxis.ZAxis:
                            vec.z = Mathf.Lerp(TargetRotation.z, StartingRotation.z, (Time.time - _initTime) / DoorClosingDuration);
                            break;
                        default:
                            break;
                    }
                    transform.localEulerAngles = vec;
                    if (transform.localRotation.eulerAngles == StartingRotation || ((Time.time - _initTime) / DoorClosingDuration) > 1)
                    {
                        _animate = false;
                        _activationState.ActivatationLevel = 1;
                    }
                    //if (transform.localRotation.eulerAngles == TargetRotation || ((Time.time - _initTime) / DoorOpeningDuration) > 0)
                    //{
                    //    _animate = false;
                    //    _activationState.ActivatationLevel = 1;
                    //}
                }
            }

            if(DoorTrigger)
            {
                DoorTrigger.gameObject.SetActive(_activationState.Activated);
            }

        }
        //if (Knock != null)
        //{
        //    Knock.AllowPlayRequest = !_animate;
        //}
    }

    public override void Interact()
    {
        if (_animate)
        {
            return;
        }
        //nInteractCalled++;
        //Debug.Log("Door interact script called " + nInteractCalled + " times.");
        //CustomXRInteractable doorInteractable = GetComponent<CustomXRInteractable>();
        Vector3 pos = transform.position;
        //if (doorInteractable != null)
        //    pos = doorInteractable.InteractorTransform.position;

        _bOpening = !_bOpening;
        SetDoorState(_bOpening);

        if (_netObj == null || NetworkManager == null)
            return;

        if (_bOpening)
        {
            _netObj.SendMessage("OPEN", new VRNTextMessage());
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.DoorOpen,
                ObjectName = DoorName,
                Position = pos.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
            });
        }
        else
        {
            _netObj.SendMessage("CLOSED", new VRNTextMessage());
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.DoorClose,
                ObjectName = DoorName,
                Position = pos.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
            });
        }
    }

    /// <summary>
    /// Door state set via one-time message on multiplayer rather than sycing up the door transform at set intervals.
    /// </summary>
    /// <param name="opening"></param>
    public void SetDoorState(bool opening, bool playAudio = true)
    {
        _bOpening = opening;
        AudioClip clip = null;
        if (_bOpening)
        {            
            clip = DoorOpenSfx;            
            _openDelayTimer = Time.time + OpenDelay;
            _initTime = _openDelayTimer;
           
        }
        else
        {
            clip = DoorCloseSfx;
            _closeDelayTimer = Time.time + CloseDelay;
            _initTime = _closeDelayTimer;
        }

        if (playAudio && _sfxPlayer != null && clip != null)
        {
            _sfxPlayer.clip = clip;
            _sfxPlayer.Play();
        }

        _startRotation = transform.localRotation.eulerAngles;
        //_initTime = Time.time;
        _animate = true;

        EnableKnockRespone(!opening);

        DoorStateChanged?.Invoke(_bOpening);
    }

    private void EnableKnockRespone(bool enabled)
    {
        if (KnockScripts == null || KnockScripts.Count <= 0)
            return;

        foreach (Knock knock in KnockScripts)
        {
            if (knock == null)
                continue;

            knock.AllowNPCResponse = enabled;
        }
    }

    public bool NeedsUpdate()
    {
        //if (Time.time > updateTime)
        //{
        //    updateTime = Time.time + 0.5f;//This was causing occasional double opens, it seems.
        //    return true;
        //}
        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        //if (_activationState == null)
        //	_activationState = VRNActivationState.Parser.ParseDelimitedFrom(reader);
        //else
        //	_activationState.MergeDelimitedFrom(reader);


        if (_activationState == null)
            _activationState = new VRNActivationState();//VRNActivationState.Parser.ParseDelimitedFrom(reader);

        _activationState.Activated = false;
        _activationState.ActivatationLevel = 0;
        //_activationState.MergeDelimitedFrom(reader);
        reader.ReadMessage(_activationState);
        
        if (_bOpening != _activationState.Activated)
        {
            SetDoorState(_activationState.Activated);
        }
        //Debug.Log("updating? " + _activationState.Activated);
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_netObj == null || !_netObj.HasAuthority)
        {
            return;
        }

        _activationState.Activated = _bOpening;
        //_activationState.ActivatationLevel = -1;
        //VRNActivationState state = new VRNActivationState
        //{
        //    ActivatationLevel = -1,
        //    Activated = _bOpening,

        //};

        //state.WriteDelimitedTo(writer);
        //_activationState.WriteDelimitedTo(writer);
        writer.WriteMessage(_activationState);
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

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        Interact();
    }

    public void OnDeactivated(Transform interactor)
    {
        
    }

    public void PerformSelectableObjectAction()
    {
        if (NetworkManager != null)
        {
            if (IsDoorOpen)
                NetworkManager.LogSessionEvent(VRNLogEventType.Dmaction, "DM Closed Door",
                    transform.position, transform.rotation, gameObject.name);
            else
                NetworkManager.LogSessionEvent(VRNLogEventType.Dmaction, "DM Opened Door",
                    transform.position, transform.rotation, gameObject.name);
        }

        if (_netObj == null || _netObj.HasAuthority)
            SetDoorState(!IsDoorOpen);
        else
            Interact();
    }

    public string GetObjectDisplayName()
    {
        return "Door";
    }

    public void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendFormat("Door Open: {0}", IsDoorOpen);
    }
}