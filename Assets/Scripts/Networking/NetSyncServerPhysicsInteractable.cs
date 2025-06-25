using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkedObject))]
public class NetSyncServerPhysicsInteractable : MonoBehaviour, INetSync, IInteractableObject, IThrowableObject
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;

    public bool ServerSideKinematic = false;
    [System.NonSerialized]
    public bool IgnorePositionSync = false;

    private NetworkedObject _netObj;

    //private float _nextSyncTime = 0;
    private Vector3 _lastSyncPos = Vector3.zero;
    private Quaternion _lastSyncRot = Quaternion.identity;
    private Vector3 _offset = Vector3.zero;
    private Quaternion _rotOffset = Quaternion.identity;

    private VRNPhysicsSyncData _vrnPhysicsSync;
    private VRNPhysicsSyncData _vrnPhysicsRequest;
    private Transform _interactor = null;
    private Rigidbody _rb = null;
    private CustomXRInteractable _interactable;
    //private ComponentInfo_Rigidbody _componentInfoRB;
    private bool _localHeld = false;
    private bool _held = false;


    private NetSyncServerConstraint _constraint;

    void Awake()
    {
        _vrnPhysicsSync = new VRNPhysicsSyncData();
        _vrnPhysicsSync.Position = new VRNVector3();
        _vrnPhysicsSync.Rotation = new VRNQuaternion();
        _vrnPhysicsSync.Velocity = new VRNVector3();
        _vrnPhysicsSync.AngularVelocity = new VRNVector3();

        _vrnPhysicsRequest = new VRNPhysicsSyncData();
        _vrnPhysicsRequest.Position = new VRNVector3();
        _vrnPhysicsRequest.Rotation = new VRNQuaternion();
        _vrnPhysicsRequest.Velocity = new VRNVector3();
        _vrnPhysicsRequest.AngularVelocity = new VRNVector3();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        NetworkedObjectManager.ObjectHeldStateChanged += OnHeldStateChanged;

        _netObj = GetComponent<NetworkedObject>();
        _rb = GetComponent<Rigidbody>();
        _interactable = GetComponent<CustomXRInteractable>();

        //_componentInfoRB = GetComponentInChildren<ComponentInfo_Rigidbody>();

        _netObj.RegisterMessageHandler(OnNetObjMessage);

        _constraint = GetComponent<NetSyncServerConstraint>();

        if (NetworkManager.IsServer)
        {
            _rb.isKinematic = ServerSideKinematic;
        }
        else
        {
            _rb.isKinematic = true;
            //DisableColliders();
        }
    }


    private void OnHeldStateChanged(Guid obj)
    {
        if (_netObj == null || obj != _netObj.uniqueID)
            return;

        var data = NetworkedObjectManager.GetObjectData(obj);
        if (data.HeldState.HeldByClientID == NetworkManager.ClientID)
        {
            _localHeld = data.HeldState.ObjectHeld;
        }
        else
        {
            _localHeld = false;
        }

        _held = data.HeldState.ObjectHeld;
    }

    void OnDestroy()
    {
        if (_netObj != null)
            _netObj.UnregisterMessageHandler(OnNetObjMessage);

        if (NetworkedObjectManager != null)
            NetworkedObjectManager.ObjectHeldStateChanged -= OnHeldStateChanged;
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (NetworkManager == null || !NetworkManager.IsServer)
            return;

        if (IgnorePositionSync)
            return;

        _vrnPhysicsRequest.Position.ResetData();
        _vrnPhysicsRequest.Rotation.ResetData();
        _vrnPhysicsRequest.Velocity.ResetData();
        _vrnPhysicsRequest.AngularVelocity.ResetData();
        _vrnPhysicsRequest.UseGravity = false;
        _vrnPhysicsRequest.IsKinematic = false;

        switch (messageType)
        {
            case "P_REQ":
                reader.ReadMessage(_vrnPhysicsRequest);
                ProcessPositionSyncRequest();
                break;
            case "THROW":
                reader.ReadMessage(_vrnPhysicsRequest);
                ProcessThrow();
                break;
        }
    }

    void SendPositionSyncRequest()
    {
        if (_interactor == null)
            return;

        _vrnPhysicsRequest.Velocity.ResetData();
        _vrnPhysicsRequest.AngularVelocity.ResetData();
        _vrnPhysicsRequest.UseGravity = false;

        CalculateHeldPosition(out var pos, out var rot);

        pos.CopyTo(_vrnPhysicsRequest.Position);
        rot.CopyTo(_vrnPhysicsRequest.Rotation);

        if (NetworkManager.IsServer)
        {
            ProcessPositionSyncRequest();
        }
        else
        {
            _netObj.SendMessage("P_REQ", _vrnPhysicsRequest);
        }
    }

    void SendThrow(Vector3 velocity, Vector3 angularVelocity)
    {
        if (!_localHeld)
            return;

        _vrnPhysicsRequest.Velocity.ResetData();
        _vrnPhysicsRequest.AngularVelocity.ResetData();
        _vrnPhysicsRequest.UseGravity = false;

        if (_interactor != null)
        {
            CalculateHeldPosition(out var pos, out var rot);
            pos.CopyTo(_vrnPhysicsRequest.Position);
            rot.CopyTo(_vrnPhysicsRequest.Rotation);
        }

        velocity.CopyTo(_vrnPhysicsRequest.Velocity);
        angularVelocity.CopyTo(_vrnPhysicsRequest.AngularVelocity);

        _vrnPhysicsRequest.UseGravity = true;

        Debug.Log($"NetServerPhys: Sending Throw {_vrnPhysicsRequest.Velocity} {_vrnPhysicsRequest.AngularVelocity}");

        if (NetworkManager.IsServer)
        {
            ProcessThrow();
        }
        else
        {
            _netObj.SendMessage("THROW", _vrnPhysicsRequest);
        }

    }

    void ProcessPositionSyncRequest()
    {
        if (!NetworkManager.IsServer)
            return;

        _rb.isKinematic = true;
        _held = true;
        transform.position = _vrnPhysicsRequest.Position.ToVector3();
        transform.rotation = _vrnPhysicsRequest.Rotation.ToQuaternion();
    }

    void ProcessThrow()
    {
        if (!NetworkManager.IsServer)
            return;

        Debug.Log($"NetServerPhys: Received Throw {_vrnPhysicsRequest.Velocity} {_vrnPhysicsRequest.AngularVelocity}");

        _rb.isKinematic = ServerSideKinematic;
        _rb.useGravity = true;
        transform.position = _vrnPhysicsRequest.Position.ToVector3();
        transform.rotation = _vrnPhysicsRequest.Rotation.ToQuaternion();
        _rb.velocity = _vrnPhysicsRequest.Velocity.ToVector3();
        _rb.angularVelocity = _vrnPhysicsRequest.AngularVelocity.ToVector3();
    }


    private void UpdateClientPosition()
    {
        if (NetworkManager == null || (NetworkManager.IsServer && !NetworkManager.IsPlaybackMode))
            return;

        if (_interactor != null && _localHeld)
        {
            ////while being held use local position data
            //transform.position = _vrnPhysicsRequest.Position.ToVector3();
            //transform.rotation = _vrnPhysicsRequest.Rotation.ToQuaternion();
        }
        else
        {
            transform.position = _vrnPhysicsSync.Position.ToVector3();
            transform.rotation = _vrnPhysicsSync.Rotation.ToQuaternion();
        }
    }

    void LateUpdate()
    {
        if (_interactable != null && _interactable.CurrentOwner as CustomXRSocket != null)
        {
            _rb.isKinematic = true;
            return;
        }

        if (_interactor != null)
        {
            //while being held use local position data
            CalculateHeldPosition(out var pos, out var rot);
            transform.position = pos;
            transform.rotation = rot;
        }

        if (NetworkManager.IsServer && _rb.isKinematic && !ServerSideKinematic) 
        {
            //check if we are no longer held
            if (!_held)
            {
                _rb.isKinematic = false;
            }
        }

        if (NetworkManager.IsPlaybackMode)
            _rb.isKinematic = true;

        ConstrainPosition();
    }
    
    void ConstrainPosition()
    {
        if (!NetworkManager.IsServer)
            return;

        if (_constraint == null)
            return;

        if (_rb.isKinematic)
            return;

        var pos = transform.position;
        var rot = transform.rotation;

        _constraint.GetConstrainedMotion(ref pos, ref rot);

        transform.position = pos;
        transform.rotation = rot;
    }

    private void DisableColliders()
    {
        var colliders = transform.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager == null || _netObj == null)
            return;

        if (NetworkManager.IsServer && !_netObj.HasAuthority)
            _netObj.RequestOwnership();

        if (_interactor != null)
            SendPositionSyncRequest();

    }

    public bool NeedsUpdate()
    {
        //return false;
        //if (Time.time > _nextSyncTime)
        //    return true;

        var dist = Vector3.Distance(_lastSyncPos, transform.position);
        if (dist > 0.005f)
            return true;

        float angleDelta = Quaternion.Angle(transform.rotation, _lastSyncRot);
        if (angleDelta > 0.5f)
            return true;


        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _vrnPhysicsSync.AngularVelocity.ResetData();
        _vrnPhysicsSync.Velocity.ResetData();
        _vrnPhysicsSync.Rotation.ResetData();
        _vrnPhysicsSync.Position.ResetData();
        _vrnPhysicsSync.IsKinematic = false;
        _vrnPhysicsSync.UseGravity = false;

        reader.ReadMessage(_vrnPhysicsSync);

        UpdateClientPosition();
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        var pos = transform.position;
        var rot = transform.rotation;

        pos.CopyTo(_vrnPhysicsSync.Position);
        rot.CopyTo(_vrnPhysicsSync.Rotation);

        _lastSyncPos = pos;
        _lastSyncRot = rot;
        //_nextSyncTime = Time.time + 5.0f;

        _vrnPhysicsSync.Velocity.ResetData();
        _vrnPhysicsSync.AngularVelocity.ResetData();

        if (_rb != null)
        {
            //_rb.velocity.CopyTo(_vrnPhysicsSync.Velocity);
            //_rb.angularVelocity.CopyTo(_vrnPhysicsSync.AngularVelocity);            
        }

        _vrnPhysicsSync.IsKinematic = ServerSideKinematic;

        writer.WriteMessage(_vrnPhysicsSync);
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {

    }

    public void OnDeactivated(Transform interactor)
    {

    }

    public void OnDropped(Transform interactor)
    {
        //Debug.Log($"NetServerPhys: {gameObject.name} OnDropped scene {gameObject.scene.name}");
        if (interactor != null && interactor != _interactor)
        {
            if (_interactor == null)
                Debug.Log($"NetServerPhys: {interactor.name} doesn't match _interactor (null) scene: {gameObject.scene.name}");
            else
                Debug.Log($"NetServerPhys: {interactor.name} doesn't match _interactor ({_interactor.name}) scene: {gameObject.scene.name}");
            return; //don't drop if a previously active interactor lets go
        }

        _interactor = null;
        UpdateClientPosition();

        Debug.Log($"NetServerPhys: {gameObject.name} dropped");
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    private void CalculateHeldPosition(out Vector3 pos, out Quaternion rot)
    {        
        if (_interactor == null)
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
            return;
        }

        pos = _interactor.TransformPoint(_offset);
        rot = _interactor.rotation * _rotOffset;

        if (_constraint != null)
        {
            _constraint.GetConstrainedMotion(ref pos, ref rot);
        }
        
    }

    public void OnPickedUp(Transform interactor)
    {

        if (interactor == null)
        {
            _interactor = null;
            return;
        }

        Debug.Log($"NetServerPhys: {gameObject.name} picked up by {interactor.name}");
        _localHeld = true;
        bool snapToController = true;
        if (_interactable != null)
            snapToController = _interactable.SnapToController;

        if (!snapToController)
        {
            _offset = Vector3.zero;
            _rotOffset = Quaternion.identity;
        }
        else
        {
            if (interactor.TryGetComponent<CustomXRInteractor>(out var xrInteractor))
            {
                transform.position = xrInteractor.CalculateHeldPosition(transform);
            }
            _offset = interactor.InverseTransformPoint(transform.position);
            _rotOffset = Quaternion.Inverse(interactor.rotation) * transform.rotation;
        }

        _interactor = interactor;
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void ThrowObject(Transform thrownBy, Vector3 velocity, Vector3 angularVelocity)
    {
        if (_interactor != null && thrownBy != _interactor)
        {
            Debug.Log($"NetServerPhys: Ignoring throw by non-matching interactor");
            return;
        }

        Debug.Log($"NetSyncServerPhysics: Throwing object {name} velocity {velocity}");

        SendThrow(velocity, angularVelocity);
        _interactor = null;
        UpdateClientPosition();
    }
}
