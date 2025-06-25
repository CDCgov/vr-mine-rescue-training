using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomXRInteractable))]
[RequireComponent(typeof(NetworkedObject))]
public class ReturnToSocket : MonoBehaviour, ISocketableObject
{
    public SocketManager SocketManager;
    public NetworkedObjectManager NetworkedObjectManager;

    private CustomXRInteractable _xrInteract;
    private NetworkedObject _netObj;
    private NetworkedObjectManager.NetObjData _netObjData = null;
    private Rigidbody _rb;
    private CustomXRSocket _returnSocket;
    private Collider _collider;

    private void Start()
    {
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);

        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        _xrInteract = GetComponent<CustomXRInteractable>();
        _netObj = GetComponent<NetworkedObject>();
        _rb = GetComponent<Rigidbody>();

        _returnSocket = _xrInteract.CurrentOwner as CustomXRSocket;
        _xrInteract.OwnerChanged += OnOwnerChanged;


        _collider = GetComponentInChildren<Collider>();
        //_xrInteract.onSelectEnter.AddListener(OnSelectEnter);
        //_xrInteract.onSelectExit.AddListener(OnSelectExit);
        if (_netObj != null)
            _netObjData = NetworkedObjectManager.GetObjectData(_netObj.uniqueID);
    }

    private void OnDestroy()
    {
        if (_xrInteract != null)
            _xrInteract.OwnerChanged -= OnOwnerChanged;
    }

    //private void OnSelectExit()
    //{
    //    Debug.Log($"Return To Socket: {gameObject.name} Dropped");
    //    ObjectDropped();
    //}

    //private void OnSelectEnter()
    //{
    //    Debug.Log($"Return To Socket: {gameObject.name} Picked Up");
    //    ObjectPickedUp();
    //}

    private void OnOwnerChanged(XRObjectController obj)
    {
        if (_returnSocket == null)
            _returnSocket = obj as CustomXRSocket;


        else if (obj == null) 
        {
            ObjectDropped();
        }

    }

    private void ObjectDropped()
    {
        if (!_netObj.HasAuthority)
            return;

        if (_returnSocket != null)
        {
            SocketManager.RequestSocketAssignment(_returnSocket.SocketID, _netObj.uniqueID);
        }
    }

    private void ObjectPickedUp()
    {

    }

    public void OnSocketed(CustomXRSocket socket)
    {
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
    }

    public void OnRemovedFromSocket(CustomXRSocket socket)
    {
        if(_collider != null)
        {
            _collider.isTrigger = false;
        }
    }

    private void Update()
    {
        if (_netObj != null && _netObjData != null && 
            !_netObj.HasAuthority && _collider != null && _netObjData.HeldState != null)
        {
            if (_collider.isTrigger != !_netObjData.HeldState.ObjectHeld)
                _collider.isTrigger = !_netObjData.HeldState.ObjectHeld;
        }
    }

    /*
    public Transform SocketTransform;
    public Rigidbody ObjectRigidBody;
    public string SocketName;
    public int SocketID = 0;
    public float SocketMoveMultiplier = 4;
    public bool AllowOtherToGrab = false;

    private bool _enableReturn = false;
    private bool _objectHeld = false;  
    private Vector3 _v3StartPosition;
    private float _lerpValue;
    private NetworkManager _netMan;
    private NetworkedObject _netObj;
    private string id;
    private NetSyncGrabState _netGrab;
    private NetSyncTransform _netSyncTransform;
    private NetSyncGrabInteractable _netGrabInteractable;
    private NetSyncID _netID;
    private NetSyncSocketTarget _netTarget;
    private string _targetCache = "";
    public CustomXRInteractable _custXRInteractable;
    public CustomXRSocket _targetedSocket;
    private string _origSocketName;
    private bool _initialRemoteSpawn = true;

    /*
    private void Start()
    {
        //if(SocketTransform == null)
        //{
        //    SocketTransform = GameObject.Find(SocketName).transform;
        //    if(transform.parent == null)
        //    {
        //        transform.parent = SocketTransform;
        //    }
        //}
        _origSocketName = SocketName;
        if (_netMan == null)
            _netMan = NetworkManager.GetDefault();
        //if(_netMan != null)
        //{            
        //    id = _netMan.GetClientID().ToString();
        //    SocketName = SocketName + "_" + id;
        //}
        //_netMan.ClientIDAssigned += ClientIDAssigned;
        if (_netObj == null)
        {
            _netObj = GetComponent<NetworkedObject>();
        }
        //SocketName = _origSocketName + "_" + _netObj.OwnerClientID;
        _netID = GetComponent<NetSyncID>();
        _netGrab = GetComponent<NetSyncGrabState>();
        if(_netGrab == null)
        {
            _netGrab = gameObject.AddComponent<NetSyncGrabState>();
        }
        if(_netSyncTransform == null)
        {
            _netSyncTransform = gameObject.GetComponent<NetSyncTransform>();
        }
        if(_netTarget == null)
        {
            _netTarget = gameObject.GetComponent<NetSyncSocketTarget>();
        }
        if(_netGrabInteractable == null)
        {
            _netGrabInteractable = gameObject.GetComponent<NetSyncGrabInteractable>();
        }

        //if (SocketTransform == null && SocketName != null)
        //{
        //    GameObject sock = GameObject.Find(SocketName);
        //    if(sock == null)
        //    {
        //        return;
        //    }
        //    SocketTransform = sock.transform;
        //    if (SocketTransform != null)
        //    {
        //        transform.parent = SocketTransform;
        //    }
        //}

        if(_custXRInteractable == null)
        {
            _custXRInteractable = gameObject.GetComponent<CustomXRInteractable>();
        }

        if(_custXRInteractable != null)
        {
            if (AllowOtherToGrab)
            {
                _custXRInteractable.IsGrabbable = true;
            }

            //_custXRInteractable.onSelectEnter.AddListener(Hel)
        }
    }

    private void Update()
    {
        return;
        //if (ObjectRigidBody.isKinematic)
        //{
        //    _enableReturn = true;
        //    _v3StartPosition = transform.position;
        //    _lerpValue = 0;
        //    return;
        //}
        if (_custXRInteractable == null)
        {
            _custXRInteractable = gameObject.GetComponent<CustomXRInteractable>();
        }

        //if (_netID != null)
        //{
        //    if (SocketID != _netID.ID)
        //    {
        //        SocketID = _netID.ID;
        //        SocketName = _origSocketName + "_" + SocketID;
        //        GameObject sock = GameObject.Find(SocketName);
        //        if (sock == null)
        //        {
        //            return;
        //        }
        //        SocketTransform = sock.transform;
        //        transform.SetParent(SocketTransform);
        //        _custXRInteractable.SetPositionForSnap();
        //        if (!AllowOtherToGrab)
        //        {
        //            _netSyncTransform.enabled = false;
        //        }
        //        else
        //        {
        //            _netGrabInteractable.LockPosition = true;
        //        }
        //        _netGrab.Grabbed = false;
        //        Debug.Log("Changing socket ID frame: " + Time.frameCount);
        //    }
        //}
        //if (!_netObj.HasAuthority)
        //{
        //    if (_netTarget != null)
        //    {
        //        if (SocketName != _netTarget.TargetToSync || SocketTransform == null)
        //        {
        //            SocketName = _netTarget.TargetToSync;
        //            GameObject sock = GameObject.Find(SocketName);
        //            if (sock == null)
        //            {
        //                //Debug.Log("Could not find socket in remote client! Attempting again next frame");
        //                return;
        //            }
        //            SocketTransform = sock.transform;
        //            transform.SetParent(SocketTransform);
        //            _custXRInteractable.SetPositionForSnap();
        //            if (!AllowOtherToGrab)
        //            {
        //                _netSyncTransform.enabled = false;
        //            }
        //            else
        //            {
        //                _netGrabInteractable.LockPosition = true;
        //            }
        //            _netGrab.Grabbed = false;
        //            Debug.Log("Changing socket ID frame: " + Time.frameCount);
        //        }
        //    }
        //}
        //else
        //{
        //    _netID.ID = _netObj.OwnerClientID;
        //    if(SocketID != _netID.ID)
        //    {
        //        SocketID = _netID.ID;
        //    }
        //    if (_netTarget != null)
        //    {
        //        if (_netTarget.TargetToSync == "")
        //        {
        //            _netTarget.TargetToSync = _origSocketName + "_" + SocketID;
        //        }
        //    }
        //}
        if (SocketTransform == null)
        {
            return;
             
        }
        else
        {
            if (_netObj != null && !_netObj.HasAuthority)
            {                
                if (!_netGrab.Grabbed)
                {
                    //transform.localPosition = Vector3.zero;
                    //transform.localEulerAngles = Vector3.zero;
                    if (!AllowOtherToGrab)
                    {
                        _netSyncTransform.enabled = false;
                    }
                    else
                    {
                        _netGrabInteractable.LockPosition = true;
                    }
                    _custXRInteractable.SetPositionForSnap();
                    //Debug.Log("Disabled interactable?");
                    
                }
                else
                {
                    if (!AllowOtherToGrab)
                    {
                        _netSyncTransform.enabled = true;
                    }
                    else
                    {
                        _netGrabInteractable.LockPosition = false;
                    }
                    //Debug.Log("Enabled interactable?");
                }
                return;
            }
            

            if (_enableReturn)
            {

                transform.position = Vector3.MoveTowards(transform.position, SocketTransform.position, Time.deltaTime * SocketMoveMultiplier);
                if (Vector3.Distance(transform.position, SocketTransform.position) < 0.01f)
                {
                    //_custXRInteractable.ChangeOwnership(SocketTransform.GetComponent<CustomXRSocket>(), false);
                    //_custXRInteractable.transform.SetParent(SocketTransform);
                    //_custXRInteractable.SetPositionForSnap();
                    CustomXRSocket customXRSocket = SocketTransform.GetComponent<CustomXRSocket>();
                    customXRSocket.SocketItem(_custXRInteractable);
                    _netGrab.Grabbed = false;
                    _netGrab.Grabbed = false;
                    _enableReturn = false;
                    _objectHeld = false;
                }
                else if (Vector3.Distance(transform.position, SocketTransform.position) > 2f)
                {
                    //_custXRInteractable.ChangeOwnership(SocketTransform.GetComponent<CustomXRSocket>(), false);
                    //_custXRInteractable.transform.SetParent(SocketTransform);
                    //_custXRInteractable.SetPositionForSnap();
                    CustomXRSocket customXRSocket = SocketTransform.GetComponent<CustomXRSocket>();
                    customXRSocket.SocketItem(_custXRInteractable);
                    _netGrab.Grabbed = false;
                    _enableReturn = false;
                    _objectHeld = false;
                }
            }
            if (!_objectHeld)
            {
                _custXRInteractable.SetPositionForSnap();
            }
        }
    }

    //void ClientIDAssigned(int id)
    //{
    //    if (SocketTransform == null)
    //    {
    //        SocketName = _origSocketName + "_" + id;
    //        GameObject sock = GameObject.Find(SocketName);
    //        if (sock == null)
    //        {
    //            return;
    //        }
    //        SocketTransform = sock.transform;
    //        if (SocketTransform != null)
    //        {
    //            transform.parent = SocketTransform;
    //        }
    //        else
    //        {
    //            return;
    //        }
    //    }
    //}

    public void SetNetID(int id)
    {
        if(_netID != null)
        {
            _netID.ID = id;
        }
    }
    //private void OnDestroy()
    //{
    //    _netMan.ClientIDAssigned -= ClientIDAssigned;
    //}
    

    public void ObjectHeld()
    {        
        _enableReturn = false;
        _objectHeld = true;
        
        if (_netGrab == null)
        {
            _netGrab = GetComponent<NetSyncGrabState>();
            if (_netGrab == null)
            {
                return;
            }
        }
        if(_custXRInteractable.CurrentOwner == null)
        {
            return;
        }

        CustomXRSocket customXRSocket = _custXRInteractable.CurrentOwner.GetComponent<CustomXRSocket>();
        if(customXRSocket != null)
        {
            _targetedSocket = customXRSocket;
            _netGrab.Grabbed = false;
            return;
        }
        if (_custXRInteractable.CurrentOwner.Controller == ControllerType.None)
        {
            return;
        }
        _netGrab.Grabbed = true;
        Debug.Log("Object held called");
    }

    public void ObjectDrop()
    {
        _enableReturn = true;
        _lerpValue = 0;
        if (_netGrab == null)
        {
            _netGrab = GetComponent<NetSyncGrabState>();
            if (_netGrab == null)
            {
                return;
            }
        }
        _netGrab.Grabbed = false;
        
    }


    public void OnSelect()
    {
        if(_custXRInteractable == null)
        {
            return;
        }
        if(_custXRInteractable.CurrentOwner == null)
        {
            return;
        }
        if (_netObj != null)
        {
            if (!_netObj.HasAuthority)
            {
                return;
            }
        }
        Debug.Log("Did it even get this far: " + _custXRInteractable.CurrentOwner.name);
        if (_custXRInteractable.CurrentOwner.Controller == ControllerType.None)
        {
            Debug.Log("Did it even get this far2: " + _custXRInteractable.CurrentOwner.name + "(curOwn), " + SocketTransform.name + "(socName)");
            if (_custXRInteractable.CurrentOwner.transform != SocketTransform)
            {
                Debug.Log("Did it even get this far3");
                CustomXRSocket cSock = SocketTransform.GetComponent<CustomXRSocket>();
                cSock.IsActive = true;
                SocketTransform = _custXRInteractable.CurrentOwner.transform;
                CustomXRSocket freschSock = SocketTransform.GetComponent<CustomXRSocket>();
                freschSock.IsActive = false;
                SocketName = freschSock.name + "_" + SocketID;
                _origSocketName = freschSock.name;
                if(transform.parent != freschSock.transform)
                {
                    transform.parent = freschSock.transform;
                }
                //Well this works, TODO: update the target socket across the network. yay.
                if (_netTarget != null)
                {
                    _netTarget.TargetToSync = SocketName;
                }
            }
        }
    }*/
}
