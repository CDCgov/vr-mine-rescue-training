using CsvHelper;
using Google.Protobuf;
//using LiteNetLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Collections.LowLevel.Unsafe;
using System.Globalization;

[System.Serializable]
public class SceneLoadEvent : UnityEvent<VRNLoadScene> { }
public class VRPlayerInfoEvent : UnityEvent<VRNVRPlayerInfo> { }
public class ClientStateEvent : UnityEvent<VRNClientState> { }
public class TeleportAllEvent : UnityEvent<VRNTeleportAll> { }


public class NetworkManager : VRMineTransport
{
    private float NetworkSyncInterval = 0.04f;
    //public const string DefaultResourcePath = "Managers/NetworkManager";
    //public static NetworkManager GetDefault() { return Resources.Load<NetworkManager>(DefaultResourcePath); }

    public static NetworkManager GetDefault(GameObject self)
    {
        return  self.GetDefaultManager<NetworkManager>("NetworkManager", false);
        //var obj = GameObject.Find("NetworkManager");
        //if (obj == null)
        //{
        //    obj = new GameObject("NetworkManager");
        //    //obj.tag = "Manager";
        //}
        //var manager = obj.GetComponent<NetworkManager>();
        //if (manager == null)
        //    manager = obj.AddComponent<NetworkManager>();

        //return manager;
    }

    public delegate void VRNMessageHandler(VRNHeader header, CodedInputStream recvStream, int clientID);


    public enum NetCommand : byte
    {
        RequestClientJoin = 1,
        AssignClientInfo = 2,
        LoadScene = 3,
        SyncNetObj = 4,
    }

    public struct NetIDInfo
    {
        //public int ownerClientID;
        //public uint netID;
        //public int prefabID;
        //public string prefabResource;
        public NetworkedObject associatedObj;
    }

    public struct PacketStats
    {
        public string PacketType;
        public int PacketCount;
        public int TotalBytes;
        public int MaxPacketSize;
    }

    public class ClientInfo
    {
        //public int playerID;
        //public string playerName;
        public short clientID;
        //public NetPeer peer;
        public NetworkConnection conn;
    }


    public event Action<VRNLoadScene> SceneLoadCommand;
    //public event Action<VRNTeleportAll> TeleportAllCommand;
    public event Action<VRNClientState> ClientStateChanged;
    //public VRPlayerInfoEvent VRPlayerInfoChanged;

    public event Action<int> ClientIDAssigned;
    //public event Action<int> PlayerIDAssigned;
    //public event Action<VRNPlayerRole> PlayerRoleChanged;
    public event Action<VRNStateDataUpdate> StateDataChanged;
    public event Action<VRNSimState> SimStateChanged;
    public event Action<int> ClientRequestedWorldState;

    public event Action<int> ClientJoinedServer;
    public event Action<int> ClientJoinedScene;
    public event Action<int> ClientDisconnected;
    public event Action<int> SceneIDChanged;

    public event Action DisconnectedFromServer;
    public event Action ServerDisconnectedFromRelay;
    public event Action NetworkUpdateRateChanged;

    public event Action SessionRecordingStarted;
    public event Action SessionRecordingStopped;


    public const string NetworkVersionString = "0.01";
    //public const int MaxPayloadSize = 10000;


    public SystemManager SystemManager;
    //public SceneLoadManager SceneLoadManager;
    //public PlayerManager PlayerManager;
    //public TeleportManager TeleportManager;
    public NetworkedObjectManager NetworkedObjectManager;

    public bool OverrideConfiguration = false;
    //public bool EnableUDPBroadcast = false;
    public bool AutomaticReconnect = true;
    //public int UDPBroadcastPort = 9898;
    public int PrimaryUDPPort = 9090;
    public bool OverrideMultiplayerName = false;

    public bool StartAsServer = false;
    public bool AcceptConnections = true;
    public string DefaultMultiplayerServer = "";
    //private VRNRelayClientList _relayClients;
    private Dictionary<int, VRNRelayClientInfo> _relayClients;
    private Dictionary<int, VRNClientStats> _clientStats;

    public bool ClientConnected
    {
        get
        {
            return _clientIDValid;
        }
    }

    public bool IsRecording
    {
        get
        {
            if (_sessionLog != null)
                return true;
            else
                return false;
        }
    }

    public bool IsStatLogEnabled
    {
        get
        {
            if (_packetSendStats != null && _packetRecvStats != null)
                return true;
            else
                return false;
        }
        set
        {
            if (value)
            {
                if (_packetRecvStats == null || _packetRecvStats == null)
                    InitializePacketStatLog();
            }
            else
            {
                _packetRecvStats = null;
                _packetSendStats = null;
            }
        }
    }

    public Dictionary<string, PacketStats> PacketSendStats
    {
        get { return _packetSendStats; }
    }

    public Dictionary<string, PacketStats> PacketRecvStats
    {
        get { return _packetRecvStats; }
    }

    /// <summary>
    /// number of object sync messages sent this frame
    /// </summary>
    public int FrameSyncCount
    {
        get; set;
    }

    /// <summary>
    /// max number of sync messages sent in a frame so far
    /// </summary>
    public int FrameSyncCountMax
    {
        get; set;
    }

    //private Dictionary<int, ClientInfo> _connectedClients;
    private Dictionary<System.Guid, NetIDInfo> _netObjects;
    //private Dictionary<NetworkConnection, ClientInfo> _peerMap;

    private Dictionary<string, VRNStateDataUpdate> _stateData;

    private bool _clientIDValid = false;
    //private short _clientID = -1;
    //private bool _playerIDValid = false;
    //private short _playerID = -1;
    //private ushort _lastNetIDIndex = 0;
    private short _lastClientIDAssigned = 0;
    //private short _lastPlayerIDAssigned = 0;


    private byte[] _receiveBuffer;

    private HashSet<Guid> _destroyedObjects = new HashSet<Guid>();

    //private EventBasedNetListener _netListener;
    //private NetManager _netManager;
    //private NetPeer _serverConn;


    private Dictionary<VRNPacketType, List<VRNMessageHandler>> _handlerMap;

    private bool _sentJoinRequest = false;

    private VRNSimState _simState;

    private VRNHeader _vrnHeader = new VRNHeader();
    private VRNPingResponse _pingReponse = new VRNPingResponse();
    private float _lastNetSyncSent = 0;

    [System.NonSerialized]
    public int LastBytesPerSecond = 0;
    [System.NonSerialized]
    public double AvgBytesPerSecond = 0;

    [System.NonSerialized]
    public int LastFramePacketsSent = 0;
    [System.NonSerialized]
    public int LastFrameBytesSent = 0;

    [System.NonSerialized]
    public double AvgFramePackets = 0;
    [System.NonSerialized]
    public double AvgFrameBytes = 0;

    private int _framePacketsSent = 0;
    private long _frameBytesSent = 0;

    private long _bytesPerSecond = 0;
    private double _avgBytesPerSecond = 0;
    private float _lastPerSecondReset = 0;

    private bool _syncUpdatesPaused = false;

    private string _serverAddress = null;

    private bool _isServer = true;
    private int _sceneID = 0;

    private Dictionary<string, PacketStats> _packetSendStats;
    private Dictionary<string, PacketStats> _packetRecvStats;


    private SessionLog _sessionLog;

    public bool IsPlaybackMode = false;

    public bool IsServer
    {
        get
        {
            return _isServer;
        }
    }

    public bool IsInGame
    {
        get
        {
            if (_isServer)
                return true;
            else if (_clientIDValid)
                return true;
            else
                return false;
        }
    }

    //public short ClientID
    //{
    //    get
    //    {
    //        return (short)ClientID;
    //        //return _clientID;
    //    }
    //}

    public int ActiveSceneID
    {
        get
        {
            return _sceneID;
        }
    }


    private void Initialize()
    {
        FrameSyncCount = 0;
        FrameSyncCountMax = 0;

        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        NetworkedObjectManager.gameObject.tag = "Manager";

        //DontDestroyOnLoad(gameObject);
    }

    protected override void Awake()
    {
        base.Awake();

        Debug.Log("NetworkManager - Awake");
        _stateData = new Dictionary<string, VRNStateDataUpdate>();

        //_sendBuffer = new MemoryStream(MaxPayloadSize);
        //_msgBuffer = new MemoryStream(MaxPayloadSize);
        //_codedSendBuffer = new CodedOutputStream(_sendBuffer);

        _receiveBuffer = new byte[MaxPayloadSize];

        _relayClients = new Dictionary<int, VRNRelayClientInfo>();
        _clientStats = new Dictionary<int, VRNClientStats>();
        //_connectedClients = new Dictionary<int, ClientInfo>();
        _netObjects = new Dictionary<System.Guid, NetIDInfo>();
        //_peerMap = new Dictionary<NetworkConnection, ClientInfo>();

        Initialize();
    }

    protected override void Start()
    {        
        base.Start();
        Util.DontDestroyOnLoad(gameObject);

        if (IsServer && !AcceptConnections)
            return; //don't connect to relay

        Debug.Log($"Connecting to {_serverAddress}:{PrimaryUDPPort}");
        ConnectToServer();

    }

    private void UpdateSyncInterval()
    {
        if (SystemManager.SystemConfig != null && SystemManager.SystemConfig.MPObjectUpdateRateHz > 0)
        {
            NetworkSyncInterval = 1.0f / SystemManager.SystemConfig.MPObjectUpdateRateHz;
            Debug.Log($"Object sync interval set to {(NetworkSyncInterval * 1000.0f):F1}ms");
        }
    }

    void OnEnable()
    {
        _isServer = StartAsServer;

        Debug.Log("NetManager: Enabled");
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        //_test = Guid.NewGuid().ToString();
        //Debug.Log($"NetworkManager - OnEnable {_test}");
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (_handlerMap == null)
            _handlerMap = new Dictionary<VRNPacketType, List<VRNMessageHandler>>();

        if (!OverrideConfiguration)
        {
            //StartAsServer = SystemManager.SystemConfig.DefaultToServerMode;
            PrimaryUDPPort = SystemManager.SystemConfig.MultiplayerPort;
            _isServer = StartAsServer;
        }

        InitializeNetwork();

        UpdateSyncInterval();

        Debug.Log("Loading network manager");

        if (StartAsServer)
        {
            Debug.Log($"Starting as server, primary UDP port {PrimaryUDPPort}");

          
            ClientID = 0;
            _clientIDValid = true;

            ClientIDAssigned?.Invoke(ClientID);

            //initialize sim state
            _simState = new VRNSimState();
            _simState.ActiveScene = "";
            _simState.ActiveSceneID = 0;
            //SendSimStateChanged(_simState);
        }

        _serverAddress = DefaultMultiplayerServer;
        if (!OverrideConfiguration || _serverAddress == null || _serverAddress.Length <= 0)
        {
            _serverAddress = SystemManager.SystemConfig.MultiplayerServer;
            PrimaryUDPPort = SystemManager.SystemConfig.MultiplayerPort;
        }




        if (SystemManager.SystemConfig.MPPacketStatLogEnabled)
        {
            InitializePacketStatLog();
        }
    }

    private void InitializePacketStatLog()
    {
        _packetRecvStats = new Dictionary<string, PacketStats>();
        _packetSendStats = new Dictionary<string, PacketStats>();
    }



    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (_sessionLog != null)
        {
            StopSessionRecording();
        }

        _destroyedObjects.Clear();
    }

    void ConnectToServer()
    {
        //_serverConn = _netManager.Connect(_serverAddress, PrimaryUDPPort, "VRMineNet_1");
        Connect(_serverAddress, (ushort)PrimaryUDPPort);
    }

    void OnDisable()
    {
        Debug.Log("NetManager: Disabled");
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        //this seems to cause a race condition that disconnects right after connecting
        //if (_netManager != null)
        //{
        //    _netManager.Stop();
        //    _netManager = null;
        //}

        if (SystemManager.SystemConfig.MPPacketStatLogEnabled && _packetRecvStats != null && _packetSendStats != null)
        {
            WritePacketStatLogs();
        }
    }

    public void LogSessionEvent(VRNLogEventType eventType, string message, Vector3 pos, Quaternion rot, string objName,
        VRNLogObjectType objType = VRNLogObjectType.Unknown, string positionMetadata = null)
    {
        var playerManager = PlayerManager.GetDefault(gameObject);
        var playerID = -1;

        if (playerManager != null && playerManager.CurrentPlayer != null)
            playerID = playerManager.CurrentPlayer.PlayerID;
        else if (IsServer)
            playerID = 0;

        if (message == null)
            message = "";
        if (positionMetadata == null)
            positionMetadata = "";
        if (objName == null)
            objName = "Unknown";

        VRNLogEvent ev = new VRNLogEvent
        {
            EventType = eventType,
            Message = message,
            Position = pos.ToVRNVector3(),
            Rotation = rot.ToVRNQuaternion(),
            ObjectType = objType,
            ObjectName = objName,
            SourcePlayerID = playerID,
            PositionMetadata = positionMetadata,
        };

        LogSessionEvent(ev);
    }

    /// <summary>
    /// Send or write a log event as appropriate
    /// </summary>
    /// <param name="ev"></param>
    public void LogSessionEvent(VRNLogEvent ev)
    {
        if (IsServer)
            WriteSessionEvent(ev);
        else
            SendSessionEvent(ev);
    }

    /// <summary>
    /// Send a session event to the server for logging
    /// </summary>
    /// <param name="ev"></param>
    private void SendSessionEvent(VRNLogEvent ev)
    {
        SendNetMessage(VRNPacketType.LogEvent, ev, broadcast: false);
    }

    private void WriteSessionEvent(VRNLogEvent ev)
    {
        if (_sessionLog == null)
            return;

        ev.Timestamp = Time.time;
        _sessionLog.WriteEvent(ev);
    }

    public bool IsSessionRecording()
    {
        if (_sessionLog != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void StartSessionRecording(string teleportTarget, string sessionName)
    {
        var filename = SessionLog.GenerateFilename(_simState.ActiveScene, sessionName, SystemManager.SystemConfig.SessionLogsFolder);
        Debug.Log($"SessionLog: Starting session log recording to {filename}");
        StartSessionRecording(teleportTarget, sessionName, filename);
    }

    public void StartSessionRecording(string teleportTarget, string sessionName, string filename)
    {
        VRNLogHeader logHeader = new VRNLogHeader();
        logHeader.ActiveScene = _simState.ActiveScene;
        //logHeader.TeleportTarget = _simState.TeleportTarget;
        logHeader.SessionName = sessionName;
        //logHeader.TeleportTarget = TeleportManager.ActivePOIName;
        logHeader.TeleportTarget = teleportTarget;
        logHeader.LogStartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

        if (_sessionLog != null)
            StopSessionRecording();

        try
        {
            _sessionLog = new SessionLog();
            _sessionLog.CreateLog(filename, logHeader);

            //request clients to send world state, including calibration data
            SendRequestWorldState();

            //send all network object data 
            //BroadcastActiveNetObjects();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Couldn't create session log : {ex.Message}");
            _sessionLog = null;
        }

        SessionRecordingStarted?.Invoke();
    }

    public SessionLog GetActiveSessionRec()
    {
        return _sessionLog;
    }

    public void StopSessionRecording()
    {
        if (_sessionLog != null)
        {
            Debug.Log($"SessionLog: Stopping session record, {_sessionLog.MessageCount} messages written");
            _sessionLog.Dispose();
            _sessionLog = null;

            SessionRecordingStopped?.Invoke();
        }
    }

    private void WritePacketStatLogs()
    {
        //string guid = Guid.NewGuid().ToString();
        string dateString = System.DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss");

        string filename = $"{dateString}_PacketSend.csv";
        WritePacketStatLog(filename, _packetSendStats.Values);

        filename = $"{dateString}_PacketRecv.csv";
        WritePacketStatLog(filename, _packetRecvStats.Values);
    }

    private void WritePacketStatLog(string filename, IEnumerable<PacketStats> packetStats)
    {
        if (packetStats == null)
            return;

        Directory.CreateDirectory("NetLogs");
        filename = Path.Combine("NetLogs", filename);


        using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
        {

            using (CsvWriter csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
            {
                csv.WriteField("PacketType");
                csv.WriteField("Count");
                csv.WriteField("TotalBytes");
                csv.NextRecord();

                foreach (var ps in packetStats)
                {
                    csv.WriteField(ps.PacketType);
                    csv.WriteField(ps.PacketCount);
                    csv.WriteField(ps.TotalBytes);
                    csv.NextRecord();
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Debug.Log("NetworkManager - OnDestroy");
        StopSessionRecording();
    }

    void OnApplicationQuit()
    {
        Debug.Log("Network Manager - OnApplicationQuit");
        StopSessionRecording();
    }


    public void GetStatusText(StringBuilder sb)
    {
        if (StartAsServer)
        {
            sb.AppendLine("Started as a server");
            //sb.AppendLine($"{_connectedClients.Count} clients connected");
        }
        else
        {
            string connStatus = "Unknown";


            if (!_clientConnection.IsCreated)
            {
                connStatus = "Not Created";
            }
            else
            {
                var state = _clientConnection.GetState(NetworkDriver);
                switch (state)
                {
                    case NetworkConnection.State.Connected:
                        connStatus = "Connected";
                        break;
                    case NetworkConnection.State.Connecting:
                        connStatus = "Connecting";
                        break;
                    case NetworkConnection.State.Disconnected:
                        connStatus = "Disconnected";
                        break;

                }
            }


            if (IsConnected)
            {
                sb.AppendLine($"Connected to server {_serverAddress} state: {connStatus}");
                //sb.AppendLine($"Ping: {_serverConn.Ping}");

            }
            else
            {
                sb.AppendLine($"Connecting to {_serverAddress} state: {connStatus}");
            }
        }
    }


    public void RegisterHandler(VRNPacketType packetType, VRNMessageHandler handler)
    {
        if (_handlerMap == null)
            _handlerMap = new Dictionary<VRNPacketType, List<VRNMessageHandler>>();

        List<VRNMessageHandler> handlerList;
        if (_handlerMap.TryGetValue(packetType, out handlerList))
        {
            handlerList.Add(handler);
        }
        else
        {
            handlerList = new List<VRNMessageHandler>();
            handlerList.Add(handler);
            _handlerMap[packetType] = handlerList;
        }

    }

    public void UnregisterHandler(VRNPacketType packetType, VRNMessageHandler handler)
    {
        if (_handlerMap == null)
            return;

        List<VRNMessageHandler> handlerList;
        if (_handlerMap.TryGetValue(packetType, out handlerList))
        {
            if (handlerList == null)
                return;

            for (int i = 0; i < handlerList.Count; i++)
            {
                if (handlerList[i] == handler)
                {
                    handlerList.RemoveAt(i);
                    break;
                }
            }
        }
    }


    public bool IsClientConnected(int clientID)
    {
        return _relayClients.ContainsKey(clientID);

    }

    //check if the object id is on the destroyed object list
    public bool CheckObjectDestroyed(Guid objid)
    {
        return _destroyedObjects.Contains(objid);
    }


    public void PauseSyncUpdates(bool paused)
    {
        _syncUpdatesPaused = paused;
    }

    public void ClearNetworkObjectCache()
    {
        _netObjects.Clear();
    }


    protected override void OnClientConnected(NetworkConnection client)
    {
        if (IsServer)
        {
            //set DM to client ID 0 on relay
            var msg = new VRNAssignClientInfo { ClientID = 0 };
            SendNetMessage(VRNPacketType.AssignClientInfo, msg, reliable: true);

            //reset network for connected clients
            SendNetMessage(VRNPacketType.NetworkReset, msg: null, reliable: true, broadcast: true);

            BroadcastStateData();

            //state data may not contain sim state
            if (!_stateData.ContainsKey("SimState"))
            {
                SendSimStateChanged(_simState);
            }
        }
        else
        {
            Debug.Log($"Connected to server");

            if (!_sentJoinRequest)
            {
                //SendRequestJoinWorld(SystemInfo.deviceName);
                SendRequestJoinServer();
                _sentJoinRequest = true;
            }
        }
    }

    protected override void OnClientDisconnected(NetworkConnection client)
    {
        if (IsServer)
        {
            Debug.Log($"NetworkManager: Server {ClientID} disconnected");
            ServerDisconnectedFromRelay?.Invoke();
        }
        else
        {
            Debug.Log($"NetworkManager: Client {ClientID} disconnected");
            ResetNetworkState();
        }
    }

    void ResetNetworkState()
    {
        if (IsServer)
            return;

        Debug.Log($"NetManager: Resetting network state");

        var wasConnected = _clientIDValid;

        _clientIDValid = false;
        _sentJoinRequest = false;
        //_clientID = -1;
        ClientID = -1;

        if (_simState != null)
            _simState.ActiveSceneID = -1;
        _sceneID = -1;

        if (wasConnected)
        {
            DisconnectedFromServer?.Invoke();
            SceneIDChanged?.Invoke(_sceneID);
        }

        //if we are still connected, send the join request
        if (IsConnected)
        {
            SendRequestJoinServer();
        }
    }

    unsafe protected override void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline)
    {
        NativeArray<byte> buffer = new NativeArray<byte>(reader.Length, Allocator.Temp);

        reader.ReadBytes(buffer);

        //buffer.CopyTo(_receiveBuffer);

        fixed (byte* unsafeBuffer = _receiveBuffer)
        {
            UnsafeUtility.MemCpy(unsafeBuffer, buffer.GetUnsafePtr<byte>(), reader.Length);
        }

        ProcessNetworkMsg(client, _receiveBuffer, buffer.Length);
    }


    private MemoryStream _recvStream;
    private CodedInputStream _codedRecvStream;

    void ProcessNetworkMsg(NetworkConnection conn, byte[] data, int length)
    {


        //_codedRecvStream = new CodedInputStream(dataReader.RawData, dataReader.UserDataOffset, dataReader.AvailableBytes);
        _codedRecvStream = new CodedInputStream(data, 0, length);


        try
        {

            _vrnHeader.PacketType = VRNPacketType.Unknown;

            _codedRecvStream.ReadMessage(_vrnHeader);

            if (_packetRecvStats != null)
                LogRecvPacketStats(_vrnHeader.PacketType.ToString(), length);

            int msgStartPos = (int)_codedRecvStream.Position;
            //int msgSize = dataReader.AvailableBytes - ((int)_codedRecvStream.Position - (int)dataReader.UserDataOffset);
            int msgSize = length - ((int)_codedRecvStream.Position);
            var msgStream = new MemoryStream(data, msgStartPos, msgSize);

            if (IsRecording && _vrnHeader.PacketType != VRNPacketType.Ping)
            {
                _sessionLog.WriteLog(_vrnHeader.PacketType, msgStream,
                    broadcast: _vrnHeader.PacketDest == VRNPacketDestination.Broadcast);
                msgStream.Position = 0;
            }

            /*
            //replicate packet if necessary
            if (_isServer && _vrnHeader.PacketDest == VRNPacketDestination.Broadcast)
            {
                SendNetMessage(_vrnHeader.PacketType, msgStream, null, fromPeer, method: deliveryMethod);
                msgStream.Position = 0;
            }
            else if (_isServer && _vrnHeader.PacketDest == VRNPacketDestination.Direct)
            {
                if (_vrnHeader.DestClientID != 0)
                {
                    ClientInfo client;
                    if (_connectedClients.TryGetValue(_vrnHeader.DestClientID, out client))
                    {
                        SendNetMessage(_vrnHeader.PacketType, msgStream, clientID: client.clientID, reliable: true);
                        msgStream.Position = 0;
                    }
                }
            }*/

            List<VRNMessageHandler> handlerList;
            if (_handlerMap.TryGetValue(_vrnHeader.PacketType, out handlerList))
            {

                //DEBUG: make sure this works (shouldn't be needed)
                _codedRecvStream = new CodedInputStream(data, msgStartPos, msgSize);
                //_codedRecvStream = new CodedInputStream(dataReader.RawData, msgStartPos, msgSize);

                for (int i = 0; i < handlerList.Count; i++)
                {
                    var handler = handlerList[i];
                    //handler(_vrnHeader, _codedRecvStream, -1); //TODO: Get Client ID
                    handler(_vrnHeader, _codedRecvStream, _vrnHeader.FromClientID);

                    if (i < handlerList.Count - 1)
                    {
                        //only recreate the CodedInputStream if there are additional handlers
                        _codedRecvStream = new CodedInputStream(data, msgStartPos, length);
                    }
                }

                msgStream.Position = 0;
            }

            var recvStream = msgStream;

            switch (_vrnHeader.PacketType)
            {

                case VRNPacketType.Ping:
                    SendNetMessage(VRNPacketType.PingResponse, _pingReponse, reliable: false, broadcast: false);
                    break;

                case VRNPacketType.SendClientState:
                    var clstate = VRNClientState.Parser.ParseDelimitedFrom(recvStream);
                    ClientStateChanged?.Invoke(clstate);
                    break;



                case VRNPacketType.ClientConnected:
                    var clientConStatus = VRNClientConnectionStatus.Parser.ParseDelimitedFrom(recvStream);
                    HandleClientConnected(clientConStatus);
                    break;

                case VRNPacketType.ClientDisconnected:
                    var clientDisconnectStatus = VRNClientConnectionStatus.Parser.ParseDelimitedFrom(recvStream);
                    HandleClientDisconnected(clientDisconnectStatus);
                    break;

                case VRNPacketType.SendClientList:
                    throw new System.NotImplementedException();

                case VRNPacketType.LoadScene:

                    var loadScene = VRNLoadScene.Parser.ParseDelimitedFrom(recvStream);



                    HandleLoadScene(loadScene);
                    break;


                case VRNPacketType.AssignPlayerRole:

                    break;

                case VRNPacketType.TextMessage:
                    var msg = VRNTextMessage.Parser.ParseDelimitedFrom(recvStream);
                    Debug.Log($"Received Text Message: {msg.Message}");
                    break;

                case VRNPacketType.StateDataUpdate:
                    var stateUpdate = VRNStateDataUpdate.Parser.ParseDelimitedFrom(recvStream);
                    HandleStateDataUpdate(stateUpdate);
                    break;

                case VRNPacketType.RequestWorldState:
                    Debug.Log($"Received RequestWorldState");
                    var reqWorldState = VRNRequestWorldState.Parser.ParseDelimitedFrom(recvStream);
                    ClientRequestedWorldState?.Invoke(reqWorldState.ClientID);
                    break;

                case VRNPacketType.SetNetworkUpdateRate:
                    Debug.Log($"Received SetNetworkUpdateRate");
                    var netUpdateRate = VRNNetworkUpdateRate.Parser.ParseDelimitedFrom(recvStream);
                    HandleSetNetworkUpdateRate(netUpdateRate);
                    break;

                case VRNPacketType.RelayClientStats:
                    var clientStatsList = VRNRelayClientStatsList.Parser.ParseDelimitedFrom(recvStream);
                    UpdateClientStats(clientStatsList);
                    break;
            }

            if (_isServer)
            {
                //ClientInfo clientInfo;
                //int clientID = -1;
                VRNRelayClientList relayClientList;

                switch (_vrnHeader.PacketType)
                {
                    case VRNPacketType.NetObjectSync:
                        break;

                    case VRNPacketType.ClientLoadedScene:
                        BroadcastActiveNetObjects();

                        //if (_peerMap.TryGetValue(conn, out clientInfo))
                        //{
                        //    clientID = clientInfo.clientID;
                        //}
                        ClientJoinedScene?.Invoke(_vrnHeader.FromClientID);
                        break;

                    case VRNPacketType.RequestJoinServer:
                        var rjs = VRNRequestJoinServer.Parser.ParseDelimitedFrom(recvStream);

                        //NOTE: This is handled by the relay only

                        break;



                    case VRNPacketType.RequestWorldState:
                        BroadcastActiveNetObjects();
                        break;

                    case VRNPacketType.LogEvent:
                        var logEv = VRNLogEvent.Parser.ParseDelimitedFrom(recvStream);
                        WriteSessionEvent(logEv);
                        break;

                    case VRNPacketType.AssignClientInfo:

                        var aci = VRNAssignClientInfo.Parser.ParseDelimitedFrom(recvStream);
                        Debug.Log($"NetManager({gameObject.name}): Server ClientID Assigned {aci.ClientID}");
                        break;

                    case VRNPacketType.RelayClientJoined:
                        relayClientList = VRNRelayClientList.Parser.ParseDelimitedFrom(recvStream);
                        HandleRelayClientJoined(relayClientList);
                        break;

                    case VRNPacketType.RelayClientLeft:
                        relayClientList = VRNRelayClientList.Parser.ParseDelimitedFrom(recvStream);
                        RemoveDisconnectedClients(relayClientList);
                        break;

                    case VRNPacketType.RelayClientListChanged:
                        relayClientList = VRNRelayClientList.Parser.ParseDelimitedFrom(recvStream);
                        RemoveDisconnectedClients(relayClientList);
                        break;
                }
            }
            else
            {
                switch (_vrnHeader.PacketType)
                {
                    case VRNPacketType.AssignClientInfo:

                        var aci = VRNAssignClientInfo.Parser.ParseDelimitedFrom(recvStream);

                        if (_clientIDValid)
                        {
                            if (aci.ClientID != ClientID)
                                Debug.LogError($"NetManager: Received client id assignment of {aci.ClientID} but already assigned {ClientID}");
                            else
                                Debug.Log($"NetManager: Received repeat client ID assignment: {ClientID} ");
                            break;
                        }


                        //_clientID = (short)aci.ClientID;
                        ClientID = aci.ClientID;
                        _clientIDValid = true;

                        Debug.Log($"NetManager: ClientID Assigned {ClientID}");
                        ClientIDAssigned?.Invoke(ClientID);

                        var name = GetMultiplayerName();

                        var clientState = new VRNClientState
                        {
                            PlayerName = name,
                            SceneLoadState = VRNSceneLoadState.None,
                            SceneName = "",
                        };

                        SendVRClientState(clientState);
                        break;

                    case VRNPacketType.NetworkReset:
                        ResetNetworkState();
                        break;

                    case VRNPacketType.RelayClientLeft:
                        var relayClientList = VRNRelayClientList.Parser.ParseDelimitedFrom(recvStream);
                        bool dmPresent = false;
                        foreach (var client in relayClientList.ClientList)
                        {
                            if (client.ClientID == 0)
                                dmPresent = true;
                        }
                        if (!dmPresent && _clientIDValid)
                            ResetNetworkState();
                        break;
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing packet {_vrnHeader.PacketType.ToString()}, {ex.Message}, stack: {ex.StackTrace}");
        }

    }

    void HandleClientConnected(VRNClientConnectionStatus status)
    {


    }

    void HandleClientDisconnected(VRNClientConnectionStatus status)
    {
        //PlayerManager.RemovePlayer(status.ClientID);
        ClientDisconnected?.Invoke(status.ClientID);
    }

    void HandleRelayClientJoined(VRNRelayClientList relayClientList)
    {
        if (!IsServer)
            return;

        foreach (var client in relayClientList.ClientList)
        {
            if (!_relayClients.ContainsKey(client.ClientID))
            {
                _relayClients.Add(client.ClientID, client);
                SendClientConnected(client.ClientID);

                BroadcastStateData();
                ClientJoinedServer?.Invoke(client.ClientID);
            }
        }
    }

    void UpdateRelayClientList(VRNRelayClientList relayClientList)
    {
        //_relayClients = relayClientList;
        _relayClients.Clear();
        foreach (var client in relayClientList.ClientList)
        {
            _relayClients.Add(client.ClientID, client);
        }
    }

    void UpdateClientStats(VRNRelayClientStatsList clientList)
    {
        _clientStats.Clear();
        foreach (var clientStats in clientList.ClientStats)
        {
            _clientStats.Add(clientStats.ClientID, clientStats);
        }
    }

    public VRNClientStats GetClientStats(int clientID)
    {
        return _clientStats[clientID];
    }

    void RemoveDisconnectedClients(VRNRelayClientList relayClientList)
    {
        if (_relayClients == null)
            return;

        HashSet<int> connectedClients = new HashSet<int>();
        List<int> disconnectedClients = new List<int>(5);

        foreach (var relayClient in relayClientList.ClientList)
        {
            connectedClients.Add(relayClient.ClientID);
        }

        foreach (var oldClient in _relayClients.Values)
        {
            if (!connectedClients.Contains(oldClient.ClientID))
            {
                disconnectedClients.Add(oldClient.ClientID);
                //Debug.Log($"Client {oldClient.ClientID} disconnected from relay");
                //SendClientDisconnected(oldClient.ClientID);
            }
        }

        UpdateRelayClientList(relayClientList);

        foreach (var clientID in disconnectedClients)
        {
            Debug.Log($"Client {clientID} disconnected from relay");
            SendClientDisconnected(clientID);
        }
    }

    void HandleStateDataUpdate(VRNStateDataUpdate stateUpdate)
    {
        if (stateUpdate == null || stateUpdate.StateID == null || stateUpdate.StateID.Length <= 0)
            return;

        //Debug.Log("Received state update");

        _stateData[stateUpdate.StateID] = stateUpdate;

        StateDataChanged?.Invoke(stateUpdate);

        //handle special case SimState state
        if (stateUpdate.StateID == "SimState")
        {
            try
            {
                var simState = VRNSimState.Parser.ParseFrom(stateUpdate.StateData);
                _simState = simState;
                var oldID = _sceneID;

                _sceneID = _simState.ActiveSceneID;
                if (_sceneID != oldID)
                {
                    try
                    {
                        SceneIDChanged?.Invoke(_sceneID);
                    }
                    catch (Exception) { }
                }

                Debug.Log($"Received state update {simState.ActiveScene}");

                SimStateChanged?.Invoke(simState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Bad sim state update : {ex.Message}");
            }
        }
    }

    void HandleSetNetworkUpdateRate(VRNNetworkUpdateRate netUpdateRate)
    {
        SetNetworkUpdateRate(netUpdateRate.ObjectUpdateRate, netUpdateRate.VRUpdateRate);
    }

    public void SetNetworkUpdateRate(float objectUpdateRate, float vrUpdateRate, bool sendToNetwork = false)
    {
        if (SystemManager == null || SystemManager.SystemConfig == null)
        {
            Debug.LogError($"NetworkManager: System config missing or not initialized");
            return;
        }

        SystemManager.SystemConfig.MPObjectUpdateRateHz = objectUpdateRate;
        SystemManager.SystemConfig.MPVRUpdateRateHz = vrUpdateRate;

        UpdateSyncInterval();

        NetworkUpdateRateChanged?.Invoke();

        if (sendToNetwork)
        {
            VRNNetworkUpdateRate msg = new VRNNetworkUpdateRate();
            msg.ObjectUpdateRate = objectUpdateRate;
            msg.VRUpdateRate = vrUpdateRate;
            SendNetMessage(VRNPacketType.SetNetworkUpdateRate, msg, reliable: true);
        }
    }

    public VRNSimState SimState
    {
        get
        {
            return _simState;
        }
    }

    public void SendRequestWorldState()
    {
        var msg = new VRNRequestWorldState()
        {
            ClientID = this.ClientID,
        };


        if (IsServer)
        {
            ClientRequestedWorldState?.Invoke(ClientID);
            SendNetMessage(VRNPacketType.RequestWorldState, msg);
        }
        else
        {
            SendNetMessage(VRNPacketType.RequestWorldState, msg, broadcast: false);
        }
    }

    void SendSimStateChanged(VRNSimState simState)
    {
        _simState = simState;
        SendStateDataUpdate("SimState", simState);
    }

    void SendStateDataUpdate(string stateID, IMessage stateDataMsg)
    {
        var stream = new MemoryStream(500);
        stateDataMsg.WriteTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        SendStateDataUpdate(stateID, stream);
    }

    void SendStateDataUpdate(string stateID, Stream stateData)
    {
        var byteData = ByteString.FromStream(stateData);

        VRNStateDataUpdate stateMsg = new VRNStateDataUpdate
        {
            StateID = stateID,
            ClientIDSource = ClientID,
            StateData = byteData,
        };

        SendStateDataUpdate(stateMsg);
    }

    void SendStateDataUpdate(VRNStateDataUpdate stateUpdate, bool updateLocalState = true)
    {
        if (updateLocalState)
            _stateData[stateUpdate.StateID] = stateUpdate;

        Debug.Log($"Sending state update {stateUpdate.StateID} with {stateUpdate.StateData.Length} bytes");
        SendNetMessage(VRNPacketType.StateDataUpdate, stateUpdate);
    }

    void HandleLoadScene(VRNLoadScene loadScene)
    {
        if (!_isServer)
        {
            Debug.LogError($"Error: LoadScene message received on client");
            return;
        }

        Debug.Log($"Loading scene {loadScene.SceneName}");
        SceneLoadCommand?.Invoke(loadScene);

        if (_simState == null)
            _simState = new VRNSimState();

        _sceneID++;
        _simState.ActiveScene = loadScene.SceneName;
        _simState.ActiveSceneID = _sceneID;

        try
        {
            SceneIDChanged?.Invoke(_sceneID);
        }
        catch (Exception) { }

        SendSimStateChanged(_simState);

    }


    void SpawnObject(AssetReference asset, System.Guid objID, Vector3 startPos, Quaternion startRot, bool spawnActivated = false)
    {
        //this doesn't work - need to convert from AssetReference to string id
        throw new System.NotImplementedException();
        //SpawnObject(asset.AssetGUID, objID, startPos, startRot, spawnActivated);
    }

    public async void SpawnObject(string assetID, System.Guid objID, Vector3 startPos, Quaternion startRot, bool spawnActivated = false)
    {
        await NetworkedObjectManager.SpawnObject(assetID, objID, startPos, startRot, spawnActivated);

    }

    public void DestroyObject(System.Guid objID)
    {
        NetworkedObjectManager.DestroyObject(objID);

    }

    public NetworkedObject GetNetworkObject(System.Guid guid)
    {
        return NetworkedObjectManager.GetNetworkObject(guid);

    }

    public void SendVRClientState(VRNClientState state)
    {
        state.ClientID = ClientID;
        //state.PlayerID = _playerID;
        SendNetMessage(VRNPacketType.SendClientState, state);
    }


    //public void SendVRPlayerInfo(VRNVRPlayerInfo info, NetPeer excludePeer = null, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
    //{

    //    SendNetMessage(VRNPacketType.VrplayerInfo, info, null, excludePeer);
    //}


    public void SendPlayerList(VRNPlayerList list)
    {
        SendNetMessage(VRNPacketType.SendPlayerList, list);
    }

    protected void BroadcastStateData()
    {
        if (_stateData == null)
            return;

        Debug.Log($"Broadcasting {_stateData.Count} state data packets");

        foreach (var state in _stateData.Values)
        {
            SendStateDataUpdate(state, false);
        }
    }

    protected void BroadcastActiveNetObjects()
    {

    }


    public void SendClientLoadedScene()
    {
        VRNAssignClientInfo clientInfo = new VRNAssignClientInfo
        {
            ClientID = ClientID,
            //PlayerID = PlayerID,
        };

        SendNetMessage(VRNPacketType.ClientLoadedScene, clientInfo);
    }


    protected void SendClientConnected(int clientID)
    {
        VRNClientConnectionStatus status = new VRNClientConnectionStatus
        {
            ClientID = clientID,
            Connected = true,
        };

        SendNetMessage(VRNPacketType.ClientConnected, status);
        HandleClientConnected(status);
    }

    protected void SendClientDisconnected(int clientID)
    {
        VRNClientConnectionStatus status = new VRNClientConnectionStatus
        {
            ClientID = clientID,
            Connected = false,
        };

        SendNetMessage(VRNPacketType.ClientDisconnected, status);
        HandleClientDisconnected(status);
    }

    public string GetMultiplayerName()
    {
        var name = SystemManager.SystemConfig.MultiplayerName;
        if (name == null || name.Length <= 0)
            name = SystemInfo.deviceName;

        if (OverrideMultiplayerName)
        {
            if (transform.parent != null)
                name = transform.parent.name;
            else
            {
                int randomVal = (int)(UnityEngine.Random.value * 100.0f);
                name = $"Client{randomVal}";
            }
        }

        return name;
    }

    public void SendRequestJoinServer()
    {
        var name = GetMultiplayerName();

        SendNetMessage(VRNPacketType.RequestJoinServer, new VRNRequestJoinServer
        {
            ClientName = name,
        }, broadcast: false, reliable: true);
    }

    public void SendLoadScene(string sceneName, bool allowSceneActivation)
    {
        //_sceneID++;
        Debug.Log($"Sending load scene {sceneName}");
        var msg = new VRNLoadScene
        {
            SceneName = sceneName,
            AllowSceneActivation = allowSceneActivation,
            SceneID = -1,
        };

        if (!_isServer)
        {
            SendNetMessage(VRNPacketType.LoadScene, msg, broadcast: false);
        }
        HandleLoadScene(msg);
    }

    private void LogSendPacketStats(string packetType, int size)
    {
        if (_packetSendStats == null)
            return;

        LogPacketStats(packetType, size, _packetSendStats);
    }

    private void LogRecvPacketStats(string packetType, int size)
    {
        if (_packetRecvStats == null)
            return;

        LogPacketStats(packetType, size, _packetRecvStats);
    }

    private void LogPacketStats(string packetType, int size, Dictionary<string, PacketStats> logDictionary)
    {
        PacketStats stats;
        if (logDictionary.TryGetValue(packetType, out stats))
        {
            stats.PacketCount = stats.PacketCount + 1;
            stats.TotalBytes = stats.TotalBytes + size;
            if (size > stats.MaxPacketSize)
                stats.MaxPacketSize = size;
        }
        else
        {
            stats = new PacketStats();
            stats.PacketCount = 1;
            stats.PacketType = packetType;
            stats.TotalBytes = size;
            stats.MaxPacketSize = size;
        }

        logDictionary[packetType] = stats;
    }

    public void SendTextMessage(string message)
    {

        VRNTextMessage msg = new VRNTextMessage
        {
            Message = message,
        };

        SendNetMessage(VRNPacketType.TextMessage, msg);
    }

    protected override void OnPacketSent(VRNPacketType packetType, IMessage msg, bool broadcast, int size)
    {
        base.OnPacketSent(packetType, msg, broadcast, size);

        if (_sessionLog != null)
        {
            _sessionLog.WriteLog(packetType, msg, broadcast);
        }

        LogSendPacketStats(packetType.ToString(), size);
    }

    //protected override void OnPacketSent(MemoryStream data, bool broadcast, int size)
    //{
    //    base.OnPacketSent(data, broadcast, size);

    //    if (_sessionLog != null)
    //    {
    //        _sessionLog.WriteLogNoHeader(data, broadcast);
    //    }
    //}

    protected override void OnPacketSent(VRNPacketType packetType, MemoryStream data, bool broadcast, int size)
    {
        base.OnPacketSent(packetType, data, broadcast, size);

        if (_sessionLog != null)
        {
            _sessionLog.WriteLog(packetType, data, broadcast);
        }

        if (data != null)
            LogSendPacketStats(packetType.ToString(), size + (int)data.Length);
        else
            LogSendPacketStats(packetType.ToString(), size);
    }

    protected override void OnPacketSent(VRNNetObjMessage netObjHeader, IMessage msg, bool broadcast, int size)
    {
        base.OnPacketSent(netObjHeader, msg, broadcast, size);


        if (_sessionLog != null)
        {
            _sessionLog.WriteLog(netObjHeader, msg, broadcast);
        }

        LogSendPacketStats(VRNPacketType.NetObjectMessage.ToString(), size);
    }

    private StringBuilder _sbPacketStats = new StringBuilder();
    protected override void Update()
    {
        base.Update();

        AvgFrameBytes = 0.75 * AvgFrameBytes + 0.25 * _frameBytesSent;
        AvgFramePackets = 0.75 * AvgFramePackets + 0.25 * _framePacketsSent;

        LastFrameBytesSent = (int)_frameBytesSent;
        LastFramePacketsSent = _framePacketsSent;

        _bytesPerSecond += LastFrameBytesSent;

        if (Time.time - _lastPerSecondReset >= 1.0f)
        {
            LastBytesPerSecond = (int)_bytesPerSecond;
            AvgBytesPerSecond = 0.75 * AvgBytesPerSecond + 0.25 * _bytesPerSecond;

            _bytesPerSecond = 0;
            _lastPerSecondReset = Time.time;
        }

        _frameBytesSent = 0;
        _framePacketsSent = 0;

        float elapsed = 0;
        //_netManager?.PollEvents();


        if (!IsPlaybackMode)
        {
            elapsed = Time.unscaledTime - _lastNetSyncSent;
            if (elapsed > NetworkSyncInterval)
            {
                _lastNetSyncSent = Time.unscaledTime;
                //send object state for objects that we have authority on, if necessary
                FrameSyncCount = NetworkedObjectManager.SendObjectSyncData();
                if (FrameSyncCount > FrameSyncCountMax)
                    FrameSyncCountMax = FrameSyncCount;

            }
        }


        if (AutomaticReconnect && (!_clientConnection.IsCreated || _clientConnection.GetState(NetworkDriver) == NetworkConnection.State.Disconnected))
        {
            ConnectToServer();
        }

        //if (_packetSendStats != null)
        //{
        //    _sbPacketStats.Clear();
        //    _sbPacketStats.AppendLine("Packet Stats");
        //    foreach (var kvp in _packetSendStats)
        //    {
                
        //        var packetSize = kvp.Value.TotalBytes / kvp.Value.PacketCount;
        //        _sbPacketStats.AppendFormat("{0,20}: {1} {2}\n", kvp.Value.PacketType, kvp.Value.PacketCount, packetSize);
        //    }

        //    Debug.Log(_sbPacketStats.ToString());
        //}

    }

    public int GetClientID()
    {
        return ClientID;
    }


}