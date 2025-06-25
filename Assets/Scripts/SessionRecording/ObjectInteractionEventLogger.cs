using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkedObject))]
[RequireComponent(typeof(CustomXRInteractable))]
[RequireComponent(typeof(VRNObjectData))]
public class ObjectInteractionEventLogger : MonoBehaviour, IInteractableObject
{
    public enum ObjectInteractionEvent
    {
        Select,
        Activate,
    }

    public const float FieldOfViewAngle = 45;

    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    public ObjectInteractionEvent ObjectEvent = ObjectInteractionEvent.Select;

    public VRNLogEventType EventType;
    public VRNLogEventType DroppedEventType;
    public string EventMessage = "";

    public bool RequireInsideFOV = false;
    public float MinHoldTime = 0.25f;
    public float MinTimeInFOV = 0.5f;
    public float RepeatDelay = 3.0f;
    public float MinDistanceFromPrevious = 0.305f;
    public float IgnoreMinDistanceAfterSeconds = 30;
    public bool AllowRepeatsWhileHeld = true;
    public bool LogDroppedEvent = false;
    public bool CheckInBounds = false;
    public string BoundingBoxTag = "Stretcher";

    private CustomXRInteractable _xrInteractable;
    private VRNObjectData _objData;
    private NetworkedObject _netObj;

    private bool _isHeld = false;
    private float _heldStartTime;
    private float _fovTime = 0;
    private float _lastLogTime = 0;
    private Vector3 _lastLogPosition = Vector3.zero;
    private bool _eventLogged = false; //has an event been logged while its been held this time

    private Transform _cameraTransform;

    private BoxCollider _boundsCollider;

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _xrInteractable = GetComponent<CustomXRInteractable>();
        _objData = GetComponent<VRNObjectData>();
        _netObj = GetComponent<NetworkedObject>();

        if (_objData == null)
        {
            _objData = gameObject.AddComponent<VRNObjectData>();
            _objData.ObjectType = VRNLogObjectType.Generic;
            _objData.ObjectName = "Unknown";
        }

        if (_objData.ObjectName == null)
            _objData.ObjectName = "Unknown";

        //switch (ObjectEvent)
        //{
        //    case ObjectInteractionEvent.Select:
        //        _xrInteractable.onSelectEnter.AddListener(OnObjectEventStart);
        //        _xrInteractable.onSelectExit.AddListener(OnObjectEventEnd);
        //        break;

        //    case ObjectInteractionEvent.Activate:
        //        _xrInteractable.onActivate.AddListener(OnObjectEventStart);
        //        _xrInteractable.onDeactivate.AddListener(OnObjectEventEnd);
        //        break;
        //}
        


        if (CheckInBounds)
        {
            var obj = GameObject.FindGameObjectWithTag(BoundingBoxTag);
            if (obj != null)
            {
                _boundsCollider = obj.GetComponent<BoxCollider>();
                //if (_boundsCollider != null)
                //{
                //    Debug.Log($"ObjectInteractionEventLogger: Found bounding box on {obj.name} for {gameObject.name}");
                //}
            }
        }
    }

    private void OnDestroy()
    {
        //if (_xrInteractable != null)
        //{
        //    _xrInteractable.onSelectEnter.RemoveListener(OnObjectEventStart);
        //    _xrInteractable.onSelectExit.RemoveListener(OnObjectEventEnd);
        //}
    }

    private void OnObjectEventStart()
    {
        //Debug.Log($"ObjectInteractionEventLogger: OnSelectEnter - Owner: {_xrInteractable.CurrentOwner}");

        //ignore message if we are owned by a socket
        //if (_xrInteractable.CurrentOwner as CustomXRSocket != null)
        //    return;

        _heldStartTime = Time.time;
        _isHeld = true;
        _fovTime = 0;
        _eventLogged = false;
    }

    private void OnObjectEventEnd()
    {
        //Debug.Log($"ObjectInteractionEventLogger: OnSelectExit - Owner: {_xrInteractable.CurrentOwner}");

        if (LogDroppedEvent)
        {
            float holdTime = Time.time - _heldStartTime;
            if (holdTime > MinHoldTime && NetworkManager.ClientID == _netObj.OwnerClientID)
            {
                LogEvent(dropped: true);
            }
        }

        _isHeld = false;
    }

    public void LogEvent(bool dropped = false)
    {
        Debug.Log($"ObjectInteractionEventLogger: Log event triggered for {gameObject.name} Dropped: {dropped} Client {NetworkManager.ClientID} Owner {_netObj.OwnerClientID}");

        _eventLogged = true;
        _lastLogTime = Time.time;
        _lastLogPosition = transform.position;

        string positionMetadata = "";

        if (CheckInBounds && _boundsCollider != null)
        {
            if (_boundsCollider.bounds.Contains(transform.position))
            {
                Debug.Log($"ObjectInteractionEventLogger: {gameObject.name} inside bounds");
                positionMetadata = BoundingBoxTag;
            }
            else
            {
                Debug.Log($"ObjectInteractionEventLogger: {gameObject.name} outside bounds");
            }
        }

        switch (EventType)
        {
            case VRNLogEventType.GasCheck:
                GasMeterDisplay gmDisplay = GetComponent<GasMeterDisplay>();
                if(gmDisplay != null)
                {
                    MineAtmosphere mAtmo = gmDisplay.GetAtmosphere();
                    
                    string methane = (mAtmo.Methane * 100).ToString("F1");

                    int co = (int)(Mathf.Round(mAtmo.CarbonMonoxide * 1000000.0f));
                    if (co > GasMeterDisplay.GasMeterMaxCOReading)
                        co = GasMeterDisplay.GasMeterMaxCOReading;
                    //CarbonMonoxideDisplay.text = _Atmosphere.CarbonMonoxide.ToString ("F1");

                    string coText = co.ToString();

                    int h2s4 = (int)(mAtmo.HydrogenSulfide * 100);
                    //HydrogenSulfideDisplay.text = _Atmosphere.HydrogenSulfide.ToString ("F2");
                    string h2s4Text = h2s4.ToString();

                    string o2Text = (mAtmo.Oxygen * 100).ToString("F1");
                    positionMetadata += $"Methane: {methane}%, CO: {coText}ppm, O2: {o2Text}%";
                }
                break;
            case VRNLogEventType.SentinelInspect:
                Sentinel sentinel = GetComponent<Sentinel>();
                if(sentinel != null)
                {
                    positionMetadata += $"{sentinel.SmallText.text} psi";
                }
                break;
            case VRNLogEventType.FireExtinguisherPickedUp:
            case VRNLogEventType.FireExtinguisherDropped:
            case VRNLogEventType.FireExtinguisherDischarge:
                FireExtinguisherInteraction fireExtinguisherInteraction = GetComponent<FireExtinguisherInteraction>();
                if(fireExtinguisherInteraction != null)
                {
                    positionMetadata += $"Capacity: {(fireExtinguisherInteraction.PercentFull * 100).ToString("F1")}%";
                }
                break;
            default:
                break;
        }

        //if (!NetworkManager.IsRecording)
        //    return;

        var playerID = -1;
        if (PlayerManager.CurrentPlayer != null)
            playerID = PlayerManager.CurrentPlayer.PlayerID;

        VRNLogEvent ev = new VRNLogEvent
        {
            EventType = EventType,
            Message = EventMessage,
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            ObjectType = _objData.ObjectType,
            ObjectName = _objData.ObjectName,
            SourcePlayerID = playerID,
            PositionMetadata = positionMetadata,
        };

        if (dropped)
        {
            ev.EventType = DroppedEventType;
        }

        NetworkManager.LogSessionEvent(ev);
    }

    private void Update()
    {
        if (!_isHeld)
            return;

        if (_xrInteractable == null || _xrInteractable.GrabWithoutOwnerChange == false)
        {
            if (!_netObj.HasAuthority && _netObj.OwnershipState != NetworkedObject.NetOwnershipState.OwnershipRequested &&
                !_netObj.ServerExclusive)
                return;
        }

        if (!AllowRepeatsWhileHeld && _eventLogged)
            return;

        float holdTime = Time.time - _heldStartTime;
        if (holdTime < MinHoldTime)
            return;

        float sinceLastLog = Time.time - _lastLogTime;
        if (sinceLastLog < RepeatDelay)
            return;

        //check min distance if we have not exceeded the IgnoreMinDistanceAfterSeconds value
        if (IgnoreMinDistanceAfterSeconds < 0 || sinceLastLog < IgnoreMinDistanceAfterSeconds)
        {
            float distance = Vector3.Distance(_lastLogPosition, transform.position);
            if (distance < MinDistanceFromPrevious)
                return;
        }

        if (RequireInsideFOV)
        {
            if (_cameraTransform == null)
            {
                if (Camera.main == null)
                    return;

                _cameraTransform = Camera.main.transform;
            }

            Vector3 forward = _cameraTransform.forward;
            Vector3 dir = transform.position - _cameraTransform.position;
            dir.Normalize();

            float angle = Vector3.Angle(forward, dir);
            //Debug.Log($"$ObjectInteractionEventLogger: Angle {angle:F2}");

            if (angle < FieldOfViewAngle)
            {
                _fovTime += Time.deltaTime;
            }
            else
            {
                _fovTime = 0;
            }

            if (_fovTime < MinTimeInFOV)
                return;
        }

        LogEvent();
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
        if (ObjectEvent != ObjectInteractionEvent.Select)
            return;

        OnObjectEventStart();
    }

    public void OnDropped(Transform interactor)
    {
        if (ObjectEvent != ObjectInteractionEvent.Select)
            return;

        OnObjectEventEnd();
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        if (ObjectEvent != ObjectInteractionEvent.Activate)
            return;

        OnObjectEventStart();
    }

    public void OnDeactivated(Transform interactor)
    {
        if (ObjectEvent != ObjectInteractionEvent.Activate)
            return;

        OnObjectEventEnd();
    }
}
