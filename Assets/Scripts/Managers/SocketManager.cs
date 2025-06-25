using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
//using LiteNetLib;
using UnityEngine.SceneManagement;
using Google.Protobuf;

public class SocketManager : SceneManagerBase
{
    public class RegisteredSocketData
    {
        public string SocketID;
        public CustomXRSocket Socket;
        public bool IsOccupied;
        public Guid SocketedObjectID;
    }

    public static SocketManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SocketManager>("SocketManager", true);
    }

    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public PlayerManager PlayerManager;
    public Action SocketUpdated;
    //The string value is the SocketID, and the socket is the socket the ID is paired with


    [System.NonSerialized]
    public bool EnableObjectScan = true; //global flag for socket scan, mainly to turn off sockets in debrief

    private Dictionary<string, RegisteredSocketData> _sockets;

    //private List<VRNSocketData> objectsToSocket;
    //private List<Guid> _lockGuid;
    private float _update = 0;
    public float _timeDelta = 0.5f;

    private VRNSocketData _socketData;

    private void Awake()
    {
        _sockets = new Dictionary<string, RegisteredSocketData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        if (NetworkManager == null || NetworkedObjectManager == null)
            return;

        //objectsToSocket = new List<VRNSocketData>();
        //if (_lockGuid == null)
        //{
        //    _lockGuid = new List<Guid>();
        //}
        //NetworkManager.RegisterHandler(VRNPacketType.Vrsocket, SocketUpdateHandler);
        NetworkManager.RegisterHandler(VRNPacketType.VrsocketAssigned, SocketAssignedHandler);
        NetworkManager.RegisterHandler(VRNPacketType.VrsocketVacate, SocketVacatedHandler);
        NetworkManager.RegisterHandler(VRNPacketType.VrsocketAssignRequest, SocketRequestedHandler);
        NetworkManager.RegisterHandler(VRNPacketType.VrsocketVacateRequest, SocketVacateRequestHandler);

        NetworkManager.ClientRequestedWorldState += OnClientRequestedWorldState;
        NetworkedObjectManager.ActiveObjectListChanged += OnActiveObjectListChanged;

        NetworkManager.SceneIDChanged += OnSceneIDChanged;

        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        Util.DontDestroyOnLoad(gameObject);
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Manager";
        }
    }

    private void OnSceneIDChanged(int sceneID)
    {
        if (_sockets == null)
            return;

        Debug.Log($"SocketManager: Clearing sockets, scene ID changed to {sceneID}");
        _sockets.Clear();
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        //Debug.Log($"SocketManager: ActiveSceneChanged");
        //_sockets.Clear();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //Debug.Log($"SocketManager: SceneLoaded");
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        //Debug.Log($"SocketManager: SceneUnloaded");
    }

    private void OnDestroy()
    {
        if (NetworkManager != null)
        {
            //NetworkManager.UnregisterHandler(VRNPacketType.Vrsocket, SocketUpdateHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.VrsocketAssigned, SocketAssignedHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.VrsocketVacate, SocketVacatedHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.VrsocketAssignRequest, SocketRequestedHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.VrsocketVacateRequest, SocketVacateRequestHandler);

            NetworkManager.ClientRequestedWorldState -= OnClientRequestedWorldState;
        }

        if (NetworkedObjectManager != null)
        {
            NetworkedObjectManager.ActiveObjectListChanged -= OnActiveObjectListChanged;
        }

        NetworkManager.SceneIDChanged -= OnSceneIDChanged;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveObjectListChanged()
    {
        //List<VRNSocketData> socketsToRemove = new List<VRNSocketData>();
        //foreach (var obj in NetworkedObjectManager.GetActiveObjects())
        //{
        //    foreach(VRNSocketData data in objectsToSocket)
        //    {
        //        if(Guid.TryParse(data.ObjectID, out Guid check))
        //        {
        //            if(obj.AssociatedObj == null)
        //            {
        //                continue;
        //            }
        //            if(check == obj.AssociatedObj.uniqueID)
        //            {
        //                CustomXRInteractable customXRInteractable = obj.AssociatedObj.GetComponent<CustomXRInteractable>();
        //                Debug.Log($"Found object: {customXRInteractable.name}");
        //                if (customXRInteractable == null)
        //                {
        //                    Debug.LogError("Socketed item has no interactable component on it.");
        //                    continue;
        //                }
        //                CustomXRSocket socket;
        //                if (_sockets.TryGetValue(data.SocketID, out socket))
        //                {
        //                    socket.SocketItem(customXRInteractable);
        //                    socketsToRemove.Add(data);
        //                }
        //            }
        //        }
        //    }
        //}

        //foreach (VRNSocketData removal in socketsToRemove)
        //{
        //    if (objectsToSocket.Contains(removal))
        //    {
        //        objectsToSocket.Remove(removal);
        //    }
        //}
    }

    private void OnClientRequestedWorldState(int obj)
    {
        if (!NetworkManager.IsServer)
            return;

        Debug.Log($"Client requested world state: {obj}");
        //foreach (KeyValuePair<string, CustomXRSocket> kvp in _sockets)
        //{
         
        //    //if(kvp.Value.SocketedInteractable != null)
        //    //{
        //    //    AssignSocket(kvp.Key, kvp.Value.SocketedInteractable.GetComponent<NetworkedObject>().uniqueID.ToString());
        //    //}
        //}

        foreach (var socketData in _sockets.Values)
        {
            if (socketData.IsOccupied)
                AssignSocket(socketData.SocketID, socketData.SocketedObjectID);
            else
                VacateSocket(socketData.SocketID);
        }
    }

    private VRNSocketData ReadSocketData(CodedInputStream reader)
    {
        //if (_socketData == null)
        //    _socketData = VRNSocketData.Parser.ParseDelimitedFrom(recvStream);
        //else
        //{
        //    _socketData.ObjectID = ByteString.Empty;
        //    _socketData.SocketID = "";
        //    _socketData.MergeDelimitedFrom(recvStream);
        //}

        if (_socketData == null)
            _socketData = new VRNSocketData();

        _socketData.ObjectID = ByteString.Empty;
        _socketData.SocketID = "";
        reader.ReadMessage(_socketData);

        return _socketData;
    }

    private void SocketAssignedHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        var socketData = ReadSocketData(reader);

        AssignSocket(socketData.SocketID, socketData.ObjectID.ToGuid());
    }

    private void SocketVacatedHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        var socketData = ReadSocketData(reader);

        VacateSocket(socketData.SocketID);
    }

    private void SocketRequestedHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        var socketData = ReadSocketData(reader);

        if (!NetworkManager.IsServer)
            return;

        AssignSocket(socketData.SocketID, socketData.ObjectID.ToGuid());
    }

    private void SocketVacateRequestHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        var socketData = ReadSocketData(reader);

        if (!NetworkManager.IsServer)
            return;

        VacateSocket(socketData.SocketID);
    }

    //private void SocketUpdateHandler(VRNHeader header, Stream recvStream, NetPeer fromPeer)
    //{

    //    var socketData = VRNSocketData.Parser.ParseDelimitedFrom(recvStream);
    //    CustomXRSocket socket;
    //    bool isValidGUID = false;
    //    bool lockCheck = false;
    //    Guid objGuid;
    //    if(System.Guid.TryParse(socketData.ObjectID, out objGuid)){
    //        isValidGUID = true;
    //    }
    //    if (NetworkManager.IsServer)
    //    {
    //        Debug.Log($"Received Socket Message: Socket ID - {socketData.SocketID}, Object ID - {socketData.ObjectID}");
    //        if (_sockets.TryGetValue(socketData.SocketID, out socket))
    //        {
    //            switch (socketData.SocketMessageType)
    //            {
    //                case 1:

    //                    NetworkedObject netObj = null;
    //                    if (isValidGUID) 
    //                    { 
    //                        netObj = NetworkedObjectManager.GetNetworkObject(objGuid);
    //                    }
    //                    if (netObj == null && isValidGUID)
    //                    {
    //                        objectsToSocket.Add(socketData);
    //                        return;
    //                    }
    //                    CustomXRInteractable customXRInteractable = netObj.GetComponent<CustomXRInteractable>();
    //                    if (customXRInteractable == null)
    //                    {
    //                        Debug.LogError("Socketed item has no interactable component on it.");
    //                        return;
    //                    }
    //                    Debug.Log($"Socket Manager: Socketing item - {customXRInteractable.name}");
    //                    socket.SocketItem(customXRInteractable);
    //                    lockCheck = _lockGuid.Contains(objGuid);
    //                    if (lockCheck)
    //                    {
    //                        _lockGuid.Remove(objGuid);
    //                    }
    //                    VRNSocketData vRNSocketData = new VRNSocketData();
    //                    vRNSocketData.SocketID = socketData.SocketID;
    //                    vRNSocketData.ObjectID = socketData.ObjectID;
    //                    vRNSocketData.SocketMessageType = 2;
    //                    NetworkManager.SendNetMessage(VRNPacketType.Vrsocket, vRNSocketData, null, null, true, DeliveryMethod.ReliableOrdered);
    //                    break;

    //                case 8:

    //                    _sockets.Remove(socketData.SocketID);
    //                    break;
    //                case 10:
    //                    Debug.Log($"Socket Manager: Removing socketed item - {socketData.SocketID}");
    //                    socket.RemoveSocketedItem();
    //                    //return;
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            if(isValidGUID)
    //                objectsToSocket.Add(socketData);
    //        }
    //    }
    //    else
    //    {
    //        if (_sockets.TryGetValue(socketData.SocketID, out socket))
    //        {
    //            switch (socketData.SocketMessageType)
    //            {
    //                case 1:
    //                    break;
    //                case 2:
    //                    NetworkedObject netObj = NetworkedObjectManager.GetNetworkObject(System.Guid.Parse(socketData.ObjectID));
    //                    if (netObj == null && isValidGUID)
    //                    {
    //                        objectsToSocket.Add(socketData);
    //                        return;
    //                    }
    //                    CustomXRInteractable customXRInteractable = netObj.GetComponent<CustomXRInteractable>();
    //                    if (customXRInteractable == null)
    //                    {
    //                        Debug.LogError("Socketed item has no interactable component on it.");
    //                        return;
    //                    }
    //                    if (socket.SocketedInteractable == null)
    //                    {
    //                        Debug.Log($"Socket Manager: Socketing item - {customXRInteractable.name}");
    //                        socket.SocketItem(customXRInteractable);
    //                    }
    //                    break;

    //                case 8:
    //                    _sockets.Remove(socketData.SocketID);
    //                    break;
    //                case 10:
    //                    socket.RemoveSocketedItem();

    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            if(isValidGUID)
    //                objectsToSocket.Add(socketData);
    //        }
    //    }
    //}

    //private void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.F12))
    //    {
    //        foreach (KeyValuePair<string, CustomXRSocket> keyValuePair in _sockets)
    //        {
    //            Debug.Log($"SocketID: {keyValuePair.Key}, Socket: {keyValuePair.Value.name}");
    //        }
    //    }

    //    if (objectsToSocket.Count > 0 && Time.time > _update)
    //    {
    //        List<VRNSocketData> socketsToRemove = new List<VRNSocketData>();
    //        foreach (VRNSocketData socketData in objectsToSocket)
    //        {
    //            CustomXRSocket socket;
    //            if (_sockets.TryGetValue(socketData.SocketID, out socket))
    //            {
    //                //Debug.Log($"Trying to find objectID: {socketData.ObjectID}");
    //                Guid guid;
    //                if (System.Guid.TryParse(socketData.ObjectID, out guid))
    //                {
    //                    NetworkedObject netObj = NetworkedObjectManager.GetNetworkObject(guid);
    //                    if (netObj != null)
    //                    {
    //                        CustomXRInteractable customXRInteractable = netObj.GetComponent<CustomXRInteractable>();
    //                        Debug.Log($"Found object: {customXRInteractable.name}");
    //                        if (customXRInteractable == null)
    //                        {
    //                            Debug.LogError("Socketed item has no interactable component on it.");
    //                            continue;
    //                        }
    //                        socket.SocketItem(customXRInteractable);
    //                        socketsToRemove.Add(socketData);
    //                    }
    //                }
    //            }
    //        }
    //        foreach (VRNSocketData removal in socketsToRemove)
    //        {
    //            if (objectsToSocket.Contains(removal))
    //            {
    //                objectsToSocket.Remove(removal);
    //            }
    //        }

    //        _update = Time.time + _timeDelta;
    //    }
    //}

    private RegisteredSocketData CreateSocketData(string socketID)
    {
        var socketData = new RegisteredSocketData();
        socketData.SocketID = socketID;
        socketData.IsOccupied = false;
        socketData.SocketedObjectID = Guid.Empty;

        _sockets[socketID] = socketData;

        return socketData;
    }

    public void RegisterSocket(string socketID, CustomXRSocket socket)
    {
        //Debug.Log($"In register socket: {socketID}");
        //if(!_sockets.ContainsKey(socketID))
        //    _sockets.Add(socketID, socket);

        var socketData = GetSocketData(socketID);

        if (socketData == null)
        {
            socketData = CreateSocketData(socketID);
        }

        socketData.SocketID = socketID;
        socketData.Socket = socket;

        //if we already received a message that the socket is occupied, socket the target object
        if (socketData.IsOccupied && socketData.Socket != null)
        {
            Debug.Log($"SocketManager: Socketing item {socketData.SocketedObjectID} in socket {socketData.SocketID} on registration");
            socketData.Socket.InternalSocketItem(socketData.SocketedObjectID);
        }

        _sockets[socketID] = socketData;
    }

    public void UnregisterSocket(string socketID)
    {
        //ClearLocalSocket(socketID);
        VacateSocket(socketID);
        _sockets.Remove(socketID);
        //VRNSocketData socketData = new VRNSocketData();
        //socketData.SocketID = socketID;
        //socketData.ObjectID = "none";
        //socketData.SocketMessageType = 8;
        //NetworkManager.SendNetMessage(VRNPacketType.Vrsocket, socketData, null, null, true, DeliveryMethod.ReliableOrdered);
    }

    public void RequestSocketAssignment(string socketID, Guid objID)
    {
        if (NetworkManager.IsServer)
        {
            AssignSocket(socketID, objID);
            return;
        }

        if (_socketData == null)
            _socketData = new VRNSocketData();

        _socketData.ObjectID = objID.ToByteString();
        _socketData.SocketID = socketID;
        NetworkManager.SendNetMessage(VRNPacketType.VrsocketAssignRequest, _socketData, reliable: true);
    }

    public void RequestSocketVacate(string socketID)
    {
        if (gameObject == null)
            return;

        var trace = new System.Diagnostics.StackTrace(true);
        var calledBy = trace.GetFrame(1);

        //Debug.Log($"SocketManager: {calledBy.GetFileName()}:{calledBy.GetMethod()}:{calledBy.GetFileLineNumber()} requesting socket vacate {socketID} scene {gameObject.scene.name}");
        if (NetworkManager.IsServer)
        {
            VacateSocket(socketID);
            return;
        }

        if (_socketData == null)
            _socketData = new VRNSocketData();

        _socketData.ObjectID = ByteString.Empty;
        _socketData.SocketID = socketID;
        NetworkManager.SendNetMessage(VRNPacketType.VrsocketVacateRequest, _socketData, reliable: true);
    }

    private void AssignSocket(string socketID, Guid objID)
    {
        Debug.Log($"SocketManager: Assigning item {objID} to socket {socketID}");
        var socketData = GetSocketData(socketID);
        if(socketData == null)
        {
            //Debug.LogError($"Attempt to assign unregistered socket {socketID}");
            socketData = CreateSocketData(socketID);
        }

        if (NetworkManager.IsServer && socketData.IsOccupied && socketData.SocketedObjectID != objID)
        {
            //replacing socketed object, remove old object
            if (socketData.Socket != null)
                socketData.Socket.RequestRemoveSocketedItem();
        }

        socketData.IsOccupied = true;
        socketData.SocketedObjectID = objID;

        VRNSocketData vRNSocketData = new VRNSocketData();
        vRNSocketData.SocketID = socketID;
        vRNSocketData.ObjectID = objID.ToByteString();

        if (NetworkManager.IsServer)
            NetworkManager.SendNetMessage(VRNPacketType.VrsocketAssigned, vRNSocketData, reliable: true);

        if (socketData.Socket != null)
            socketData.Socket.InternalSocketItem(socketData.SocketedObjectID);
    }

    private void VacateSocket(string socketID)
    {
        Debug.Log($"Vacating socket {socketID}");

        var socketData = GetSocketData(socketID);
        if (socketData == null)
        {
            //Debug.LogError($"Attempt to vacate unregistered socket {socketID}");
            //return;
            socketData = CreateSocketData(socketID);
        }

        socketData.IsOccupied = false;
        socketData.SocketedObjectID = Guid.Empty;

        VRNSocketData socketMessage = new VRNSocketData();
        socketMessage.SocketID = socketID;
        socketMessage.ObjectID = ByteString.Empty;
        //socketData.SocketMessageType = 10;

        if (NetworkManager.IsServer)
            NetworkManager.SendNetMessage(VRNPacketType.VrsocketVacate, socketMessage, reliable: true);

        if (socketData.Socket != null)
            socketData.Socket.InternalRemoveSocketedItem();
    }

    //remove any socketed items from the specified socket prior to the local socket being destroyed
    private void ClearLocalSocket(string socketID)
    {
        var socketData = GetSocketData(socketID);
        if (socketData == null)
        {
            return;
        }

        if (socketData.Socket != null)
            socketData.Socket.InternalRemoveSocketedItem();

        socketData.Socket = null;
    }

    public void UnsocketObject(Guid objectID)
    {
        foreach (var socket in _sockets.Values)
        {
            if (socket.IsOccupied && socket.SocketedObjectID == objectID)
            {
                RequestSocketVacate(socket.SocketID);
                break;
            }
        }
    }

    public bool IsSocketOccupied(string socketID)
    {
        var socketData = GetSocketData(socketID);
        if (socketData == null)
            return false;

        return socketData.IsOccupied;
    }

    public RegisteredSocketData GetSocketData(string socketID)
    {
        if (_sockets == null)
            return null;

        RegisteredSocketData socketData = null;
        if (_sockets.TryGetValue(socketID, out socketData))
            return socketData;

        return null;
    }

    public IEnumerable<CustomXRSocket> GetAllSockets()
    {
        foreach (var kvp in _sockets)
        {
            if (kvp.Value != null)
            {
                yield return kvp.Value.Socket;
            }
        }
    }

    public IEnumerable<RegisteredSocketData> GetAllSocketData()
    {
        foreach (var kvp in _sockets)
        {
            if (kvp.Value != null)
            {
                yield return kvp.Value;
            }
        }
    }

}
