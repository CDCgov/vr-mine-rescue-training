using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.AddressableAssets;
using Google.Protobuf;

public class NetworkedObject : MonoBehaviour, ISelectableObjectInfo, ISelectableObjectAction
{

    public enum NetOwnershipState
    {
        OwnedBySelf,
        OwnershipRequested,
        OwnedByOther
    }

    public delegate void NetObjMessageHandler(string messageType, CodedInputStream reader);

    [NonSerialized]
    public bool Initialized = false;
    
    public bool HasAuthority
    {
        get
        {
            if (OwnershipState == NetOwnershipState.OwnedByOther)
                return false;
            else
                return true;
        }
    }

    [NonSerialized]
    public NetOwnershipState OwnershipState;
    public bool AllowNetDestroy = true;
    //public bool AutomaticSpawn = false;
    public string UniqueIDString = Guid.NewGuid().ToString();
    public bool UseReliableSync = false;
    public bool SyncChildComponents = true;

    //[NonSerialized]
    public bool SpawnActivated = false;
    public bool ServerExclusive = false; //prevent clients taking ownership


    [NonSerialized]
    public Guid uniqueID;
    //public uint NetID = 0;


    [NonSerialized]
    public short OwnerClientID = 0;
    //public AssetReference OriginalObject;
    public string SourcePrefab;
    public int SceneID = -1;

    public INetSync[] _netSyncInterfaces;

    public NetworkManager NetManager;
    public NetworkedObjectManager NetworkedObjectManager;

    private bool _spawnSent = false;
    private long _lastSyncTick = -1;
    private float _lastSyncTime = -1;
    private float _createTime = -1;

    private List<NetObjMessageHandler> _messageHandlers = new List<NetObjMessageHandler>();

    public long LastSyncTick
    {
        get { return _lastSyncTick; }
    }

    public float LastSyncTime
    {
        get { return _lastSyncTime; }
    }

    public void SendMessage(string messageType, IMessage msg)
    {
        NetManager.SendNetObjMessage(uniqueID, messageType, msg);
    }

    //public void HandleMessage(string messageType, byte[] data, int offset, int length)
    public void HandleMessage(string messageType, CodedInputStream reader)
    {
        //var streamPos = reader.Position;

        foreach (var handler in _messageHandlers)
        {
            //TODO: Right now only one handler can read from the input stream
            //Either change to recreate / reset the CodedInputStrema position or 
            //use a single IMessage for object messages (e.g. using oneof)

            //reader.Seek(streamPos, SeekOrigin.Begin);
            //var reader = new CodedInputStream(data, offset, length);
            handler(messageType, reader);

        }
    }

    public void RegisterMessageHandler(NetObjMessageHandler handler)
    {
        _messageHandlers.Add(handler);
    }

    public void UnregisterMessageHandler(NetObjMessageHandler handler)
    {
        _messageHandlers.Remove(handler);
    }

    public bool NeedsUpdate()
    {
        if (_netSyncInterfaces == null || _netSyncInterfaces.Length <= 0)
            return false;

        for (int i = 0; i < _netSyncInterfaces.Length; i++)
        {
            if (_netSyncInterfaces[i].NeedsUpdate())
                return true;
        }

        return false;
    }

    public void SyncObjState(CodedInputStream reader, Int64 serverTick, float serverTime)
    {
        if (!Initialized)
        {
            Debug.Log($"Received sync and initialized object {gameObject.name}");
            gameObject.SetActive(true);
            Initialized = true;
        }

        //if (_lastSyncTick > serverTick)
        //    return;

        _lastSyncTick = serverTick;
        _lastSyncTime = Time.unscaledTime;

        if (OwnershipState != NetOwnershipState.OwnedByOther)
        {
            //ignore updates if we are trying to control this object
            return;
        }

        if (_netSyncInterfaces == null)
            return;

        for (int i = 0; i < _netSyncInterfaces.Length; i++)
        {
            _netSyncInterfaces[i].SyncObjState(reader);
        }
    }

    public void ForceSyncObjState(CodedInputStream reader)
    {
        if (!Initialized)
        {
            Debug.Log($"Received sync and initialized object {gameObject.name}");
            gameObject.SetActive(true);
            Initialized = true;
        }

        if (_netSyncInterfaces == null)
            return;

        for (int i = 0; i < _netSyncInterfaces.Length; i++)
        {
            _netSyncInterfaces[i].SyncObjState(reader);
        }
    }

    public void SetOwner(short ownerID)
    {
        OwnerClientID = ownerID;
        //_lastSyncTime = Time.unscaledTime - 10;

        //if net manager is null, let Start() handle authority setup
        if (NetManager == null)
            return;

        if (ownerID == NetManager.ClientID)
        {
            //HasAuthority = true;
            OwnershipState = NetOwnershipState.OwnedBySelf;
        }
        else
        {
            //HasAuthority = false;
            OwnershipState = NetOwnershipState.OwnedByOther;
        }
    }

    public bool RequestOwnership()
    {
        if (ServerExclusive && !NetManager.IsServer)
            return false;

        OwnershipState = NetOwnershipState.OwnershipRequested;
        NetworkedObjectManager.RequestOwnership(this);
        //_lastSyncTime = Time.unscaledTime - 10;

        return true;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_netSyncInterfaces == null)
            return;

        _lastSyncTime = Time.unscaledTime;

        for (int i = 0; i < _netSyncInterfaces.Length; i++)
        {
            _netSyncInterfaces[i].WriteObjState(writer);
        }
    }

    void Awake()
    {
        if (SyncChildComponents)
            _netSyncInterfaces = GetComponentsInChildren<INetSync>();
        else
            _netSyncInterfaces = GetComponents<INetSync>();

        uniqueID = Guid.Parse(UniqueIDString);
    }

    //void OnClientIDAssigned(int playerID)
    //{
    //    if (gameObject == null)
    //        return;

    //    //if (AutomaticSpawn)
    //    //{
    //    //    Debug.Log($"NetObj: AutoSpawn {name}:{uniqueID} in OnClientIDAssigned");
    //    //    NetManager.SendSpawnObject(SourcePrefab, uniqueID, transform.position, transform.rotation, true);
    //    //}
    //}

    void Start()
    {
        _createTime = Time.time;
        //Debug.Log("NetObject: Start");
        if (NetManager == null)
        {
            NetManager = NetworkManager.GetDefault(gameObject);
        }
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        _lastSyncTime = -1;

        //NetManager.ClientIDAssigned += OnClientIDAssigned;

        //if (AutomaticSpawn)
        //{
        //    uniqueID = Guid.NewGuid();
        //    Initialized = true;
        //    //HasAuthority = true;
        //    OwnerClientID = NetManager.ClientID;
        //    OwnershipState = NetOwnershipState.OwnedBySelf;

        //    Debug.Log($"NetObj: AutoSpawn {name}:{uniqueID} in Start");
        //    NetManager.SendSpawnObject(SourcePrefab, uniqueID, transform.position, transform.rotation, true);
        //}
        if (NetManager.CheckObjectDestroyed(uniqueID))
        {
            //we were destroyed sometime between creation and Start() being called
            Destroy(gameObject);
            return;
        }
        else if (OwnerClientID == NetManager.ClientID)
        {
            Initialized = true;
            //HasAuthority = true;
            OwnerClientID = (short)NetManager.ClientID;
            OwnershipState = NetOwnershipState.OwnedBySelf;
        }
        else
        {
            gameObject.SetActive(false);
            Initialized = false;
            //HasAuthority = false;
            OwnershipState = NetOwnershipState.OwnedByOther;
        }

        if (SourcePrefab == null || SourcePrefab.Length <= 0)
            AllowNetDestroy = false; //don't allow destroy on scene objects

        NetworkedObjectManager.RegisterNetworkedObject(this);

        //if SpawnActivated is true, we have been spawned by a network call and should activate regardless of authority
        if (SpawnActivated)
        {
            gameObject.SetActive(true);
        }

        //Debug.LogFormat("InstanceID: {0}", GetInstanceID().ToString());
        //MasterControl.SceneControl.AddSceneObject(GetInstanceID(), this);


        //if (!Initialized && !NetworkManager.IsMasterClient)
        //{
        //	//deactivate if we aren't the master client - the master client should claim this objects authority and initialize it
        //	gameObject.SetActive(false);
        //	return;
        //}
        //else if (!Initialized)
        //{
        //	//NetworkManager.ClaimSceneObject(gameObject);
        //}
    }

    private void Update()
    {
        //if (!HasAuthority)
        //{
        //    float delta = Time.time - _lastSyncTime;
        //    if (delta > 5)
        //    {
        //        Debug.LogWarning($"NetObj: Disabling object due to lack of sync messages: {gameObject.name}");
        //        gameObject.SetActive(false);
        //    }
        //}
    }

    void OnDestroy()
    {
        if (NetManager != null && NetworkedObjectManager != null)
        {
            //NetManager.ClientIDAssigned -= OnClientIDAssigned;

            if (HasAuthority && AllowNetDestroy)
            {
                Debug.Log($"NetObj: Sending destroy for {name}:{uniqueID}");
                //NetManager.SendDestroyObject(uniqueID);
                NetworkedObjectManager.DestroyObject(uniqueID);
            }
            NetworkedObjectManager.UnregisterNetworkedObject(this);
        }
        else
        {
            if (_createTime < 0)
            {
                Debug.Log($"Networked Object '{gameObject.name}' destroyed with no NetworkManager active. Start not called.");
            }
            else
            {
                float elapsed = Time.time - _createTime;                
                Debug.LogWarning($"Networked Object '{gameObject.name}' destroyed with no NetworkManager active. Lifetime: {elapsed:F1}s ");
            }
        }
    }

    public void GetObjectInfo(System.Text.StringBuilder sb)
    {

        sb.AppendLine($"ID: {uniqueID}");
        sb.AppendLine($"Name: {gameObject.name}");
        sb.AppendLine($"Owner ID : {OwnerClientID}");
        sb.AppendLine($"Scene ID : {SceneID}");
        sb.AppendLine($"Has Authority: {HasAuthority}");
        sb.AppendLine($"Source Prefab: {SourcePrefab}");

        var objData = NetworkedObjectManager.GetObjectData(uniqueID);
        if (objData == null)
            sb.AppendLine($"Object not registered in manager!");
        else
        {
            sb.AppendLine($"Owner Exclusive: {objData.OwnerExclusive}");
            //sb.AppendLine($"Held: {objData.HeldState.}")
        }
    }

    public string SelectableActionName { get { return "Destroy"; } }

    public void PerformSelectableObjectAction()
    {
        NetworkedObjectManager.DestroyObject(uniqueID);
        NetManager.LogSessionEvent(VRNLogEventType.Dmaction, "DM Destroyed Object", transform.position, transform.rotation, gameObject.name, VRNLogObjectType.Unknown, null);
    }
}