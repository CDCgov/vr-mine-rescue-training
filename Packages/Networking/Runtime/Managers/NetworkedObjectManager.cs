//using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Google.Protobuf;
using System;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class NetworkedObjectManager : SceneManagerBase
{
    public class NetObjData
    {
        public NetworkedObject AssociatedObj;
        public int SceneID;
        public bool LoadInProgress = false;
        public VRNSpawnObject SpawnObjData;
        public VRNObjectHeldState HeldState;
        /// <summary>
        /// true if the object exclusively belongs to its owner
        /// e.g. objects that should be destroyed if the player leaves
        /// </summary>
        public bool OwnerExclusive = false;

        public bool IsSceneObject
        {
            get
            {
                return (SpawnObjData == null || SceneID < 0);
            }
        }

        public bool IsObjectHeld
        {
            get
            {
                if (HeldState != null)
                    return HeldState.ObjectHeld;

                return false;
            }
        }
    }

    private struct SpawnRequestData
    {
        public Guid ObjectID;
        public TaskCompletionSource<NetworkedObject> TCS;
    }

    public static NetworkedObjectManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<NetworkedObjectManager>("NetworkedObjectManager");
    }


    public NetworkManager NetworkManager;
    //public SceneLoadManager SceneLoadManager;

    public event Action ActiveObjectListChanged;
    public event Action<Guid> ObjectHeldStateChanged;

    public int NetworkedObjectCount
    {
        get 
        {
            if (_netObjects == null)
                return 0;

            return _netObjects.Count;
        }
    }

    private Dictionary<System.Guid, NetObjData> _netObjects;
    private MemoryStream _msgBuffer;
    private CodedOutputStream _codedMsgBuffer;
    private HashSet<Guid> _deleteSet;
    private HashSet<Guid> _activeSet;
    private List<Guid> _destroyedSceneObjects;

    private List<SpawnRequestData> _spawnRequests;

    private float _worldStateSendTime = -1;
    //private VRNObjectHeldState _heldStatus = new VRNObjectHeldState();

    private byte[] _netObjMessageBuffer;

    private void Awake()
    {
        _netObjects = new Dictionary<Guid, NetObjData>();
        _msgBuffer = new MemoryStream(NetworkManager.MaxPayloadSize);
        _codedMsgBuffer = new CodedOutputStream(_msgBuffer);
        _deleteSet = new HashSet<Guid>();
        _activeSet = new HashSet<Guid>();
        _spawnRequests = new List<SpawnRequestData>();
        _destroyedSceneObjects = new List<Guid>();
        _netObjMessageBuffer = new byte[NetworkManager.MaxPayloadSize];
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        //if (SceneLoadManager == null)
        //    SceneLoadManager = SceneLoadManager.GetDefault();

        NetworkManager.RegisterHandler(VRNPacketType.RequestSpawnObject, RequestSpawnObjectHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SpawnObject, SpawnObjectHandler);
        NetworkManager.RegisterHandler(VRNPacketType.DestroyObject, DestroyObjectHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SetOwner, SetOwnerHandler);
        NetworkManager.RegisterHandler(VRNPacketType.RequestOwnership, RequestOwnershipHandler);
        NetworkManager.RegisterHandler(VRNPacketType.NetObjectSync, NetObjSyncHandler);
        NetworkManager.RegisterHandler(VRNPacketType.NetObjectMessage, NetObjMessageHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendNetworkedObjectList, SendNetworkedObjectListHandler);
        NetworkManager.RegisterHandler(VRNPacketType.NetObjectSyncMulti, NetObjSyncMultiHandler);

        NetworkManager.RegisterHandler(VRNPacketType.VrsetHeldStatus, SetHeldStatusHandler);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        NetworkManager.SceneIDChanged += OnSceneIDChanged;

        NetworkManager.ClientRequestedWorldState += OnClientRequestedWorldState;
        NetworkManager.ClientDisconnected += OnClientDisconnected;
        NetworkManager.ClientJoinedScene += OnClientJoinedScene;
        NetworkManager.ServerDisconnectedFromRelay += OnServerDisconnectedFromRelay;

        Util.DontDestroyOnLoad(gameObject);

    }

    private void OnClientJoinedScene(int obj)
    {
        DelayedWorldStateSend();
    }

    private void OnSceneIDChanged(int sceneID)
    {
        CleanActiveObjectList();
        _destroyedSceneObjects.Clear();
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        CleanActiveObjectList();
    }

    private void SetHeldStatusHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var heldStatus = VRNObjectHeldState.Parser.ParseDelimitedFrom(recvStream);
        var heldStatus = new VRNObjectHeldState();
        reader.ReadMessage(heldStatus);

        //Debug.Log($"NetworkedObjectManager: Held state {heldStatus.ObjectID} : {heldStatus.ObjectHeld}  client: {heldStatus.HeldByClientID}");

        var objID = heldStatus.ObjectID.ToGuid();

        UpdateHeldStatus(objID, heldStatus.ObjectHeld, heldStatus.HeldByClientID);

        //var objData = GetObjectData(objID);
        //if (objData == null)
        //    return;

        //objData.HeldState = heldStatus;
        //ObjectHeldStateChanged?.Invoke(objID);

        //if (heldStatus.ObjectHeld)
        //{
        //    CustomXRInteractable custXR = objData.AssociatedObj.GetComponent<CustomXRInteractable>();
        //    if (custXR != null)
        //        objData.AssociatedObj.transform.localScale = new Vector3(custXR.ScaleHeldObject, custXR.ScaleHeldObject, custXR.ScaleHeldObject);
        //}
        //else
        //{
        //    objData.AssociatedObj.transform.localScale = Vector3.one;
        //}


    }

    private void OnServerDisconnectedFromRelay()
    {
        _deleteSet.Clear();

        foreach (var kvp in _netObjects)
        {
            var data = kvp.Value;

            if (data.AssociatedObj == null)
                continue;

            int ownerClientID = data.AssociatedObj.OwnerClientID;
            if (ownerClientID == 0)
                continue;

            if (data.OwnerExclusive)
            {
                //destroy objects exclusvie to a disconnected owner
                _deleteSet.Add(kvp.Key);
            }
            else
            {
                //server should take control of objects not exclusive to the disconnected owner
                data.AssociatedObj.RequestOwnership();
            }
        }

        foreach (var objid in _deleteSet)
        {
            DestroyObject(objid);
        }
    }

    private void OnClientDisconnected(int obj)
    {
        if (NetworkManager.IsServer)
        {
            //destroy any objects exclusively owned by the disconnected client
            _deleteSet.Clear();

            foreach (var kvp in _netObjects)
            {
                var data = kvp.Value;

                if (data.AssociatedObj == null)
                    continue;

                //if (!data.OwnerExclusive)
                //    continue;

                int ownerClientID = data.AssociatedObj.OwnerClientID;
                if (ownerClientID == 0)
                    continue;

                if (!NetworkManager.IsClientConnected(ownerClientID))
                {
                    if (data.OwnerExclusive)
                    {
                        //destroy objects exclusvie to a disconnected owner
                        _deleteSet.Add(kvp.Key);
                    }
                    else
                    {
                        //server should take control of objects not exclusive to the disconnected owner
                        data.AssociatedObj.RequestOwnership();
                    }
                }
            }

            foreach (var objid in _deleteSet)
            {
                DestroyObject(objid);
            }
        }
    }

    private void OnClientRequestedWorldState(int obj)
    {
        //SendWorldState();
        DelayedWorldStateSend();
    }

    private void DelayedWorldStateSend()
    {
        if (_worldStateSendTime < 0)
        {
            //mark to send the world state in the near future - delay to avoid multiple back-to-back sends
            _worldStateSendTime = Time.time + 1.25f;
        }
    }

    private void SendWorldState()
    {
        BroadcastActiveNetObjects();
        SendObjectSyncData(true);
    }

    public IEnumerable<NetObjData> GetActiveObjects()
    {
        foreach (var data in _netObjects.Values)
        {
            yield return data;
        }
    }

    public void SetHeldStatus(Guid objID, bool held, int clientID = 0)
    {

        var objData = GetObjectData(objID);
        if (objData == null)
            return;

        //_heldStatus.ObjectID = objID.ToByteString();
        //_heldStatus.ObjectHeld = held;
        //_heldStatus.HeldByClientID = clientID;

        UpdateHeldStatus(objID, held, clientID);

        NetworkManager.SendNetMessage(VRNPacketType.VrsetHeldStatus, objData.HeldState, reliable: true);
    }

    private void UpdateHeldStatus(Guid objID, bool held, int clientID)
    {

        var objData = GetObjectData(objID);
        if (objData == null)
            return;

        if (objData.HeldState == null)
            objData.HeldState = new VRNObjectHeldState();

        objData.HeldState.ObjectID = objID.ToByteString();
        objData.HeldState.HeldByClientID = clientID;
        objData.HeldState.ObjectHeld = held;

        ObjectHeldStateChanged?.Invoke(objID);

        //if (held)
        //{
        //    CustomXRInteractable custXR = objData.AssociatedObj.GetComponent<CustomXRInteractable>();
        //    if(custXR != null)
        //        objData.AssociatedObj.transform.localScale = new Vector3(custXR.ScaleHeldObject, custXR.ScaleHeldObject, custXR.ScaleHeldObject);
        //}
        //else
        //{
        //    objData.AssociatedObj.transform.localScale = Vector3.one;
        //}

        Debug.Log($"NetworkedObjectManager: Held state {objID} : {held}  client: {clientID}");

    }

    public NetObjData GetObjectData(Guid objID)
    {
        if (_netObjects == null)
            return null;

        NetObjData data;
        if (_netObjects.TryGetValue(objID, out data))
            return data;

        return null;
    }

    public async Task<NetworkedObject> SpawnObject(string assetID, System.Guid objID, Vector3 startPos, Quaternion startRot, bool spawnActivated = false, int ownerID = -1, bool ownerExclusive = false)
    {
        VRNTransformData xformData = new VRNTransformData()
        {
            Position = startPos.ToVRNVector3(),
            Rotation = startRot.ToVRNQuaternion(),
        };

        VRNSpawnObject spawnObject = new VRNSpawnObject()
        {
            ObjID = ByteString.CopyFrom(objID.ToByteArray()),
            OwnerID = NetworkManager.ClientID,
            AssetID = assetID,
            SpawnActivated = spawnActivated,
            StartPosition = xformData,
            SceneID = NetworkManager.ActiveSceneID,
            OwnerExclusive = ownerExclusive,
        };

        if (ownerID >= 0)
        {
            spawnObject.OwnerID = ownerID;
        }

        NetworkedObject netObj = null;

        if (NetworkManager.IsServer)
        {
            SendSpawnObject(spawnObject);
            netObj = await HandleSpawnObject(spawnObject);
        }
        else
        {
            SpawnRequestData data = new SpawnRequestData
            {
                ObjectID = objID,
                TCS = new TaskCompletionSource<NetworkedObject>(),
            };

            _spawnRequests.Add(data);
            SendRequestSpawnObject(spawnObject);

            if (await Task.WhenAny(data.TCS.Task, Task.Delay(5000)) != data.TCS.Task 
                || data.TCS.Task == null || data.TCS.Task.Result == null)
            {
                _spawnRequests.Remove(data);
                Debug.LogError($"NetworkedObjectManager: Error when spawning {assetID} : {objID}");
                DestroyObject(objID); //destroy object if it actually was spawned
                return null;
            }

            netObj = data.TCS.Task.Result;
        }

        return netObj;
    }

    public void DestroyObject(System.Guid objID)
    {
        VRNDestroyObject destroyObject = new VRNDestroyObject()
        {
            ObjID = ByteString.CopyFrom(objID.ToByteArray()),
        };

        SendDestroyObject(destroyObject);
        HandleDestroyObject(destroyObject);
    }

    public NetworkedObject GetNetworkObject(System.Guid guid)
    {
        NetObjData netInfo;
        if (_netObjects.TryGetValue(guid, out netInfo))
        {
            return netInfo.AssociatedObj;
        }
        return null;
    }

    public int SendObjectSyncData(bool sendAll = false)
    {
        int syncCount = 0;
        if (_netObjects != null && _netObjects.Count > 0)
        {
            //send object state for objects that we have authority on, if necessary
            foreach (NetObjData data in _netObjects.Values)
            {
                if (data.AssociatedObj == null)
                    continue;

                if (!data.AssociatedObj.HasAuthority)
                    continue;

                bool needsUpdate = data.AssociatedObj.NeedsUpdate();
                if (Time.unscaledTime - data.AssociatedObj.LastSyncTime > 10.0f)
                    needsUpdate = true;

                if (needsUpdate || sendAll)
                {
                    SendSyncObj(data.AssociatedObj);
                    syncCount++;
                }
            }
        }

        //Debug.Log($"NetworkedObjectManager: Sent {syncCount} sync messages");
        return syncCount;
    }

    public bool RequestOwnership(NetworkedObject obj)
    {
        if (NetworkManager.IsServer)
        {
            obj.OwnershipState = NetworkedObject.NetOwnershipState.OwnedBySelf;
            obj.OwnerClientID = (short)NetworkManager.ClientID;
            //obj.HasAuthority = true;

            SendSetOwnership(obj.uniqueID, (short)NetworkManager.ClientID);
        }
        else if (obj.ServerExclusive)
        {
            Debug.LogWarning($"NetworkedObjectManager: Attempt to claim server exclusive object {obj.name} {obj.uniqueID}");
            return false;
        }
        else
        {
            obj.OwnershipState = NetworkedObject.NetOwnershipState.OwnershipRequested;
            var objIdBytes = ByteString.CopyFrom(obj.uniqueID.ToByteArray());

            VRNRequestOwnership msg = new VRNRequestOwnership
            {
                ObjID = objIdBytes,
                ClientID = NetworkManager.ClientID,
            };

            Debug.Log($"Sending ownership request {NetworkManager.ClientID} : {obj.uniqueID.ToString()}");

            NetworkManager.SendNetMessage(VRNPacketType.RequestOwnership, msg, clientID: 0, broadcast: false);
        }

        return true;
    }

    public void RegisterNetworkedObject(NetworkedObject obj)
    {
        if (_netObjects.ContainsKey(obj.uniqueID))
        {
            //objects spawned by scripts will already be registered
            Debug.Log($"NetObject: obj {obj.uniqueID.ToString()} already registered");
            return;
        }

        //this should only happen for scene objects
        Debug.Log($"NetObject: Registered scene net obj {obj.uniqueID.ToString()} with manager {gameObject.name}");

        NetObjData objInfo = new NetObjData
        {
            AssociatedObj = obj,
            SceneID = -1, //-1 for scene object
        };

        //_netObjects.Add(obj.uniqueID, objInfo);
        _netObjects[obj.uniqueID] = objInfo;
        RaiseActiveObjectListChanged();
    }

    public void UnregisterNetworkedObject(NetworkedObject obj)
    {
        if (_netObjects != null && _netObjects.ContainsKey(obj.uniqueID))
        {
            _netObjects.Remove(obj.uniqueID);
            RaiseActiveObjectListChanged();
        }
    }

    private async void RequestSpawnObjectHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNSpawnObject.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNSpawnObject();
        reader.ReadMessage(msg);
        await HandleRequestSpawnObject(msg);
    }

    private async void SpawnObjectHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNSpawnObject.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNSpawnObject();
        reader.ReadMessage(msg);
        await HandleSpawnObject(msg);
    }

    private void DestroyObjectHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNDestroyObject.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNDestroyObject();
        reader.ReadMessage(msg);
        HandleDestroyObject(msg);
    }

    private void SetOwnerHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNSetOwner.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNSetOwner();
        reader.ReadMessage(msg);
        HandleSetOwner(msg);
    }

    private void RequestOwnershipHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNRequestOwnership.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNRequestOwnership();
        reader.ReadMessage(msg);
        HandleRequestOwnership(msg);
    }

    private void NetObjSyncMultiHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNNetObjSync.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNNetObjSyncMulti();
        reader.ReadMessage(msg);
        //HandleSyncObj(msg, header, reader);


    }


    private void NetObjSyncHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNNetObjSync.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNNetObjSync();
        reader.ReadMessage(msg);
        HandleSyncObj(msg, header, reader);
    }

    private void NetObjMessageHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNNetObjMessage.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNNetObjMessage();
        reader.ReadMessage(msg);
        HandleNetObjMessage(msg, header, reader);
    }
    private void SendNetworkedObjectListHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNNetworkedObjectList.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNNetworkedObjectList();
        reader.ReadMessage(msg);
        HandleSendNetworkedObjectList(msg);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        StartCoroutine(OnSceneLoadedCoroutine());
    }

    private IEnumerator OnSceneLoadedCoroutine()
    {
        //wait for any scene objects to register
        yield return new WaitForSecondsRealtime(2.0f);

        //clear out invalid objects
        CleanActiveObjectList();

        if (NetworkManager.IsServer)
        {
            //transmit list to clients
            BroadcastActiveNetObjects();
        }
    }

    private void BroadcastActiveNetObjects()
    {
        if (!NetworkManager.IsServer)
            return;

        Debug.Log($"NetworkedObjectManager: Broadcasting all active networked objects");
        if (_netObjects == null || !NetworkManager.IsServer)
            return;

        VRNNetworkedObjectList objList = new VRNNetworkedObjectList();
        objList.ObjectList.Clear();

        foreach (var kvp in _netObjects)
        {
            var netobj = kvp.Value;

            //don't send scene objects for now
            if (netobj.SceneID < 0)
                continue;


            if (netobj.SpawnObjData == null && netobj.AssociatedObj == null)
            {
                Debug.LogError($"NetworkedObjectManager: Bad networked object data");
                continue;
            }

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (netobj.AssociatedObj != null)
            {
                pos = netobj.AssociatedObj.transform.position;
                rot = netobj.AssociatedObj.transform.localRotation;
            }

            if (netobj.SpawnObjData == null)
            {
                VRNSpawnObject spawnObj = new VRNSpawnObject();
                spawnObj.ObjID = ByteString.CopyFrom(netobj.AssociatedObj.uniqueID.ToByteArray());
                spawnObj.OwnerID = netobj.AssociatedObj.OwnerClientID;
                spawnObj.SceneID = netobj.SceneID;
                spawnObj.StartPosition = new VRNTransformData()
                {
                    Position = pos.ToVRNVector3(),
                    Rotation = rot.ToVRNQuaternion(),
                };

                netobj.SpawnObjData = spawnObj;
            }
            else
            {
                if (netobj.SpawnObjData.StartPosition == null)
                    netobj.SpawnObjData.StartPosition = new VRNTransformData();

                netobj.SpawnObjData.StartPosition.Position = pos.ToVRNVector3();
                netobj.SpawnObjData.StartPosition.Rotation = rot.ToVRNQuaternion();
            }

            objList.ObjectList.Add(netobj.SpawnObjData);
        }

        foreach (var destroyedObjID in _destroyedSceneObjects)
        {
            objList.DestroyedSceneObjects.Add(ByteString.CopyFrom(destroyedObjID.ToByteArray()));
        }

        Debug.Log($"NetworkedObjectManager: sending {objList.ObjectList.Count} objects");
        NetworkManager.SendNetMessage(VRNPacketType.SendNetworkedObjectList, objList);

        foreach (var kvp in _netObjects)
        {
            if (kvp.Value.HeldState != null)
                NetworkManager.SendNetMessage(VRNPacketType.VrsetHeldStatus, kvp.Value.HeldState);
        }


    }

    private void CleanActiveObjectList()
    {
        if (_netObjects == null)
            return;

        _deleteSet.Clear();

        foreach (var kvp in _netObjects)
        {
            var netobj = kvp.Value;

            if (netobj.SceneID >= 0 &&
                netobj.SceneID != NetworkManager.ActiveSceneID &&
                NetworkManager.ActiveSceneID >= 0)
            {
                _deleteSet.Add(kvp.Key);
            }
            else if (netobj.AssociatedObj == null && !netobj.LoadInProgress)
            {
                _deleteSet.Add(kvp.Key);
            }
            else if (netobj.AssociatedObj != null && netobj.AssociatedObj.gameObject == null)
            {
                _deleteSet.Add(kvp.Key);
            }
        }

        ProcessDeleteSet();
        RaiseActiveObjectListChanged();
    }

    private void ProcessDeleteSet()
    {
        if (_deleteSet.Count > 0)
        {
            Debug.Log($"NetworkedObjectManager: Deleting {_deleteSet.Count} objects");
        }

        foreach (var id in _deleteSet)
        {
            var netobj = _netObjects[id];

            if (netobj.AssociatedObj != null)
            {
                if (netobj.AssociatedObj.gameObject != null)
                    Destroy(netobj.AssociatedObj.gameObject);
            }

            _netObjects.Remove(id);
        }
    }


    private void SendSpawnObject(string assetID, System.Guid objID, Vector3 startPos, Quaternion startRot, bool spawnActivated = false)
    {
        VRNTransformData xformData = new VRNTransformData()
        {
            Position = startPos.ToVRNVector3(),
            Rotation = startRot.ToVRNQuaternion(),
        };

        VRNSpawnObject spawnObject = new VRNSpawnObject()
        {
            ObjID = ByteString.CopyFrom(objID.ToByteArray()),
            OwnerID = NetworkManager.ClientID,
            AssetID = assetID,
            SpawnActivated = spawnActivated,
            StartPosition = xformData,
        };

        SendSpawnObject(spawnObject);
    }

    private void SendRequestSpawnObject(VRNSpawnObject spawnObject)
    {
        Debug.Log($"Sending spawn object request: Scn:{spawnObject.SceneID} {spawnObject.AssetID}, " +
            $"{spawnObject.ObjID.ToGuid().ToString()} {spawnObject.StartPosition.Position.ToString()}");
        NetworkManager.SendNetMessage(VRNPacketType.RequestSpawnObject, spawnObject, clientID: 0, broadcast: false);
    }

    private void SendSpawnObject(VRNSpawnObject spawnObject)
    {
        Debug.Log($"Sending spawn object message: {spawnObject.AssetID}, " +
            $"{spawnObject.ObjID.ToGuid().ToString()} {spawnObject.StartPosition.Position.ToString()}");

        NetworkManager.SendNetMessage(VRNPacketType.SpawnObject, spawnObject);
    }

    private void SendDestroyObject(System.Guid objID)
    {
        VRNDestroyObject destroyObject = new VRNDestroyObject()
        {
            ObjID = ByteString.CopyFrom(objID.ToByteArray()),
        };

        SendDestroyObject(destroyObject);
    }

    private void SendDestroyObject(VRNDestroyObject destroyObject)
    {
        Debug.Log($"Sending DestroyObject {destroyObject.ObjID.ToGuid().ToString()}");
        NetworkManager.SendNetMessage(VRNPacketType.DestroyObject, destroyObject);
    }

    protected void SendSetOwnership(Guid objID, short clientID)
    {
        var objIdBytes = ByteString.CopyFrom(objID.ToByteArray());
        SendSetOwnership(objIdBytes, clientID);
    }

    protected void SendSetOwnership(ByteString objID, short clientID)
    {
        VRNSetOwner msg = new VRNSetOwner
        {
            ObjID = objID,
            OwnerClientID = clientID,
        };

        NetworkManager.SendNetMessage(VRNPacketType.SetOwner, msg);
        HandleSetOwner(msg);
    }

    private void SendSyncObj(NetworkedObject obj)
    {
        if (!obj.Initialized)
            return;
        try
        {
            //var method = obj.UseReliableSync ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
            _msgBuffer.SetLength(0);
            _msgBuffer.Position = 0;

            WriteNetSyncObj(_codedMsgBuffer, obj.uniqueID, obj.OwnerClientID);


            //obj.WriteObjState(_msgBuffer);
            obj.WriteObjState(_codedMsgBuffer);
            _codedMsgBuffer.Flush();

            _msgBuffer.Position = 0;
            NetworkManager.SendNetMessage(VRNPacketType.NetObjectSync, _msgBuffer, reliable: obj.UseReliableSync);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending obj sync for {obj.name}: {ex.Message} {ex.StackTrace}");
        }
        //Debug.Log($"Sending sync for {obj.uniqueID} size {_msgBuffer.Length}");
    }

    void WriteNetSyncObj(CodedOutputStream stream, Guid objID, short ownerID)
    {
        var objIdBytes = ByteString.CopyFrom(objID.ToByteArray());

        VRNNetObjSync msg = new VRNNetObjSync
        {
            ObjID = objIdBytes,
            OwnerID = ownerID,
        };

        //msg.WriteDelimitedTo(stream);
        stream.WriteMessage(msg);
    }

    public async Task HandleRequestSpawnObject(VRNSpawnObject spawnObject)
    {
        if (!NetworkManager.IsServer)
            return;
        if (spawnObject.SceneID != NetworkManager.ActiveSceneID)
            return;

        //spawn the object as the server, which both creates the local object & sends the spawn command
        //to all clients
        var objID = new Guid(spawnObject.ObjID.ToByteArray());
        await SpawnObject(spawnObject.AssetID, objID,
            spawnObject.StartPosition.Position.ToVector3(),
            spawnObject.StartPosition.Rotation.ToQuaternion(),
            spawnObject.SpawnActivated,
            spawnObject.OwnerID,
            spawnObject.OwnerExclusive);
    }


    public async Task<NetworkedObject> HandleSpawnObject(VRNSpawnObject spawnObject)
    {
        NetObjData netinfo;
        var objID = new Guid(spawnObject.ObjID.ToByteArray());

        //Debug.Log($"NetObject: HandleSpawnObject {spawnObject.ObjID} {spawnObject.SceneID}");

        if (_netObjects.TryGetValue(objID, out netinfo))
        {
            if (netinfo.AssociatedObj != null && netinfo.AssociatedObj.gameObject != null)
            {
                //object already exists, don't spawn a duplicate
                return netinfo.AssociatedObj;
            }
            else
            {
                _netObjects.Remove(objID);
            }
        }

        //if (_destroyedObjects.Contains(objID))
        //{
        //    //this object was already destroyed, don't create it
        //    //_destroyedObjects.Remove(objID);
        //    return null;
        //}

        //add the networked object data structure immediately
        NetObjData objData = new NetObjData();
        objData.SceneID = spawnObject.SceneID;
        objData.SpawnObjData = spawnObject;
        objData.LoadInProgress = true;
        objData.OwnerExclusive = spawnObject.OwnerExclusive;
        _netObjects.Add(objID, objData);

        GameObject prefab = null;

        try
        {
            prefab = await Addressables.LoadAssetAsync<GameObject>(spawnObject.AssetID).Task;
            if (prefab == null)
                return null;
        }
        catch (Exception)
        {
            return null;
        }

        var obj = Instantiate<GameObject>(prefab);
        if (gameObject.scene.name != "DontDestroyOnLoad")
        {
            SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        }


        Debug.Log($"NetObject: Spawn {obj.name}, spAct: {spawnObject.SpawnActivated}, id: {objID}, sceneID: {objData.SceneID}");
        obj.SetActive(spawnObject.SpawnActivated);
        if (spawnObject.StartPosition != null && spawnObject.StartPosition.Position != null)
            obj.transform.position = spawnObject.StartPosition.Position.ToVector3();
        if (spawnObject.StartPosition != null && spawnObject.StartPosition.Rotation != null)
            obj.transform.rotation = spawnObject.StartPosition.Rotation.ToQuaternion();

        var netObj = obj.GetComponent<NetworkedObject>();
        if (netObj != null)
        {
            if (netObj.ServerExclusive)
                spawnObject.OwnerID = 0;

            netObj.NetManager = NetworkManager;
            netObj.NetworkedObjectManager = this;
            netObj.SetOwner((short)spawnObject.OwnerID);
            netObj.UniqueIDString = objID.ToString();
            netObj.uniqueID = objID;
            //netObj.AutomaticSpawn = false;
            netObj.SpawnActivated = spawnObject.SpawnActivated;
        }

        if (NetworkManager.ClientID == spawnObject.OwnerID)
        {
            obj.SetActive(true);
        }

        objData.AssociatedObj = netObj;
        objData.LoadInProgress = false;
        //RegisterNetworkedObject(netObj);

        RaiseActiveObjectListChanged();

        //check if anyone is waiting for this spawn to complete
        foreach (var spawnReqData in _spawnRequests)
        {
            if (spawnReqData.ObjectID == objID)
            {
                //Debug.Log($"Completing spawn request for {netObj.name} id {objID}");
                spawnReqData.TCS.SetResult(netObj);
                _spawnRequests.Remove(spawnReqData);
                break;
            }
        }

        return netObj;
    }

    private void RaiseActiveObjectListChanged()
    {
        try
        {
            ActiveObjectListChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"NetworkedObjectManager: Exception handling ActiveObjectListChanged stack: {ex.StackTrace}");
        }
    }

    void HandleDestroyObject(VRNDestroyObject destroyObject)
    {
        var objID = new Guid(destroyObject.ObjID.ToByteArray());


        NetObjData netObjData;
        if (_netObjects.TryGetValue(objID, out netObjData))
        {
            if (netObjData.IsSceneObject)
            {
                _destroyedSceneObjects.Add(objID);
            }

            if (netObjData.AssociatedObj != null)
            {
                Debug.Log($"NetObject Destroy: {netObjData.AssociatedObj.name}, id: {objID.ToString()}");
                Destroy(netObjData.AssociatedObj.gameObject);
            }
            else
            {
                Debug.Log($"NetObject Destroy Nonexistant Object, id: {objID.ToString()}");
                //_destroyedObjects.Add(objID);
            }

            _netObjects.Remove(objID);
            RaiseActiveObjectListChanged();
        }
        else
        {
            Debug.Log($"NetObject Destroy unknown object, id: {objID.ToString()}");
            //_destroyedObjects.Add(objID);
        }
    }

    //Until everything is switched to CodedInputStream, temporarily copy to a buffer
    //to avoid instantiating CodedInputStream on a Stream
    private byte[] _syncBuffer;

    private void HandleSyncObj(VRNNetObjSync nos, VRNHeader header, CodedInputStream reader)
    {
        if (_syncBuffer == null)
        {
            _syncBuffer = new byte[NetworkManager.MaxPayloadSize];
        }

        var objID = new Guid(nos.ObjID.ToByteArray());
        //Debug.Log($"Got sync data for {objID.ToString()}");

        NetObjData netInfo;
        if (!_netObjects.TryGetValue(objID, out netInfo))
        {
            Debug.LogWarning($"NetworkedObjectManager: {gameObject.name} got sync data for unknown object ID {objID.ToString()}");
            return;
        }

        if (netInfo.AssociatedObj == null)
            return;

        if (netInfo.AssociatedObj.OwnershipState != NetworkedObject.NetOwnershipState.OwnedByOther)
            return;


        //int count = recvStream.Read(_syncBuffer, 0, _syncBuffer.Length);
        //var codedStream = new CodedInputStream(_syncBuffer, 0, count);
        netInfo.AssociatedObj.SyncObjState(reader, header.TickCount, header.ServerTime);

        //netInfo.AssociatedObj.SyncObjState(recvStream, header.TickCount, header.ServerTime);

    }

    public void HandleNetObjMessage(VRNNetObjMessage nom, VRNHeader header, CodedInputStream reader)
    {
        var objID = new Guid(nom.ObjID.ToByteArray());
        NetObjData netInfo;
        if (!_netObjects.TryGetValue(objID, out netInfo))
        {
            Debug.LogWarning($"Got message for unknown object ID {objID.ToString()}");
            return;
        }

        if (netInfo.AssociatedObj == null)
        {
            Debug.LogWarning($"Got message for missing/destroyed object ID {objID.ToString()}");
            return;
        }

        netInfo.AssociatedObj.HandleMessage(nom.MessageType, reader);

        //var bytes = reader.ReadBytes();
        //bytes.CopyTo(_netObjMessageBuffer, 0);

        //netInfo.AssociatedObj.HandleMessage(nom.MessageType, _netObjMessageBuffer, 0, bytes.Length);
    }

    void HandleSetOwner(VRNSetOwner setown)
    {
        if (_netObjects == null)
            return;

        Guid objId = new Guid(setown.ObjID.ToByteArray());
        NetObjData netObjData;

        //Debug.Log($"Received ownership info {setown.OwnerClientID} : {objId.ToString()}");


        if (_netObjects.TryGetValue(objId, out netObjData))
        {
            if (netObjData.AssociatedObj != null)
                netObjData.AssociatedObj.SetOwner((short)setown.OwnerClientID);
        }
    }

    void HandleRequestOwnership(VRNRequestOwnership reqown)
    {
        if (!NetworkManager.IsServer)
            return;

        SendSetOwnership(reqown.ObjID, (short)reqown.ClientID);

    }



    async void HandleSendNetworkedObjectList(VRNNetworkedObjectList objlist)
    {
        if (NetworkManager.IsServer)
            return;

        Debug.Log($"NetworkedObjectManager: Received object list, contains {objlist.ObjectList.Count} objects");
        CleanActiveObjectList();

        _activeSet.Clear();
        _deleteSet.Clear();

        //build active set
        foreach (var spawnObj in objlist.ObjectList)
        {
            //add active objects to the active set
            Debug.Log($"NetworkedObjectManager: Received obj {spawnObj.AssetID} id {spawnObj.ObjID}");
            var objid = spawnObj.ObjID.ToGuid();
            _activeSet.Add(objid);
        }

        //add any deleted scene objects that are still active to the delete set
        foreach (var destroyedObjID in objlist.DestroyedSceneObjects)
        {
            var objID = destroyedObjID.ToGuid();
            if (_netObjects.ContainsKey(objID))
                _deleteSet.Add(objID);
        }


        //check local objects against the active set and destroy any that aren't present
        foreach (var kvp in _netObjects)
        {
            var netobj = kvp.Value;

            //queue any non-scene objects not in the active set for deletion
            //Scene objects should have a scene id of -1
            //if (!_activeSet.Contains(kvp.Key) && kvp.Value.SpawnObjData != null && kvp.Value.SceneID >= 0)
            if (!_activeSet.Contains(kvp.Key) && !kvp.Value.IsSceneObject)
            {
                _deleteSet.Add(kvp.Key);
                Debug.Log($"NetworkedObjectManager: adding {kvp.Key} to the delete set");
            }
        }

        Debug.Log($"NetworkedObjectManager: removing {_deleteSet.Count} objects not in list");
        ProcessDeleteSet();


        //process the list of active objects
        foreach (var spawnObj in objlist.ObjectList)
        {
            NetObjData netObjData;
            NetworkedObject netObj;

            var objid = spawnObj.ObjID.ToGuid();

            //check if the object is instantiated and if not spawn it
            if (_netObjects.TryGetValue(objid, out netObjData))
            {
                netObj = netObjData.AssociatedObj;
            }
            else
            {
                netObj = await HandleSpawnObject(spawnObj);
                netObjData = _netObjects[objid];
            }

            if (netObj != null)
                netObj.SetOwner((short)spawnObj.OwnerID);
        }

        RaiseActiveObjectListChanged();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        NetworkManager.SceneIDChanged -= OnSceneIDChanged;

        NetworkManager.ClientRequestedWorldState -= OnClientRequestedWorldState;
        NetworkManager.ClientDisconnected -= OnClientDisconnected;
        NetworkManager.ServerDisconnectedFromRelay -= OnServerDisconnectedFromRelay;
    }

    private void Update()
    {
        //Debug.Log($"NetworkedObjectManager: {gameObject.name} num objects: {_netObjects.Count}");
        if (NetworkManager.IsPlaybackMode)
            return;

        if (_worldStateSendTime > 0 && Time.time > _worldStateSendTime)
        {
            Debug.Log("NetworkedObjectManager: Sending world state");
            _worldStateSendTime = -1;
            SendWorldState();
        }

    }

}
