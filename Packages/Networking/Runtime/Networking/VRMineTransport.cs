using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using Unity.Networking.Transport.Error;
using Google.Protobuf;

public abstract class VRMineTransport : MonoBehaviour
{
    public const int MaxPayloadSize = 20000;
    public const int MaxQueueSize = 1000;

    public NetworkDriver NetworkDriver;

    public int ClientID = -1;
    public bool IsBeingDestroyed {  get { return _destroyInProgress; } }

    protected NetworkConnection _clientConnection;
    //protected NativeList<NetworkConnection> _connections;
    protected List<ConnectionInfo> _connections;

    protected NetworkPipeline _reliablePipeline;
    protected NetworkPipeline _unreliablePipeline;
    protected NetworkPipeline _basePipeline;
    protected NetworkPipeline _criticalPipeline;

    protected Dictionary<NetworkPipeline, string> _pipelineNames;

    private VRNHeader _vrnSendHeader = new VRNHeader();
    private MemoryStream _sendBuffer;
    private MemoryStream _msgBuffer;
    private CodedOutputStream _codedSendBuffer;
    private TransportSendQueue _reliablePacketQueue;

    //private NativeArray<byte> _buffer;

    public bool SimulateLatency = false;

    private bool _initialized = false;
    private MovingAverage _sendBytesPerSecondAvg = new MovingAverage(8);
    private int _frameBytesCount;
    private int _secondBytesCount;
    private int _sampleSecond;
    protected bool _destroyInProgress = false;


    protected struct ConnectionInfo
    {
        public NetworkConnection conn;
        public TransportSendQueue reliablePacketQueue;
        public uint sendErrors;
    }

    public bool IsConnected
    {
        get
        {
            if (!_clientConnection.IsCreated || !NetworkDriver.IsCreated)
                return false;

            return _clientConnection.GetState(NetworkDriver) == NetworkConnection.State.Connected;
        }
    }

    public float AverageBytesPerSecond
    {
        get
        {
            return _sendBytesPerSecondAvg.Average;
        }
    }

    protected virtual void Awake()
    {
        _pipelineNames = new Dictionary<NetworkPipeline, string>();

        _sendBuffer = new MemoryStream(MaxPayloadSize);
        _msgBuffer = new MemoryStream(MaxPayloadSize);
        _codedSendBuffer = new CodedOutputStream(_sendBuffer);
        _reliablePacketQueue = new TransportSendQueue(MaxPayloadSize, MaxQueueSize);
    }

    protected virtual void InitializeNetwork()
    {
        if (_initialized)
            return;

        NetworkSettings settings = new NetworkSettings();
         
        settings.WithFragmentationStageParameters(32000);
        settings.WithReliableStageParameters(32);
        
        settings.WithBaselibNetworkInterfaceParameters(2048, 4096, 4096);
        //settings.WithBaselibNetworkInterfaceParameters(64, 64, 4096);

        settings.WithNetworkConfigParameters(disconnectTimeoutMS: 8000); 

        if (SimulateLatency)
            settings.WithSimulatorStageParameters(30, 256, 100, 15);

        //settings.WithDataStreamParameters(100000);
        //settings.WithPipelineParameters()
        NetworkDriver = NetworkDriver.Create(settings);
        //_buffer = new NativeArray<byte>()
        //_connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        _connections = new List<ConnectionInfo>(32);

        if (SimulateLatency)
        {
            Debug.Log($"VRMineTransport: Simulating latency on {gameObject.name}");
            _reliablePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(ReliableSequencedPipelineStage),
                typeof(SimulatorPipelineStage));
            _unreliablePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(UnreliableSequencedPipelineStage),
                typeof(SimulatorPipelineStage));
            _basePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(SimulatorPipelineStage));
            _criticalPipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(ReliableSequencedPipelineStage),
                typeof(SimulatorPipelineStage));
        }
        else
        {
            _reliablePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(ReliableSequencedPipelineStage));
            _unreliablePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(UnreliableSequencedPipelineStage));
            _basePipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage));
            _criticalPipeline = NetworkDriver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(ReliableSequencedPipelineStage));
        }


        _pipelineNames.Add(_reliablePipeline, "Reliable");
        _pipelineNames.Add(_unreliablePipeline, "Unreliable");
        _pipelineNames.Add(_basePipeline, "Base");
        _pipelineNames.Add(_criticalPipeline, "Critical");

        _initialized = true;

    }

    protected virtual void Start()
    {
        InitializeNetwork();
    }

    protected virtual void OnDestroy()
    {
        _destroyInProgress = true;
        _reliablePacketQueue.Dispose();
        NetworkDriver.Dispose();
        //_connections.Dispose();

        if (_connections != null)
        {
            foreach (var connData in _connections)
            {
                connData.reliablePacketQueue.Dispose();
            }

            _connections.Clear();
        }
    }

    
    protected abstract void OnClientConnected(NetworkConnection client);
    protected abstract void OnClientDisconnected(NetworkConnection client);
    protected abstract void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline);

    protected virtual void OnPacketSent(VRNPacketType packetType, IMessage msg, bool broadcast, int size) { }
    protected virtual void OnPacketSent(VRNPacketType packetType, MemoryStream data, bool broadcast, int size) { }
    protected virtual void OnPacketSent(MemoryStream data, bool broadcast, int size) { }
    protected virtual void OnPacketSent(VRNNetObjMessage netObjHeader, IMessage msg, bool broadcast, int size) { }

    protected virtual void Update()
    {
        NetworkConnection conn;
        NetworkDriver.ScheduleUpdate().Complete();

        if (NetworkDriver.Listening)
        {
            // CleanUpConnections
            for (int i = 0; i < _connections.Count; i++)
            {
                var connData = _connections[i];
                if (!connData.conn.IsCreated)
                {
                    //_connections.RemoveAtSwapBack(i);
                    RemoveConnection(i);
                    --i;
                }

                //var connState = connData.conn.GetState(NetworkDriver);
                //if (connState == NetworkConnection.State.)
            }

            // AcceptNewConnections
            NetworkConnection c;
            while ((c = NetworkDriver.Accept()) != default(NetworkConnection))
            {
                //_connections.Add(c);
                ConnectionInfo connData = new ConnectionInfo
                {
                    conn = c,
                    reliablePacketQueue = new TransportSendQueue(MaxPayloadSize, MaxQueueSize),
                    sendErrors = 0,
                };
                _connections.Add(connData);

                var remoteAddr = NetworkDriver.RemoteEndPoint(c);
                Debug.Log($"Accepted a connection from {remoteAddr.Address}, {_connections.Count} connected clients");

                OnClientConnected(c);
            }

        }

        if (_clientConnection.IsCreated)
        {
            ProcessConnection(-1, _clientConnection);
        }
        
        for (int i = 0; i < _connections.Count; i++)
        {
            //conn = _connections[i];
            //ProcessConnection(i, conn);
            var connData = _connections[i];
            ProcessConnection(i, connData.conn);
        }
    }

    protected virtual void LateUpdate()
    {
        //Debug.Log($"VRMineTransport: LateUpdate");
        _reliablePacketQueue.SendQueuedPackets(NetworkDriver);

        if (_connections != null)
        {
            foreach (var connData in _connections)
            {
                connData.reliablePacketQueue.SendQueuedPackets(NetworkDriver);
            }
        }

        int curSecond = (int)Time.unscaledTime;
        if (curSecond != _sampleSecond)
        {
            _sendBytesPerSecondAvg.AddSample(_secondBytesCount);
            _sampleSecond = curSecond;
            _secondBytesCount = 0;
        }

        _secondBytesCount += _frameBytesCount;
        _frameBytesCount = 0;
    }

    private void ProcessConnection(int index, NetworkConnection conn)
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        NetworkPipeline pipeline = default(NetworkPipeline);

        while ((cmd = NetworkDriver.PopEventForConnection(conn, out stream, out pipeline)) != NetworkEvent.Type.Empty)
        {
            try
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    OnClientDataMessage(conn, stream, pipeline);
                }
                else if (cmd == NetworkEvent.Type.Connect)
                {
                    OnClientConnected(conn);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    var remoteAddr = NetworkDriver.RemoteEndPoint(conn);

                    Debug.Log($"Client {remoteAddr.Address} disconnected from server");

                    try
                    {
                        OnClientDisconnected(conn);
                    } 
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error in OnClientDisconnect {ex.Message} {ex.StackTrace}");
                    }

                    if (index >= 0)
                    {
                        //_connections[index] = default(NetworkConnection);
                        var connData = _connections[index];
                        connData.conn = default(NetworkConnection);
                        _connections[index] = connData;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"VRMineTransport: Client {ClientID} Error processing connection event {cmd} {ex.Message} | {ex.StackTrace}");
            }
        }
    }

    public void Listen(ushort port)
    {
        var endpoint = NetworkEndPoint.AnyIpv4; // The local address to which the client will connect to is 127.0.0.1
        endpoint.Port = port;
        if (NetworkDriver.Bind(endpoint) != 0)
            Debug.Log($"Failed to bind to port {port}");
        else
            NetworkDriver.Listen();
    }

    public void Connect(string address, ushort port)
    {
        NetworkEndPoint ep = new NetworkEndPoint();

        if (!NetworkEndPoint.TryParse(address, port, out ep))
        {
            //try dns lookup
            var ip = System.Net.Dns.GetHostEntry(address);

            if (ip == null || ip.AddressList == null || ip.AddressList.Length <= 0)
            {
                throw new System.Exception($"Couldn't lookup address \"{address}\"");
            }

            var ipstr = ip.AddressList[0].ToString();
            Debug.Log($"Found ip {ipstr} for address {address}");

            ep = NetworkEndPoint.Parse(ipstr, port);
        }

        Debug.Log($"VRMineTransport: Connecting to {ep.Address} : {ep.Port}");

        Connect(ep);
    }

    public void Connect(NetworkEndPoint endpoint)
    {
        //endpoint.Port = port;
        _clientConnection = NetworkDriver.Connect(endpoint);
    }

    public void Disconnect()
    {
        if (!IsConnected)
            return;
        
        _clientConnection.Disconnect(NetworkDriver);
        _clientConnection = default(NetworkConnection);
    }

    public void Disconnect(NetworkConnection client)
    {
        for (int i = 0; i < _connections.Count; i++)
        {
            var connData = _connections[i];
            if (connData.conn == client)
            {
                connData.conn.Disconnect(NetworkDriver);
                //connData.conn.Close(NetworkDriver);
                RemoveConnection(i);
                --i;
            }
        }
    }
    
    private void RemoveConnection(int index)
    {
        //Debug.Log($"Removing connection {index} current count: {_connections.Count}");
        var connData = _connections[index];
        _connections.RemoveAtSwapBack(index);
        //Debug.Log($"new count: {_connections.Count}");

        if (connData.reliablePacketQueue != null)
        {
            connData.reliablePacketQueue.Dispose();
        }
    }

    public void Send(byte[] data, int length, bool reliable = false, bool sequenced = false, NetworkConnection conn = default(NetworkConnection))
    {
        if (reliable)
            Send(_reliablePipeline, data, length, conn);
        else if (sequenced)
            Send(_unreliablePipeline, data, length, conn);
        else
            Send(_basePipeline, data, length, conn);
    }

    public TransportSendQueue GetSendQueue(NetworkConnection conn)
    {
        //find the reliable packet queue
        for (int i = 0; i < _connections.Count; i++)
        {
            if (_connections[i].conn == conn)
            {
                return _connections[i].reliablePacketQueue;
            }
        }

        return null;
    }

    protected bool Send(NetworkPipeline pipeline, byte[] data, int length, NetworkConnection conn = default(NetworkConnection))
    {
        if (data == null || data.Length < length || length <= 0)
            return false;

        if (length > MaxPayloadSize)
        {
            Debug.LogError($"VRMineTransport: attempted to send packet exceeding max payload size, length: {length}");
        }

        _frameBytesCount += length;

        if (!conn.IsCreated)
            conn = _clientConnection;

        if (pipeline == _reliablePipeline)
        {
            if (conn == _clientConnection)
            {
                //queue the packet for sending instead
                //Debug.Log($"Queueing packet for send {length} bytes");
                _reliablePacketQueue.Enqeuue(pipeline, data, length, conn);
            }
            else
            {
                //find the reliable packet queue
                var sendQueue = GetSendQueue(conn);
                if (sendQueue != null)
                    sendQueue.Enqeuue(pipeline, data, length, conn);
                else
                {
                    Debug.LogError($"VRMineTransport: Couldn't find send queue for connection {conn.InternalId}");
                }
            }
            return true;
        }

        NativeArray<byte> buffer = new NativeArray<byte>(data, Allocator.Temp);
        var slice = buffer.GetSubArray(0, length);


        try
        {
            int result;
            StatusCode statusCode;

            //Debug.Log($"Sending {slice.Length} byte packet to net ID: {conn.InternalId}");

            DataStreamWriter writer;
            result = NetworkDriver.BeginSend(pipeline, conn, out writer);
            if (result < 0)
            {
                statusCode = (StatusCode)result;
                Debug.LogError($"VRMineTransport: Client {ClientID} Error sending packet {statusCode.ToString()}");
                return false;
                //throw new System.Exception($"VRMineTransport: Error sending packet {statusCode.ToString()}");
            }

            writer.WriteBytes(slice);
            result = NetworkDriver.EndSend(writer);
            if (result < 0)
            {
                statusCode = (StatusCode)result;

                Debug.LogError($"VRMineTransport: Client {ClientID} Error sending packet {statusCode.ToString()}");

                return false;
                //throw new System.Exception($"VRMineTransport: Error sending packet {statusCode.ToString()}");
            }
        }
        finally
        {
            buffer.Dispose();
        }

        return true;
    }


    public void SendNetMessage(VRNPacketType packetType, IMessage msg, bool reliable = true, bool broadcast = true, int clientID = -1, NetworkConnection conn = default(NetworkConnection))
    {
        if (!conn.IsCreated)
            conn = _clientConnection;

        WriteHeader(_codedSendBuffer, packetType, broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct, clientID);

        if (msg != null)
        {
            //msg.WriteDelimitedTo(_sendBuffer);
            _codedSendBuffer.WriteMessage(msg);
            _codedSendBuffer.Flush();
        }

        OnPacketSent(packetType, msg, broadcast, (int)_sendBuffer.Length);

        //Debug.Log($"Sending {packetType.ToString()}, bytes: {_sendBuffer.Length}");

        SendBufferContents(conn, reliable, broadcast);
    }

    public void SendNetMessage(VRNPacketType packetType, MemoryStream data, bool reliable = true, bool broadcast = true, int clientID = -1)
    {
        if (broadcast)
            WriteHeader(_codedSendBuffer, packetType, VRNPacketDestination.Broadcast);
        else
            WriteHeader(_codedSendBuffer, packetType, VRNPacketDestination.Direct, clientID);

        long dataPos = data.Position;
        OnPacketSent(packetType, data, broadcast, (int)_sendBuffer.Length);
        data.Position = dataPos;

        CopyData(data, _sendBuffer);

        //LogSendPacketStats(packetType.ToString(), (int)_sendBuffer.Length);

        SendBufferContents(_clientConnection, reliable, broadcast);
    }

    public void SendNetObjMessage(System.Guid objId, string messageType, System.Action<CodedOutputStream> sendCallback, bool reliable = true, bool broadcast = true, int clientID = -1)
    {
        WriteHeader(_codedSendBuffer, VRNPacketType.NetObjectMessage, broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct, clientID);

        VRNNetObjMessage netObjHeader = new VRNNetObjMessage();
        netObjHeader.ObjID = ByteString.CopyFrom(objId.ToByteArray());
        netObjHeader.MessageType = messageType;

        _codedSendBuffer.WriteMessage(netObjHeader);
        sendCallback(_codedSendBuffer);
        _codedSendBuffer.Flush();

        _msgBuffer.Position = 0;
        OnPacketSent(VRNPacketType.NetObjectMessage, _msgBuffer, broadcast, (int)_sendBuffer.Length);

        SendBufferContents(_clientConnection, reliable, broadcast);
    }

    public void SendNetObjMessage(System.Guid objId, string messageType, IMessage msg, bool reliable = true, bool broadcast = true, int clientID = -1)
    {

        WriteHeader(_codedSendBuffer, VRNPacketType.NetObjectMessage, broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct, clientID);

        VRNNetObjMessage netObjHeader = new VRNNetObjMessage();
        netObjHeader.ObjID = ByteString.CopyFrom(objId.ToByteArray());
        netObjHeader.MessageType = messageType;

        //netObjHeader.WriteDelimitedTo(_sendBuffer);
        //msg.WriteDelimitedTo(_sendBuffer);
        _codedSendBuffer.WriteMessage(netObjHeader);
        _codedSendBuffer.WriteMessage(msg);
        _codedSendBuffer.Flush();

        //Debug.Log($"Sending {packetType.ToString()}, bytes: {_sendBuffer.Length}");

        OnPacketSent(netObjHeader, msg, broadcast, (int)_sendBuffer.Length);

        SendBufferContents(_clientConnection, reliable, broadcast);
    }


    private byte[] _copyBuffer;

    // Reimplentation of Stream.CopyTo using fixed buffer
    protected void CopyData(Stream fromStream, Stream destination)
    {
        if (_copyBuffer == null)
            _copyBuffer = new byte[1024];

        int count;
        while ((count = fromStream.Read(_copyBuffer, 0, _copyBuffer.Length)) != 0)
        {
            destination.Write(_copyBuffer, 0, count);
        }
    }

    protected void SendBufferContents(NetworkConnection conn, bool reliable = true, bool broadcast = true)
    {

        if (_destroyInProgress || !conn.IsCreated || !(conn.GetState(NetworkDriver) == NetworkConnection.State.Connected))
            return;


        Send(reliable ? _reliablePipeline : _basePipeline,
            _sendBuffer.GetBuffer(), (int)_sendBuffer.Length, conn);

       
    }

    protected void WriteHeader(CodedOutputStream stream, VRNPacketType packetType, VRNPacketDestination dest, int destClientID = 0)
    {
        _sendBuffer.Position = 0;
        _sendBuffer.SetLength(0);


        _vrnSendHeader.PacketType = packetType;
        _vrnSendHeader.PacketDest = dest;
        _vrnSendHeader.DestClientID = destClientID;
        _vrnSendHeader.FromClientID = ClientID;

        //if (isServer)
        {
            _vrnSendHeader.ServerTime = Time.time;
            _vrnSendHeader.TickCount = Time.frameCount;
        }

        stream.WriteMessage(_vrnSendHeader);
        stream.Flush();
    }

}
