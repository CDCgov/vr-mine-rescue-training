using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR;

public enum ControllerType
{
    LeftController,
    RightController,
    None
}

[RequireComponent(typeof(Rigidbody))]
public class CustomXRInteractor : XRObjectController, IUIInteractor, ILineRenderable
{
    //note: upgrade to Unity 2022.3, was IUIInteractable

    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public XRInputManager XRInputManager;

    //public XRJoystickWatcher watcher;
    public Camera HeadCamera;
    //public ControllerType Controller = ControllerType.None;
    public UnityEngine.XR.XRNode ControllerXRNode;
    public float InteractorRadius = 0.1f;
    public float ThrowMultiplier = 1;
    public float RaycastDistance = 0.5f;
    //public Transform AttachPoint;
    public Rigidbody HandRigidbody;
    public Vector3 InteractCheckOffset;

    public event Action<CustomXRInteractable> ObjectPickedUp;
    public event Action<CustomXRInteractable> ObjectDropped;

    public bool AlwaysShowLineRenderer = false;

    public CustomXRInteractable ActiveInteractable
    {
        get { return _interactable; }
    }


    public bool ActiveGraphicsRaycaster = false;
    //public UnityEvent<Vector3> onRaycast;
    public LineRenderer ControllerLineRender;
    public PlayerManager PlayerManager;
    [HideInInspector]
    public bool AllowGrip = true;
    public Action InteractorDestroyed;
    ///public Animator InteractorAnimator;

    private XRInputManager.InputDeviceState _xrInputState;

    private MapmanGraphicsRaycastManager _raycastManager;
    private RaycastHit _hit;
    private SphereCollider _col;

    private float _knockCooldown = 0;
    private Vector3 _lastPosition;
    private Vector3 _positionDelta;

    private float _priorTriggerValue = 0;

    private CustomXRInteractable _interactable;
    //private IInteractableObject _interactInterface;
    protected List<IInteractableObject> _interactInterfaces;
    protected List<IThrowableObject> _throwInterfaces;

    private bool _uiRaycastActivated = false;
    private bool _activated = false;
    //private bool _gripped = false;
    private bool _held = false;
    //private CustomXRInteractable _priorInteract;

    private InputDevice _inputDevice;

    protected Collider[] _colliders;


    private float _lastDropTime;
    private float _interactableOutOfRangeTime = -1;
    private bool _hasGrabbedObject; //flag to limit grab to once per button press

    private bool _overridePhysicsScene = false;
    private PhysicsScene _physScene = default;
    private IInteractableObject[] _interactableCache = null;

    public XRInputManager.InputDeviceState XRInputState
    {
        get { return _xrInputState; }
    }

    void Awake()
    {
        _colliders = new Collider[128];
        _interactInterfaces = new List<IInteractableObject>();
        _throwInterfaces = new List<IThrowableObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (XRInputManager == null)
            XRInputManager = XRInputManager.GetDefault(gameObject);

        NetworkedObjectManager.ObjectHeldStateChanged += OnHeldStateChanged;

        _xrInputState = XRInputManager.GetInputDevice(ControllerXRNode);

        _xrInputState.TriggerPressed += OnTriggerPressed;
        _xrInputState.GripPressed += OnGripPressed;
        _xrInputState.PrimaryAxisClicked += OnJoystickPressed;
        _xrInputState.PrimaryButtonClicked += OnPrimaryButtonClicked;
        _xrInputState.SecondaryButtonClicked += OnSecondaryButtonClicked;

        _lastPosition = transform.position;

        _col = GetComponent<SphereCollider>();
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
        //_activeInteractable = new CustomXRInteractable();

        _interactable = null;
        _interactInterfaces.Clear();
    }


    void OnDestroy()
    {
        if (NetworkedObjectManager != null)
            NetworkedObjectManager.ObjectHeldStateChanged -= OnHeldStateChanged;

        if (_xrInputState != null)
        {
            _xrInputState.TriggerPressed -= OnTriggerPressed;
            _xrInputState.GripPressed -= OnGripPressed;
            _xrInputState.PrimaryAxisClicked -= OnJoystickPressed;
            _xrInputState.PrimaryButtonClicked -= OnPrimaryButtonClicked;
            _xrInputState.SecondaryButtonClicked -= OnSecondaryButtonClicked;
        }

        DestroyBehavior();
    }


    private void OnHeldStateChanged(Guid obj)
    {
        if (!_held || _interactable == null)
            return;

        var netObj = _interactable.GetComponent<NetworkedObject>();
        if (netObj == null || netObj.uniqueID != obj)
            return;

        var data = NetworkedObjectManager.GetObjectData(obj);
        if (data.HeldState.HeldByClientID != NetworkManager.ClientID)
        {
            DropInteractable();
        }
    }

    public override void GainedOwnership(CustomXRInteractable interactable)
    {
        
    }

    public override void LostOwnership(CustomXRInteractable interactable)
    {
        if (_interactable == null || _interactable != interactable)
            return;

        if (_activated)
            ActivateInteractable(false);

        if (_held)
            DropInteractable(setOwner: false);
    }

    private void OnSecondaryButtonClicked(bool pressed)
    {
        if (_interactable == null)
            return;

        RaiseSecondaryButtonPressed(pressed);
    }

    private void OnPrimaryButtonClicked(bool pressed)
    {
        if (_interactable == null)
            return;

        RaisePrimaryButtonPressed(pressed);
    }

    private void OnJoystickPressed(bool pressed)
    {
        if (_interactable == null)
            return;

        RaiseJoystickPressed(pressed);
    }

    private float ComputeDistanceToInteractable(Vector3 sourcePoint, CustomXRInteractable interactable)
    {
        var collider = interactable.GetComponent<Collider>();

        Vector3 pos;
        if (collider == null)
            pos = interactable.transform.position;
        else
            pos = collider.ClosestPoint(sourcePoint);

        return Vector3.Distance(sourcePoint, pos);
    }

    private Vector3 ComputeInteractionSourcePos()
    {
        var offset = transform.TransformDirection(InteractCheckOffset);
        return transform.position + offset;
    }

    public int CheckForInteractables()
    {
        Vector3 sourcePos = ComputeInteractionSourcePos();
        //Collider[] hitColliders = Physics.OverlapSphere(sourcePos, InteractorRadius);
        int numColliders = 0;
        if (_overridePhysicsScene)
        {
            //Debug.Log($"{gameObject.scene.name}: {gameObject.name} checking for interactables");
            numColliders = _physScene.OverlapSphere(sourcePos, InteractorRadius, _colliders, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
        else
        {
            numColliders = Physics.OverlapSphereNonAlloc(sourcePos, InteractorRadius, _colliders); 
        }
        CustomXRInteractable newSelectedInteractable = null;

        float minDistance = Mathf.Infinity;
        int numInteractables = 0;

        //foreach (Collider col in hitColliders)
        for (int i = 0; i < numColliders; i++)
        {
            var col = _colliders[i];

            //Debug.Log(Time.time + " - Grip pressed, found: " + col.name);
            //CustomXRInteractable interactable = col.GetComponent<CustomXRInteractable>();
            CustomXRInteractable xr = col.GetComponentInParent<CustomXRInteractable>();
            if (xr == null)
            {
                continue;
            }

            if (!xr.enabled)
            {
                continue;
            }

            numInteractables++;
            var dist = ComputeDistanceToInteractable(sourcePos, xr);

            if (dist < minDistance)
            {
                newSelectedInteractable = xr;
                minDistance = dist;
            }
 
        }

        if (newSelectedInteractable == null)
        {
            if (_interactableOutOfRangeTime < 0)
                _interactableOutOfRangeTime = Time.time;
        }
        else if (newSelectedInteractable != _interactable)
        {
            _interactableOutOfRangeTime = -1;
            SetActiveInteractable(newSelectedInteractable);
            //HighlightInteractable(true);
        }

        if (_interactable != null)
            _interactable.ShowSelectHighlight();


        //only grab an interactable once per grip button press
        if (_xrInputState != null && _xrInputState.GripBool && (Time.time - _lastDropTime) > 0.75f && !_hasGrabbedObject)
        {
            GrabInteractable();
        }

        return numInteractables;

    }

    public void SetPhysicsSceneOverride(PhysicsScene physics)
    {
        _overridePhysicsScene = true;
        _physScene = physics;
    }

    public void SetActiveInteractable(CustomXRInteractable interactable)
    {

        if (_interactable != null  && _interactable.CurrentOwner == this)
        {
            //ActivateInteractable(false);
            //DropInteractable();
            _interactable.ChangeOwnership(null);
        }

        _interactable = interactable;
        _activated = false;
        _interactInterfaces.Clear();
        if (interactable != null)
            interactable.GetComponentsInChildren<IInteractableObject>(_interactInterfaces);
    }

    public void SetHeldInteractable(CustomXRInteractable interactable)
    {
        _interactableOutOfRangeTime = -1;
        SetActiveInteractable(interactable);
        interactable.ChangeOwnership(this);
        interactable.ActiveInteractors.Add(this);
        //interactable.ShowSelectHighlight(false); 
        _held = true;

        interactable.TryGetComponent<NetworkedObject>(out var netObj);

        if (netObj != null)
            NetworkedObjectManager.SetHeldStatus(netObj.uniqueID, true, NetworkManager.ClientID);
    }

    public bool IsTrackedController
    {
        get
        {
            if (ControllerXRNode == XRNode.LeftHand ||
                ControllerXRNode == XRNode.RightHand)
                return true;
            else
                return false;
        }
    }

    private void Update()
    {
        //check if the selected interactable should be cleared
        if (_interactable != null && _interactableOutOfRangeTime > 0 && (Time.time - _interactableOutOfRangeTime) > 0.33f)
        {
            SetActiveInteractable(null);
        }

        //if (_interactable != null && _held)
        //{
        //    var netObj = _interactable.GetComponent<NetworkedObject>();

        //    if ((netObj != null && !netObj.HasAuthority && !netObj.ServerExclusive) ||
        //        (_interactable.CurrentOwner != this && !_interactable.GrabWithoutOwnerChange))
        //    {
        //        DropInteractable();
        //    }

        //}

        //if (IsHoldingInteractable)
        //    SnapFunction();
        //else if (IsTrackedController)
        //    CheckForInteractables();

        if (IsTrackedController && !IsHoldingInteractable)
            CheckForInteractables();

        //reset grabbed object flag when grip released
        if (_hasGrabbedObject && !_xrInputState.GripBool)
        {
            _hasGrabbedObject = false;
        }

        _positionDelta = transform.position - _lastPosition;
        _lastPosition = transform.position;
        
    }

    private void LateUpdate()
    {
        if (ControllerLineRender == null)
        {
            return;
        }
        if (ActiveGraphicsRaycaster || AlwaysShowLineRenderer)
        {
            ControllerLineRender.enabled = true;
            ControllerLineRender.SetPosition(0, transform.position);
            if (Physics.Raycast(transform.position, transform.forward, out _hit, RaycastDistance, LayerMask.GetMask("UI")))
            {
                ControllerLineRender.SetPosition(1, _hit.point);
            }
            else
            {
                ControllerLineRender.SetPosition(1, transform.position + transform.forward * RaycastDistance);
            }
        }
        else
        {
            
            ControllerLineRender.enabled = false;
        }
    }

    private void PerformTriggerRaycastAction(bool triggerValue)
    {
        if (triggerValue)
        {
            if (!_uiRaycastActivated)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, RaycastDistance, LayerMask.GetMask("UI")))
                {
                    MapmanGraphicsRaycastManager rayMan = hit.collider.GetComponent<MapmanGraphicsRaycastManager>();
                    if (rayMan != null)
                    {
                        _raycastManager = rayMan;
                        PerformGraphicsRaycast(hit.point);
                    }
                }
                _uiRaycastActivated = true;
            }
        }
        else
        {
            _uiRaycastActivated = false;
        }
    }


    private void OnTriggerPressed(bool triggerValue)
    {
        //Debug.Log($"XRTrigger: {triggerValue}");

        if (ActiveGraphicsRaycaster)
        {
            Debug.Log("Trigger pressed with active graphics raycaster");
            PerformTriggerRaycastAction(triggerValue);
            return;
        }

        if (_interactable != null)
        {
            if (triggerValue)
            {
                ActivateInteractable(true);
            }
            else
            {
                ActivateInteractable(false);
            }
        }
    }


    //private void OnPrimaryButtonPressed(bool pressed)
    //{
    //    if (_interactable == null)
    //        return;

    //    RaisePrimaryButtonPressed(pressed);
    //}

    public void SimulateGripPress(bool gripValue)
    {
        OnGripPressed(gripValue);
    }

    public void SimulateTriggerPress(bool triggerValue)
    {
        OnTriggerPressed(triggerValue);
    }

    private void OnGripPressed(bool gripValue)
    {
        if (!AllowGrip || _interactable == null)
            return;

        if (gripValue)
            GrabInteractable();
        else
            DropInteractable();
    }

    private IEnumerable<IInteractableObject> AllInteractInterfaces()
    {
        return Util.GetAllInterfaces<IInteractableObject>(_interactInterfaces);

        //int numInterfaces = _interactInterfaces.Count;
        //for (int i = 0; i < numInterfaces; i++)
        //{
        //    yield return _interactInterfaces[i];

        //    //check the interfaces haven't changed
        //    if (numInterfaces != _interactInterfaces.Count)
        //        break;
        //}
    }

    private void RaiseActivated()
    {
        if (_interactableCache == null)
            _interactableCache = new IInteractableObject[50];

        try
        {
            int count = 0;
            foreach (var iface in AllInteractInterfaces())
            {
                _interactableCache[count] = iface;
                count++;
                //iface?.OnActivated(transform);
            }

            for (int i = 0; i < count; i++)
            { 
                _interactableCache[i]?.OnActivated(transform);
            }
        }
        catch (Exception ex) { Debug.LogError($"Error in OnActivated {ex.Message} {ex.StackTrace}"); }
    }

    private void RaiseDeactivated()
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnDeactivated(transform);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnDeactivated {ex.Message} {ex.StackTrace}"); }
    }

    private void RaisePickedUp()
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnPickedUp(transform);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnPickedUp {ex.Message} {ex.StackTrace}"); }
    }

    private void RaiseDropped()
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnDropped(transform);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnDropped {ex.Message} {ex.StackTrace}"); }
    }

    private void RaisePrimaryButtonPressed(bool pressed)
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnPrimaryButtonPressed(transform, pressed);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnPrimaryButtonPressed {ex.Message} {ex.StackTrace}"); }
    }

    private void RaiseSecondaryButtonPressed(bool pressed)
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnSecondaryButtonPressed(transform, pressed);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnSecondaryButtonPressed {ex.Message} {ex.StackTrace}"); }
    }

    private void RaiseJoystickPressed(bool pressed)
    {
        try
        {
            foreach (var iface in AllInteractInterfaces())
                iface?.OnJoystickPressed(transform, pressed);
        }
        catch (Exception ex) { Debug.LogError($"Error in OnJoystickPRessed {ex.Message} {ex.StackTrace}"); }
    }


    private void ActivateInteractable(bool activated)
    {
        if (_interactable == null)
            return;

        if (activated == _activated)
            return;

        if (activated && _interactable.RequireGrabToActivate && _held == false)
            return;

        try
        {

            if (activated)
            {
                HapticShake();
                //_interactable.onActivate.Invoke();
                RaiseActivated();
            }
            else
            {
                //_interactable.onDeactivate.Invoke();
                RaiseDeactivated();
            }
        }
        catch (Exception ex)
        {
            if (_interactable != null)
                Debug.LogError($"Error activating interactable {_interactable.name} : {ex.Message} {ex.StackTrace}");
            else
                Debug.LogError($"Error activating interactable NULL : {ex.Message} {ex.StackTrace}");
        }


        _activated = activated;
    }

    //private void HighlightInteractable(bool showHighlight)
    //{
    //    if (_interactable == null)
    //        return;

    //    _interactable.ShowSelectHighlight(showHighlight);
    //}

    public bool IsHoldingInteractable
    {
        get
        {
            if (_interactable == null)
                return false;

            return _held;
        }
    }

    private void GrabInteractable()
    {
        if (_hasGrabbedObject)
            return;

        if (_interactable == null || !_interactable.IsGrabbable)
            return;

        _hasGrabbedObject = true;
        _interactableOutOfRangeTime = -1;
        NetworkedObject netObj = null;

        //NetworkedObject netObj = _interactable.GetComponent<NetworkedObject>();
        if (_interactable.TryGetComponent<NetworkedObject>(out netObj))
        {
            if (!netObj.ServerExclusive)
                netObj.RequestOwnership();

            //NetworkedObjectManager.SetHeldStatus(netObj.uniqueID, true, NetworkManager.ClientID);
        }

        if (!_interactable.GrabWithoutOwnerChange)
            _interactable.ChangeOwnership(this);

        //set held status after ownership change since ownership change can triggger a drop
        if (netObj != null)
            NetworkedObjectManager.SetHeldStatus(netObj.uniqueID, true, NetworkManager.ClientID);

        _interactable.ActiveInteractors.Add(this);

        RaisePickedUp();
        HapticShake();

        //Rigidbody rb = _interactable.GetComponent<Rigidbody>();

        if (_interactable.InteractableRigidbody != null)
            _interactable.InteractableRigidbody.isKinematic = true;

        _held = true;

        SnapToInteractor();

        ObjectPickedUp?.Invoke(_interactable);

    }

    public void DropInteractable(bool setOwner = true)
    {
        if (_interactable == null || !_held)
            return;

        var velocity = _xrInputState.Velocity * ThrowMultiplier;
        var angularVel = _xrInputState.AngularVelocity;

        if (transform.parent != null)
        {
            velocity = transform.parent.TransformDirection(velocity);
            angularVel = transform.parent.TransformDirection(angularVel * -1.0f);
        }

        DropInteractable(setOwner, _interactable.ThrowOnDetach, velocity, angularVel);
    }

    public void DropInteractable(bool setOwner, bool throwOnDetach, Vector3 throwVelocity, Vector3 throwAngularVelocity)
    {
        if (_interactable == null || !_held)
            return;

        _lastDropTime = Time.time;

        Debug.Log($"CustomXRInteractor: {gameObject.name} Dropping held item {_interactable.name} scene {gameObject.scene.name}");

        ActivateInteractable(false); 

        _held = false;
        var droppedInteractable = _interactable;
        _interactable = null;

        if (droppedInteractable.CurrentOwner != this && droppedInteractable.GrabWithoutOwnerChange != true)
            return;


        //HighlightInteractable(false);

        if (throwOnDetach)
        {
            _throwInterfaces.Clear();
            droppedInteractable.GetComponents<IThrowableObject>(_throwInterfaces);
            if (_throwInterfaces.Count > 0)
            {
                foreach (var iface in _throwInterfaces)
                {
                    iface.ThrowObject(transform, throwVelocity, throwAngularVelocity);
                }
            }
            else if (droppedInteractable.InteractableRigidbody != null)
            {
                droppedInteractable.InteractableRigidbody.isKinematic = false;
                droppedInteractable.InteractableRigidbody.velocity = throwVelocity;
                droppedInteractable.InteractableRigidbody.angularVelocity = throwAngularVelocity;
                //droppedInteractable.InteractableRigidbody.velocity = (_positionDelta / Time.deltaTime) * ThrowMultiplier;
            }
        }


        droppedInteractable.ActiveInteractors.Remove(this);

        var netObj = droppedInteractable.GetComponent<NetworkedObject>();
        if (netObj != null && droppedInteractable.ActiveInteractors.Count <= 0)
        {
            var objData = NetworkedObjectManager.GetObjectData(netObj.uniqueID);
            if (objData != null && objData.HeldState != null)
            {
                if (objData.HeldState.HeldByClientID == NetworkManager.ClientID)
                    NetworkedObjectManager.SetHeldStatus(netObj.uniqueID, false);
            }
        }

        if (setOwner && !droppedInteractable.GrabWithoutOwnerChange)
            droppedInteractable.ChangeOwnership(null);//this handles if the exit call changes ownership to something else


        //else if (droppedInteractable.CurrentOwner != null)
        //{
        //    //droppedInteractable.CurrentOwner.SnapToInteractor();
        //}

        RaiseDropped();

        _interactInterfaces.Clear();

        ObjectDropped?.Invoke(droppedInteractable);

    }


    private void OnTriggerEnter(Collider other)
    {
        if (_xrInputState == null)
            return; //temporary fix to not trigger on sockets

        Knock knock = other.GetComponent<Knock>();//Who's there?

        if (knock != null)
        {
            var knockResponse = knock.PlayKnock();

            if (Time.time > _knockCooldown || knockResponse != null)
            {
                _knockCooldown = Time.time + 10;
                knock.LogKnockEvent(knock.KnockName, knockResponse, transform.position);
            }

            //bool knockPlaying = knock.PlayKnock();
            //if (knockPlaying)
            //{
            //    HapticShake(1);
            //}
            //if (knockPlaying && (Time.time > _knockCooldown))
            //{
            //    NetworkManager.LogSessionEvent(new VRNLogEvent
            //    {
            //        EventType = VRNLogEventType.DoorKnock,
            //        ObjectName = knock.KnockName,
            //        Position = transform.position.ToVRNVector3(),
            //        Rotation = transform.rotation.ToVRNQuaternion(),
            //        SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
            //    });
            //    _knockCooldown = Time.time + 10;
            //}
        }

    }


    private void OnDrawGizmosSelected()
    {
        Vector3 sourcePos = ComputeInteractionSourcePos();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(sourcePos, InteractorRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(sourcePos, 0.02f);
    }

    private void PerformGraphicsRaycast(Vector3 position)
    {
        Vector2 screenPos = HeadCamera.WorldToScreenPoint(position);
        _raycastManager.PerformRaycast(screenPos);
    }

    private void OnGrip(bool isGripped)
    {
        switch (ControllerXRNode)
        {
            case XRNode.LeftHand:
                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmLGripOn, isGripped);
                break;
            case XRNode.RightHand:
                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmRGripOn, isGripped);
                break;
            default:
                break;
        }
    }

    public void SnapToInteractor()
    {
        if (_interactable == null)
            return;

        if (_interactable.GrabWithoutOwnerChange)
            return;

        _interactable.transform.SetParent(transform);
        if (!_interactable.SnapToController)
        {
            _interactable.transform.localPosition = Vector3.zero;

            if (_interactable.RotateToPalm)
            {
                _interactable.transform.localRotation = ComputePalmRotation(_interactable);
            }
            else
            {
                _interactable.transform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            //var dir = _interactable.transform.position - transform.position;
            //if (Physics.Raycast(transform.position, dir.normalized, out var hit, dir.magnitude + 1))
            //{

            //}

            _interactable.transform.position = CalculateHeldPosition(_interactable.transform);

        }
    }

    public Vector3 CalculateHeldPosition(Transform obj)
    {
        var collider = obj.GetComponentInChildren<Collider>();

        //if (_interactable.TryGetComponent<Collider>(out var collider))
        if (collider != null)
        {
            if (!collider.bounds.Contains(transform.position))
            {
                var closestPoint = collider.ClosestPoint(transform.position);
                //closestPoint = transform.InverseTransformPoint(closestPoint);
                //_interactable.transform.localPosition = closestPoint;

                var offset = obj.transform.position - closestPoint;
                //_interactable.transform.position = transform.position + offset;
                return transform.position + offset;
            }
        }

        return obj.position;
    }

    private Quaternion ComputePalmRotation(CustomXRInteractable interactable)
    {
        var palmEuler = interactable.PalmRotationEulers;
        if (ControllerXRNode == XRNode.LeftHand)
            palmEuler.z *= -1;

        return Quaternion.Euler(palmEuler);
    }

    public void HapticShake(float intensity = 0.5f, float duration = 0.1f)
    {
        //Debug.Log("~~~~~DO THE HAPTIC SHAKE~~~~");
        _inputDevice.SendHapticImpulse(0, intensity, duration);
    }

    protected virtual void DestroyBehavior()
    {
        Debug.Log($"Destroying interactor: {gameObject.name}");
        InteractorDestroyed?.Invoke();
    }

    #region IUIInteractable Implementation based on Unity XR interaction RayInteractor    
    // Input Module for fast access to UI systems.
    XRUIInputModule m_InputModule;
    Vector3[] _linePoints;

    void OnEnable()
    {
        FindXRUIInputModule();

        if (m_InputModule != null)
            m_InputModule.RegisterInteractor(this);
    }

    void OnDisable()
    {
        if (m_InputModule != null)
            m_InputModule.UnregisterInteractor(this);
    }

    void FindXRUIInputModule()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
            return;
        //eventSystem = new GameObject("Event System", typeof(EventSystem)).GetComponent<EventSystem>();

        m_InputModule = eventSystem.GetComponent<XRUIInputModule>();
        //if (m_InputModule == null)
        //    m_InputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
    }

    /// <summary>
    /// Updates the current UI Model to match the state of the Interactor
    /// </summary>
    /// <param name="model">The model that will match this Interactor</param>
    public void UpdateUIModel(ref TrackedDeviceModel model)
    {
        model.maxRaycastDistance = 10;
        model.position = transform.position;
        model.orientation = transform.rotation;
        //model.select = Input.GetKey(KeyCode.Space);
        //model.select = _triggerValue > 0.1f ? true : false;
        if (_xrInputState != null)
            model.select = _xrInputState.TriggerBool;
        else
            model.select = false;

        if (model.select)
        {
            //Debug.Log($"XR Select");
        }

        Vector3[] linePoints = null;
        int numPoints = 0;
        GetLinePoints(ref linePoints, out numPoints);

        List<Vector3> raycastPoints = model.raycastPoints;
        raycastPoints.Clear();
        for (int i = 0; i < linePoints.Length; i++)
        {
            raycastPoints.Add(linePoints[i]);
        }

        Debug.DrawLine(linePoints[0], linePoints[1], Color.green);


        //model.raycastLayerMask = LayerMask.NameToLayer("UI");
        model.raycastLayerMask = 0xFFFF;
    }

    /// <summary>
    /// Attempts to retrieve the current UI Model.  Returns false if not available.
    /// </summary>
    /// <param name="model"> The UI Model that matches that Interactor.</param>
    /// <returns></returns>
    public bool TryGetUIModel(out TrackedDeviceModel model)
    {
        if (m_InputModule != null)
        {
            if (m_InputModule.GetTrackedDeviceModel(this, out model))
                return true;
        }

        model = new TrackedDeviceModel(-1);
        return false;
    }


    public bool GetLinePoints(ref Vector3[] linePoints, out int noPoints)
    {
        if (_linePoints == null)
            _linePoints = new Vector3[2];

        _linePoints[0] = transform.position;
        _linePoints[1] = transform.position + transform.forward * 5.0f;

        linePoints = _linePoints;
        noPoints = _linePoints.Length;

        return true;

    }

    public bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget)
    {
        position = Vector3.zero;
        normal = Vector3.up;
        positionInLine = 0;
        isValidTarget = false;

        return false;

    }

    #endregion

}
