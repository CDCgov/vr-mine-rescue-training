using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
/// <summary>
/// Custom socket class, align an XR Grabbable 
/// </summary>
public class CustomXRSocket : XRObjectController
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public CustomXRInteractable SocketedInteractable;

    //public delegate void SocketedActivate(CustomXRSocket xrSocket);
    //public static event SocketedActivate OnSocketAttach;

    //public delegate void SocketedDeactivate(CustomXRSocket xrSocket);
    //public static event SocketedDeactivate OnSocketDetach;

    //public NetworkManager NetworkManager;
    //public NetworkedObjectManager NetworkedObjectManager;
    public SocketManager SocketManager;
    public float SocketRadius = 0.05f;
    public bool EnableObjectScan = true;
    public InteractableType AllowedInteractableTypes = InteractableType.None;
    public bool IsPlayerSocket = false;
    public bool IsFirstPersonSocket = false;
    public string SocketID = "";

    public int PlayerID = -1;

    //private bool _isObjectOriginallyKinematic = false;
    //private bool _isObjectColliderATrigger = false;
    //private bool _isColliding = false;
    //private bool _isExiting = false;
    //private bool _exitFlag = false;
    //private Transform _socketedItemTransformParent = null;
    private Transform _socketTransform = null;
    //private NetworkedObject _netObj;
    //private NetSyncSocket _netSock;
    private string _origSocketName;
    public string SocketName;
    public GameObject IndicatorPrefab;
    //public bool _setExternally = false;

    private GameObject _spawnedIndicator;
    private Material _originalMaterial;
    private bool _registered = false;
    private Guid? _objToSocket = null;

    //private List<IInteractableObject> _socketInteractInterfaces;
    private List<ISocketableObject> _socketInterfaces;

    private int _linkLineLayer;
    private Collider[] _colliders;

    private bool _overridePhysicsScene = false;
    private PhysicsScene _physicsScene;

    private void Awake()
    {
        _colliders = new Collider[128];
        //_socketInteractInterfaces = new List<IInteractableObject>();
        _socketInterfaces = new List<ISocketableObject>();

        if (SocketManager == null)
        {
            SocketManager = SocketManager.GetDefault(gameObject);
        }

        _linkLineLayer = LayerMask.NameToLayer("LinkLine");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);


        //NetworkedObject networkedParent = GetComponentInParent<NetworkedObject>();

        _socketTransform = transform;
        _origSocketName = gameObject.name;

        //_netObj = gameObject.GetComponent<NetworkedObject>();

        RegisterWithSocketManager();

        NetworkedObjectManager.ActiveObjectListChanged += OnActiveObjectListChanged;
        NetworkedObjectManager.ObjectHeldStateChanged += OnHeldStateChanged;
    }

    private void OnHeldStateChanged(Guid obj)
    {
        if (SocketedInteractable == null || SocketedInteractable.CurrentOwner != this)
            return;

        if (!SocketedInteractable.TryGetComponent<NetworkedObject>(out var netObj))
            return;

        if (!netObj.HasAuthority || netObj.uniqueID != obj)
            return;

        var objData = NetworkedObjectManager.GetObjectData(obj);
        if (objData.IsObjectHeld)
        {
            Debug.Log($"CustomXRSocket: Releasing object {SocketedInteractable.name} due to held state change");
            //remove from socket / release ownership
            SocketedInteractable.ChangeOwnership(null);
        }
    }

    private void OnActiveObjectListChanged()
    {
        if (_objToSocket != null)
        {
            InternalSocketItem((Guid)_objToSocket);
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"CustomXRSocket: {SocketID} OnDestroy");

        if (SocketedInteractable != null)
        {
            NetworkedObject networkedObject = SocketedInteractable.GetComponent<NetworkedObject>();
        }

        //if (IsFirstPersonSocket)
        //    SocketManager.UnregisterSocket(SocketID);


        NetworkedObjectManager.ActiveObjectListChanged -= OnActiveObjectListChanged;
        UnregisterSocket();
    }

    public void SetSocketID(string socketID)
    {
        if (_registered)
            UnregisterSocket();

        SocketID = socketID;
        RegisterWithSocketManager();
    }

    public override void GainedOwnership(CustomXRInteractable interactable)
    {
        if (SocketedInteractable != null)
        {
            SocketedInteractable.ChangeOwnership(null);
            SocketedInteractable = null;
        }

        SocketedInteractable = interactable;
        Debug.Log($"CustomXRSocket({this.name}): Gained ownership on {SocketedInteractable.name} socket {SocketID} scene: {gameObject.scene.name}");

        _socketInterfaces.Clear();
        SocketedInteractable.GetComponentsInChildren<ISocketableObject>(_socketInterfaces);
        SetSocketedItemParent(interactable);

        Rigidbody rb = SocketedInteractable.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        RaiseSocketed();
    }

    public void OverridePhysicsScene(PhysicsScene physScene)
    {
        _overridePhysicsScene = true;
        _physicsScene = physScene;
    }

    private void SetSocketedItemParent(CustomXRInteractable interactable)
    {

        SocketedInteractable = interactable;

        interactable.transform.SetParent(transform, true);
        interactable.transform.localPosition = Vector3.zero;
        interactable.transform.localRotation = Quaternion.identity;
    }

    private void ClearSocketedItemParent(CustomXRInteractable interactable)
    {
        if (interactable == null)
            return;

        interactable.ResetParent();
    }

    public override void LostOwnership(CustomXRInteractable interactable)
    {
        if (SocketedInteractable == null || SocketedInteractable != interactable)
            return;

        RaiseRemovedFromSocket();

        Debug.Log($"CustomXRSocket({this.name}): Lost ownership on {SocketedInteractable.name} vacating socket {SocketID} scene: {gameObject.scene.name}");
        //SocketedInteractable.OwnerChanged -= OnSocketedInteractableOwnerChanged;
        SocketedInteractable = null;

        if (SocketManager != null)
            SocketManager.RequestSocketVacate(SocketID);
    }

    private void RegisterWithSocketManager()
    {
        Debug.Log($"Registering with socket manager.({SocketID},{this.name}),@time({Time.time}");

        NetworkedObject initialSocketedNetObj = null;
        if (SocketedInteractable != null)
            SocketedInteractable.TryGetComponent<NetworkedObject>(out initialSocketedNetObj);

        SocketedInteractable = null;

        SocketManager.RegisterSocket(SocketID, this);
        _registered = true;

        //if registering the socket did not assign an object and
        //if we are the server, inform all clients of the default socket status
        //otherwise clear default socketed interactable and rely on data from server         
        if (SocketedInteractable == null && NetworkManager.IsServer && initialSocketedNetObj != null)
        {
            SocketManager.RequestSocketAssignment(SocketID, initialSocketedNetObj.uniqueID);
        }

    }

    private void UnregisterSocket()
    {
        if (SocketManager != null)
            SocketManager.UnregisterSocket(SocketID);

        _registered = false;
    }

    public void AssignSocketName(int clientID)
    {
        //Debug.Log("Assign socket name!");
        gameObject.name = SocketName + "_" + clientID;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (SocketedInteractable != null)
        {
            //Debug.Log($"CustomXRSocket: Clearing local transform on {SocketedInteractable.name}");
            SocketedInteractable.transform.localPosition = Vector3.zero;
            SocketedInteractable.transform.localRotation = Quaternion.identity;
        }

        //terrible hack
        if (gameObject.layer == _linkLineLayer)
            EnableObjectScan = false;

        //if (_netObj.HasAuthority && SocketedInteractable == null)
        if (EnableObjectScan && NetworkManager.IsServer && SocketedInteractable == null && SocketManager.EnableObjectScan)
        {
            ScanForObjects();
        }


    }

    private void ScanForObjects()
    {
        //only scan for objects if we don't have an object already socketed
        if (SocketedInteractable != null)
            return;

        //Collider[] colsInSphere = Physics.OverlapSphere(transform.position, SocketRadius);

        int numColliders = 0;
        if (_overridePhysicsScene)
        {
            numColliders = _physicsScene.OverlapSphere(transform.position, SocketRadius, _colliders, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }
        else
        {
            numColliders = Physics.OverlapSphereNonAlloc(transform.position, SocketRadius, _colliders);
        }

        CustomXRInteractable targetObj = null;
        float targetObjDist = float.MaxValue;
        Vector3 socketPos = transform.position;

        if (numColliders <= 0)
            return;

        var linkLineLayer = LayerMask.NameToLayer("LinkLine");
        List<CustomXRInteractable> itemsToMove = new List<CustomXRInteractable>();
        //foreach (Collider col in colsInSphere)
        for (int i = 0; i < numColliders; i++)
        {
            var col = _colliders[i];

            //CustomXRInteractable item = col.GetComponent<CustomXRInteractable>();
            if (col.TryGetComponent<CustomXRInteractable>(out CustomXRInteractable item)
                && (item.CurrentOwner == null)
                //&& (item.CompareTag(gameObject.tag))
                && ((item.InteractableType & AllowedInteractableTypes) > 0)
                && (item.IsGrabbable))
            {
                if (item.gameObject.layer == linkLineLayer)
                    continue;

                var itemNetObj = col.GetComponent<NetworkedObject>();
                if (itemNetObj == null)
                    continue;

                var objData = NetworkedObjectManager.GetObjectData(itemNetObj.uniqueID);
                if (objData != null && objData.HeldState != null && objData.HeldState.ObjectHeld)
                    continue;

                float dist = Vector3.Distance(item.transform.position, socketPos);
                if (targetObj == null || dist < targetObjDist)
                {
                    targetObj = item;
                    targetObjDist = dist;
                }
            }


            //Check to see if we will need to move items up
            if ((item != null) && (item.CurrentOwner == null) && (item.IsGrabbable) && !IsPlayerSocket)
            {
                itemsToMove.Add(item);
            }
        }

        if (targetObj == null)
            return;

        //Debug.Log($"CustomXRSocket: Socket {gameObject.name} found object {targetObj.gameObject.name} in range");
        //if(_interactInterfaces != null)
        //    _interactInterfaces.Clear();
        //if (targetObj != null)
        //    targetObj.GetComponentsInChildren<IInteractableObject>(_interactInterfaces);
        //ForceDeactivate();
        //NetworkedObject netObj = targetObj.GetComponent<NetworkedObject>();
        //if (netObj == null)
        if (!targetObj.TryGetComponent<NetworkedObject>(out var netObj))
        {
            return;
        }

        //if (targetObj.Lock)
        //{
        //    return;
        //}
        //targetObj.Lock = true;
        Debug.Log($"CustomXRSocket: requesting socket {SocketID} for {netObj.name} authority: {netObj.HasAuthority}");

        if (!netObj.HasAuthority)
            netObj.RequestOwnership();

        SocketManager.RequestSocketAssignment(SocketID, netObj.uniqueID);
        foreach (CustomXRInteractable item in itemsToMove)
        {
            if (item == targetObj)
                continue;

            //NetworkedObject itemNetObj = item.GetComponent<NetworkedObject>();
            //if(itemNetObj == null)
            if (!item.TryGetComponent<NetworkedObject>(out var itemNetObj))
                continue;

            if (!itemNetObj.HasAuthority)
                itemNetObj.RequestOwnership();

            Vector3 pos = item.transform.position;
            pos.y += 1;
            item.transform.position = pos;
            if (item.InteractableRigidbody != null)
            {
                if (item.InteractableRigidbody.isKinematic)
                {
                    item.InteractableRigidbody.isKinematic = false;
                }
            }
        }
        //Debug.Log("added listener " + gameObject.name);
        //if (_netSock != null)
        //{
        //    _netSock.SocketedObjToSync = SocketedInteractable.Address.ToString();
        //}
        return;

    }


    public void RequestSocketItem(CustomXRInteractable itemToSocket)
    {
        var netObj = itemToSocket.GetComponent<NetworkedObject>();
        if (netObj == null)
            return;

        SocketManager.RequestSocketAssignment(SocketID, netObj.uniqueID);
    }


    public void InternalSocketItem(System.Guid objectID)
    {
        var objData = NetworkedObjectManager.GetObjectData(objectID);
        if (objData == null || objData.AssociatedObj == null)
        {
            Debug.Log($"CustomXRSocket: Couldn't find object {objectID} to attach to {SocketID}, waiting for object");
            _objToSocket = objectID;
            return;
        }

        _objToSocket = null;

        var xrInteract = objData.AssociatedObj.GetComponent<CustomXRInteractable>();
        if (xrInteract == null)
        {
            Debug.LogError($"CustomXRSocket: Object {objectID} missing CustomXRInteractble when attempting to attach to {gameObject.name}");
            return;
        }

        InternalSocketItem(xrInteract);
    }

    public void InternalSocketItem(CustomXRInteractable itemToSocket)
    {
        if (itemToSocket == SocketedInteractable && itemToSocket.CurrentOwner == this)
        {
            return;
        }

        //SocketedInteractable = itemToSocket;
        //Debug.Log($"Socketed interactable check: {SocketedInteractable.name}");

        //only claim ownership if non-networked, or the network object has authority
        if (!itemToSocket.TryGetComponent<NetworkedObject>(out var netObj) || netObj.HasAuthority)
        {
            itemToSocket.ChangeOwnership(this);
        }
        else
        {
            itemToSocket.ChangeOwnership(null);
            SetSocketedItemParent(itemToSocket);
        }

        //SocketedInteractable.OwnerChanged += OnSocketedInteractableOwnerChanged;
    }

    //private void OnSocketedInteractableOwnerChanged(Transform obj)
    //{

    //    if (SocketedInteractable != null && SocketedInteractable.CurrentOwner != transform)
    //    {
    //        Debug.Log($"CustomXRSocket: Owner changed on {SocketedInteractable.name} vacating socket {SocketID}");
    //        SocketedInteractable.OwnerChanged -= OnSocketedInteractableOwnerChanged;
    //        SocketedInteractable = null;

    //        SocketManager.RequestSocketVacate(SocketID);
    //    }
    //}


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SocketRadius);
    }

    public void RequestRemoveSocketedItem()
    {
        if (SocketedInteractable != null)
            SocketManager.RequestSocketVacate(SocketID);
    }

    public void InternalRemoveSocketedItem()
    {
        if (SocketedInteractable == null)
            return;


        Debug.Log($"CustomXRSocket: {SocketID} removed object {SocketedInteractable.name}");
        //Collider col = SocketedInteractable.GetComponent<Collider>();
        //if (col != null)
        //    SocketedInteractable.GetComponent<Collider>().isTrigger = false;


        try
        {
            //_socketInteractInterfaces.Clear();
            //SocketedInteractable.GetComponentsInChildren<IInteractableObject>(_socketInteractInterfaces);
            //foreach (var iface in _socketInteractInterfaces)
            //    iface?.OnDropped(transform);
            //SocketedInteractable.onSelectExit?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in onSelectExit event handler: {ex.Message} :: {ex.StackTrace}");
        }
        //SocketedInteractable.OwnerChanged -= OnSocketedInteractableOwnerChanged;

        var netObj = SocketedInteractable.GetComponent<NetworkedObject>();
        
        if (SocketedInteractable.CurrentOwner == this || netObj == null || netObj.HasAuthority)
        {
            if (SocketedInteractable.TryGetComponent<ISocketableObject>(out var isock))
            {
                isock.OnRemovedFromSocket(this);
            }
        }
        else
        {
            //clear socketed item parent
            Debug.Log($"CustomXRSocket: Clearing socketed item parent: {SocketedInteractable.name}");
            ClearSocketedItemParent(SocketedInteractable);
        }

        if (SocketedInteractable.CurrentOwner == this)
            SocketedInteractable.ChangeOwnership(null);

        SocketedInteractable = null;


    }

    public void SpawnIndicator()
    {
        if (IndicatorPrefab == null)
            return;

        if (_spawnedIndicator != null)
            DestroyIndicator();

        _spawnedIndicator = GameObject.Instantiate(IndicatorPrefab, transform.position, transform.rotation, transform);
        if (_spawnedIndicator == null)
        {
            Debug.Log($"Indicator failed to spawn on {gameObject.name}");
        }
        else
        {
            Debug.Log($"Spawned indicator {_spawnedIndicator.name} on {gameObject.name} at position {_spawnedIndicator.transform.position}");
        }
    }

    public void DestroyIndicator()
    {
        if (_spawnedIndicator == null)
        {
            return;
        }
        Destroy(_spawnedIndicator);
        _spawnedIndicator = null;
    }

    public void ChangeIndicatorColor(bool highlight)
    {
        if (_spawnedIndicator == null)
        {
            return;
        }
        ChangeIndicatorColor chngCol = _spawnedIndicator.GetComponent<ChangeIndicatorColor>();
        if (highlight)
        {
            chngCol.Highlight();
        }
        else
        {
            chngCol.Restore();
        }
    }

    private void RaiseSocketed()
    {
        if (_socketInterfaces == null)
            return;

        foreach (var iface in AllSocketInterfaces())
            iface.OnSocketed(this);
    }

    private void RaiseRemovedFromSocket()
    {
        if (_socketInterfaces == null)
            return;

        foreach (var iface in AllSocketInterfaces())
            iface.OnRemovedFromSocket(this);
    }

    private IEnumerable<ISocketableObject> AllSocketInterfaces()
    {
        return Util.GetAllInterfaces<ISocketableObject>(_socketInterfaces);
        //int numInterfaces = _socketInterfaces.Count;
        //for (int i = 0; i < numInterfaces; i++)
        //{
        //    yield return _socketInterfaces[i];

        //    //check the interfaces haven't changed
        //    if (numInterfaces != _socketInterfaces.Count)
        //        break;
        //}
    }

    //private void ForceDeactivate()
    //{
    //    try
    //    {
    //        foreach (var iface in AllInteractInterfaces())
    //            iface?.OnDeactivated(transform);
    //    }
    //    catch (Exception ex) { Debug.LogError($"Error in OnDeactivated {ex.Message} {ex.StackTrace}"); }
    //}
}
