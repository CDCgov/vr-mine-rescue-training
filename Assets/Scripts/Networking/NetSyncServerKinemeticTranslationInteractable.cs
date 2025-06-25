using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constrained movement along linear axis between two points
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkedObject))]
public class NetSyncServerKinemeticTranslationInteractable : MonoBehaviour, INetSync, IInteractableObject
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;

    public Vector3 PointA;
    public Vector3 PointB;
    public Transform Reference;

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
    private ComponentInfo_Rigidbody _componentInfoRB;

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

        _netObj = GetComponent<NetworkedObject>();
        _rb = GetComponent<Rigidbody>();
        _interactable = GetComponent<CustomXRInteractable>();

        _componentInfoRB = GetComponentInChildren<ComponentInfo_Rigidbody>();

        _netObj.RegisterMessageHandler(OnNetObjMessage);

        //if (NetworkManager.IsServer)
        //{
        //    if (_componentInfoRB != null)
        //    {
        //        if (_componentInfoRB.physicsOn)
        //        {
        //            _rb.isKinematic = false;
        //        }
        //        else
        //        {
        //            _rb.isKinematic = true;
        //        }
        //    }
        //    else
        //    {
        //        _rb.isKinematic = false;
        //    }
        //}
        //else
        //{
        //    _rb.isKinematic = true;
        //    //DisableColliders();
        //}
        _rb.isKinematic = true;
    }

    void OnDestroy()
    {
        if (_netObj != null)
            _netObj.UnregisterMessageHandler(OnNetObjMessage);
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (NetworkManager == null || !NetworkManager.IsServer)
            return;

        _vrnPhysicsRequest.Position.ResetData();
        _vrnPhysicsRequest.Rotation.ResetData();
        _vrnPhysicsRequest.Velocity.ResetData();
        _vrnPhysicsRequest.AngularVelocity.ResetData();
        _vrnPhysicsRequest.UseGravity = false;
        _vrnPhysicsRequest.IsKinematic = false;

        switch (messageType)
        {
            case "K_REQ":
                reader.ReadMessage(_vrnPhysicsRequest);
                ProcessPositionSyncRequest();
                break;
            case "K_THROW":
                reader.ReadMessage(_vrnPhysicsRequest);
                ProcessK_THROW();
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

        CalculateHeldPosition(out var pos);

        pos.CopyTo(_vrnPhysicsRequest.Position);
        //rot.CopyTo(_vrnPhysicsRequest.Rotation);

        if (NetworkManager.IsServer)
        {
            ProcessPositionSyncRequest();
        }
        else
        {
            _netObj.SendMessage("K_REQ", _vrnPhysicsRequest);
        }
    }

    void SendK_THROW(Vector3 velocity, Vector3 angularVelocity)
    {
        _vrnPhysicsRequest.Velocity.ResetData();
        _vrnPhysicsRequest.AngularVelocity.ResetData();
        _vrnPhysicsRequest.UseGravity = false;

        if (_interactor != null)
        {
            CalculateHeldPosition(out var pos);
            pos.CopyTo(_vrnPhysicsRequest.Position);
            //rot.CopyTo(_vrnPhysicsRequest.Rotation);
        }

        velocity.CopyTo(_vrnPhysicsRequest.Velocity);
        angularVelocity.CopyTo(_vrnPhysicsRequest.AngularVelocity);

        _vrnPhysicsRequest.UseGravity = true;

        Debug.Log($"NetServerPhys: Sending K_THROW {_vrnPhysicsRequest.Velocity} {_vrnPhysicsRequest.AngularVelocity}");

        if (NetworkManager.IsServer)
        {
            ProcessK_THROW();
        }
        else
        {
            _netObj.SendMessage("K_THROW", _vrnPhysicsRequest);
        }

    }

    void ProcessPositionSyncRequest()
    {
        if (!NetworkManager.IsServer)
            return;

        _rb.isKinematic = true;
        transform.position = _vrnPhysicsRequest.Position.ToVector3();
        transform.rotation = _vrnPhysicsRequest.Rotation.ToQuaternion();
    }

    void ProcessK_THROW()
    {
        if (!NetworkManager.IsServer)
            return;

        Debug.Log($"NetServerPhys: Received K_THROW {_vrnPhysicsRequest.Velocity} {_vrnPhysicsRequest.AngularVelocity}");

        _rb.isKinematic = false;
        _rb.useGravity = true;
        transform.position = _vrnPhysicsRequest.Position.ToVector3();
        transform.rotation = _vrnPhysicsRequest.Rotation.ToQuaternion();
        _rb.velocity = _vrnPhysicsRequest.Velocity.ToVector3();
        _rb.angularVelocity = _vrnPhysicsRequest.AngularVelocity.ToVector3();
    }

    private void UpdateClientPosition()
    {
        if (NetworkManager == null || NetworkManager.IsServer)
            return;

        if (_interactor != null)
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
        if (_interactor != null)
        {
            //while being held use local position data
            CalculateHeldPosition(out var pos);
            transform.position = pos;
            //transform.rotation = rot;
        }
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
        if (interactor != null && interactor != _interactor)
            return; //don't drop if a previously active interactor lets go

        _interactor = null;

        Debug.Log($"NetServerPhys: {gameObject.name} dropped");
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    private void CalculateHeldPosition(out Vector3 pos)
    {
        if (_interactor == null)
        {
            pos = Vector3.zero;
            //rot = Quaternion.identity;
            return;
        }
        //Vector3 position = _interactor.TransformPoint(_offset);
        Vector3 position = _interactor.position;
        //pos = _interactor.TransformPoint(_offset);
        Vector3 worldA = transform.TransformPoint(PointA);
        Vector3 worldB = transform.TransformPoint(PointB);
        if(Reference != null)
        {
            worldA = Reference.TransformPoint(PointA);
            worldB = Reference.TransformPoint(PointB);
        }
        else
        {
            worldA = PointA;
            worldB = PointB;
        }
        //pos = Vector3.Project((position - worldA), (worldB - worldA)) + worldA;
        pos = ClosestPointOnLine(worldA, worldB, position);
        //rot = _interactor.rotation * _rotOffset;
    }

    public void OnPickedUp(Transform interactor)
    {

        if (interactor == null)
        {
            _interactor = null;
            return;
        }

        Debug.Log($"NetServerPhys: {gameObject.name} picked up by {interactor.name}");

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

    public void K_THROWObject(Transform K_THROWnBy, Vector3 velocity, Vector3 angularVelocity)
    {
        if (_interactor != null && K_THROWnBy != _interactor)
            return;

        SendK_THROW(velocity, angularVelocity);
        _interactor = null;
    }

    Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
    {
        var vVector1 = vPoint - vA;
        var vVector2 = (vB - vA).normalized;

        var d = Vector3.Distance(vA, vB);
        var t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0)
            return vA;

        if (t >= d)
            return vB;

        var vVector3 = vVector2 * t;

        var vClosestPoint = vA + vVector3;

        return vClosestPoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 worldA = transform.TransformPoint(PointA);
        Vector3 worldB = transform.TransformPoint(PointB);
        if (Reference != null)
        {
            worldA = Reference.TransformPoint(PointA);
            worldB = Reference.TransformPoint(PointB);
        }
        else
        {
            worldA = PointA;
            worldB = PointB;
        }
        Gizmos.DrawSphere(worldA, 0.01f);
        Gizmos.DrawSphere(worldB, 0.01f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(worldA, worldB);
    }
}
