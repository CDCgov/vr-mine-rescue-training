using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using Google.Protobuf;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport.Error;
using System.Text.RegularExpressions;
using Unity.Networking.Transport.Utilities;

public class RelayClientInfo
{
    public NetworkConnection Connection;
    public int ClientID;
    public float ConnectTime;
    public ulong ReceivedBytes;
    public float LastReceiveTime;
    public string IPAddress;
    public float LastPing;
    public MovingAverage CurrentPingRTT;
    public float ReliablePipelineRTT;
    public float ReliablePipelineRTTVar;
    public TransportSendQueue ReliableSendQueue;
}

public class VRMineRelay : VRMineTransport
{
    //private NativeArray<byte> _buffer = new NativeArray<byte>(4096, Allocator.Persistent);

    public static ushort RelayPort = 9090;
    public int NumSimPlayers = 0;

    private ulong _bytesSent = 0;
    public uint SendErrors = 0;

    private uint _movingBytesSent;
    private float _movingBytesSentStartTime;
    private MovingAverage _outgoingBitrate;

    public int ConnectedClients
    {
        get
        {
            if (_connections == null)
                return 0;

            return _connections.Count;
        }
    }

    private float _lastStatsMessage = 0;

    private Dictionary<NetworkConnection, RelayClientInfo> _clientInfo;
    private short _lastClientIDAssigned = 0;

    private byte[] _receiveBuffer;
    private VRNHeader _vrnHeader;
    private StringBuilder _statusText;
    private bool _networkReset = false;

    private MovingAverage _incomingPacketSize;
    private int _incomingPacketMax;

    protected override void Awake()
    {
        base.Awake();

        _incomingPacketMax = 0;
        _incomingPacketSize = new MovingAverage(100);

        _movingBytesSent = 0;
        _movingBytesSentStartTime = Time.realtimeSinceStartup;
        _outgoingBitrate = new MovingAverage(15);

        var match = Regex.Match(System.Environment.CommandLine, @"--port=(\d+)");
        if (match.Success)
        {
            ushort port = System.UInt16.Parse(match.Groups[1].Value);
            Debug.Log($"Found port on command line - {port}");

            RelayPort = port;
        }

        match = Regex.Match(System.Environment.CommandLine, @"--simplayers=(\d+)");
        if (match.Success)
        {
            NumSimPlayers = System.Int32.Parse(match.Groups[1].Value);
            Debug.Log($"Starting {NumSimPlayers} simulated players");
        }

        _statusText = new StringBuilder();
        _vrnHeader = new VRNHeader();
        _receiveBuffer = new byte[MaxPayloadSize];
        _clientInfo = new Dictionary<NetworkConnection, RelayClientInfo>();
    }

    private GameObject StartSimClient(string name)
    {
        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SimClient"));
        go.name = name;

        return go;

    }

    protected override void Start()
    {
        base.Start();


        Listen((ushort)RelayPort);
        InvokeRepeating(nameof(DisconnectIdleClients), 5, 5);
        InvokeRepeating(nameof(PingClients), 1, 0.25f);
        InvokeRepeating(nameof(SendClientStats), 3, 3);


        Debug.Log("********************************************************");
        Debug.Log("*********     VR Mine Relay September 2024     *********");
        Debug.Log("********************************************************");

        Debug.Log($"VR Mine Relay listening on port {RelayPort}");

        Debug.Log("********************************************************");

        if (NumSimPlayers > 0)
        {
            for (int i = 0; i < NumSimPlayers; i++)
            {
                StartSimClient($"SimPlayer{i}");
            }
        }
    }

    protected override void OnDestroy()
    {
        DisconnectAllClients();

        base.OnDestroy();

        //_buffer.Dispose();
    }

    protected override void OnClientConnected(NetworkConnection client)
    {
        _lastClientIDAssigned++;

        var remoteAddr = NetworkDriver.RemoteEndPoint(client);

        RelayClientInfo info = new RelayClientInfo
        {
            ClientID = _lastClientIDAssigned,
            Connection = client,
            ConnectTime = Time.realtimeSinceStartup,
            ReceivedBytes = 0,
            IPAddress = remoteAddr.Address,
            LastReceiveTime = Time.realtimeSinceStartup,
            LastPing = 0,
            CurrentPingRTT = new MovingAverage(25),
            ReliableSendQueue = GetSendQueue(client),
        };

        _clientInfo[client] = info;

        var msg = new VRNAssignClientInfo
        {
            ClientID = _lastClientIDAssigned,
        };
        SendNetMessage(VRNPacketType.AssignClientInfo, msg, clientID: _lastClientIDAssigned, reliable: true,
            broadcast: false, conn: client);

        Debug.Log($"Client {remoteAddr.Address} assigned ID {_lastClientIDAssigned}");

        SendClientList(VRNPacketType.RelayClientJoined);
    }

    protected override void OnClientDisconnected(NetworkConnection client)
    {
        if (_clientInfo.ContainsKey(client))
            _clientInfo.Remove(client);

        SendClientList(VRNPacketType.RelayClientLeft);
    }

    unsafe protected override void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline)
    {
        NativeArray<byte> buffer = default(NativeArray<byte>);
        try
        {
            int packetSize = reader.Length;
            if (packetSize > _incomingPacketMax)
                _incomingPacketMax = packetSize;

            _incomingPacketSize.AddSample((float)packetSize);

            //reader.ReadBytes((byte*)_buffer, reader.Length);
            buffer = new NativeArray<byte>(reader.Length, Allocator.Temp);
            reader.ReadBytes(buffer);

            fixed (byte* unsafeBuffer = _receiveBuffer)
            {
                UnsafeUtility.MemCpy(unsafeBuffer, buffer.GetUnsafePtr<byte>(), reader.Length);
            }

            //Debug.Log($"ReaderLength: {reader.Length} BufferLength: {buffer.Length}");
            var codedRecvStream = new CodedInputStream(_receiveBuffer, 0, buffer.Length);

            _vrnHeader.PacketType = VRNPacketType.Unknown;
            _vrnHeader.PacketDest = VRNPacketDestination.Direct;
            _vrnHeader.PacketSize = 0;
            codedRecvStream.ReadMessage(_vrnHeader);

            RelayClientInfo relayClientInfo;
            if (!_clientInfo.TryGetValue(client, out relayClientInfo))
                relayClientInfo = null;
            else
            {
                relayClientInfo.ReceivedBytes += (ulong)reader.Length;
                relayClientInfo.LastReceiveTime = Time.realtimeSinceStartup;
            }

            //if (_vrnHeader.PacketType == VRNPacketType.PingResponse)
            //{
            //    Debug.Log($"PingResponse received from {relayClientInfo.ClientID} ({_vrnHeader.FromClientID}) - Net ID {client.InternalId}");
            //}


            //check if this message is handled locally
            if (ProcessMessage(client, codedRecvStream, _vrnHeader, relayClientInfo))
                return;

            if (_vrnHeader.PacketDest == VRNPacketDestination.Broadcast || _vrnHeader.DestClientID < 0)
            {
                SendToOthers(client, pipeline, buffer);
            }
            else
            {
                SendToClient(_vrnHeader.DestClientID, pipeline, buffer);
            }

            if (_networkReset)
            {
                Debug.Log($"Received network reset on pipeline {_pipelineNames[pipeline]}");
                //if the network was reset, re-send client assignments
                _networkReset = false;

                foreach (var clientInfo in _clientInfo.Values)
                {
                    var msg = new VRNAssignClientInfo
                    {
                        ClientID = clientInfo.ClientID,
                    };

                    SendNetMessage(VRNPacketType.AssignClientInfo, msg, clientID: clientInfo.ClientID, reliable: true,
                        broadcast: false, conn: clientInfo.Connection);
                }
            }


        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing data message: {ex.Message} {ex.StackTrace}");
        }
        finally
        {
            buffer.Dispose();
        }
    }

    bool ProcessMessage(NetworkConnection client, CodedInputStream stream, VRNHeader header, RelayClientInfo relayClientInfo)
    {
        switch (header.PacketType)
        {
            case VRNPacketType.RequestJoinServer:
                var msg = new VRNAssignClientInfo
                {
                    ClientID = relayClientInfo.ClientID,
                };
                SendNetMessage(VRNPacketType.AssignClientInfo, msg, clientID: relayClientInfo.ClientID, reliable: true,
                    broadcast: false, conn: client);

                Debug.Log($"Client {relayClientInfo.IPAddress} re-assigned ID {relayClientInfo.ClientID}");

                SendClientList(VRNPacketType.RelayClientJoined);
                return true;

            case VRNPacketType.AssignClientInfo:
                VRNAssignClientInfo clientInfo = new VRNAssignClientInfo();
                stream.ReadMessage(clientInfo);

                if (relayClientInfo != null && relayClientInfo.ClientID != clientInfo.ClientID)
                {
                    //disconnecting any existing clients with this ID (usually a not yet timed out DM connection)                    
                    DisconnectClient(clientInfo.ClientID);
                }

                if (relayClientInfo != null)
                {
                    relayClientInfo.ClientID = clientInfo.ClientID;
                }


                SendNetMessage(VRNPacketType.AssignClientInfo, clientInfo, reliable: true, broadcast: false, conn: client);

                SendClientList(VRNPacketType.RelayClientListChanged);

                return true;

            case VRNPacketType.PingResponse:
                var pingRTT = (Time.realtimeSinceStartup - relayClientInfo.LastPing) * 1000.0f;
                relayClientInfo.CurrentPingRTT.AddSample(pingRTT);
                return true;

            case VRNPacketType.NetworkReset:
                _networkReset = true;
                return false;
        }

        return false;
    }

    void SendToClient(int clientID, NetworkPipeline pipeline, NativeArray<byte> buffer)
    {
        //TODO: remove nested iteration
        foreach (var clientInfo in _clientInfo.Values)
        {
            if (clientInfo.ClientID == clientID)
            {
                for (int i = 0; i < _connections.Count; i++)
                {
                    var connData = _connections[i];
                    if (connData.conn == clientInfo.Connection)
                    {
                        if (!SendData(pipeline, connData, buffer))
                        {
                            connData.sendErrors++;
                            _connections[i] = connData;
                        }
                        return;
                    }
                }
            }
        }

        Debug.LogError($"VRMineRelay: Couldn't find client ID {clientID} to send to");
    }

    void SendToOthers(NetworkConnection source, NetworkPipeline pipeline, NativeArray<byte> buffer)
    {
        //DataStreamWriter writer;

        for (int i = 0; i < _connections.Count; i++)
        {
            var connData = _connections[i];
            //if (i == index)
            //    continue;
            if (connData.conn == source)
                continue;

            if (!SendData(pipeline, connData, buffer))
            {
                connData.sendErrors++;
                _connections[i] = connData;
            }
            //NetworkDriver.BeginSend(pipeline, _connections[i], out writer);
            //writer.WriteBytes(buffer);
            //NetworkDriver.EndSend(writer);

            //_bytesSent += (ulong)buffer.Length;
        }
    }

    bool SendData(NetworkPipeline pipeline, ConnectionInfo connData, NativeArray<byte> buffer)
    {
        DataStreamWriter writer;
        int result;

        if (pipeline == _reliablePipeline)
        {
            //queue the packet for sending instead
            connData.reliablePacketQueue.Enqeuue(pipeline, buffer, connData.conn);
            return true;
        }

        result = NetworkDriver.BeginSend(pipeline, connData.conn, out writer);
        if (result < 0)
        {
            var endPoint = NetworkDriver.RemoteEndPoint(connData.conn);

            var statusCode = (StatusCode)result;
            Debug.LogError($"VRMineTransport:  Error sending packet to {endPoint.Address}: {statusCode.ToString()}");
            SendErrors++;
            return false;
        }

        writer.WriteBytes(buffer);

        result = NetworkDriver.EndSend(writer);
        if (result < 0)
        {
            var endPoint = NetworkDriver.RemoteEndPoint(connData.conn);

            var statusCode = (StatusCode)result;
            Debug.LogError($"VRMineTransport:  Error sending packet to {endPoint.Address}: {statusCode.ToString()}");
            SendErrors++;
            return false;
        }

        _bytesSent += (ulong)buffer.Length;
        _movingBytesSent += (uint)buffer.Length;
        return true;

    }

    /// <summary>
    /// Disconnect a client
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns>true if a client was disconnected</returns>
    private bool DisconnectClient(int clientID)
    {
        foreach (var client in _clientInfo.Values)
        {
            if (client.ClientID != clientID)
                continue;

            try
            {
                string state;
                if (client.Connection.IsCreated)
                    state = client.Connection.GetState(NetworkDriver).ToString();
                else
                    state = "Not Created";

                Debug.Log($"Disconnecting client {client.ClientID} ({client.Connection.InternalId}) - ({state})");
                Disconnect(client.Connection);
                _clientInfo.Remove(client.Connection);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error disconnecting client {client.ClientID}: {ex.Message}");
            }
        }

        return false;
    }

    private void DisconnectAllClients()
    {
        foreach (var client in _clientInfo.Values)
        {
            try
            {
                Disconnect(client.Connection);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error disconnecting client {client.ClientID}: {ex.Message}");
            }
        }

        _clientInfo.Clear();
    }

    private void DisconnectIdleClients()
    {
        foreach (var client in _clientInfo.Values)
        {
            try
            {
                var elapsed = Time.realtimeSinceStartup - client.LastReceiveTime;
                if (elapsed > 10.0f)
                {
                    //NetworkDriver.Disconnect(client.Connection);
                    Disconnect(client.Connection);
                    _clientInfo.Remove(client.Connection);

                    Debug.Log($"Disconnecting idle client {client.ClientID}");

                    SendClientList(VRNPacketType.RelayClientLeft);
                    break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in DisconnectIdleClients processing client {client.ClientID}: {ex.Message}");
            }
        }
    }

    private void PingClients()
    {
        //var msg = new VRNTextMessage();
        //msg.Message = "A";

        foreach (var client in _clientInfo.Values)
        {
            try
            {
                var elapsed = Time.realtimeSinceStartup - client.LastPing;
                if (elapsed > 1.0f)
                {
                    //Debug.Log($"Sending ping to client {client.ClientID} ({client.Connection.InternalId})");
                    client.LastPing = Time.realtimeSinceStartup;
                    SendNetMessage(VRNPacketType.Ping, msg: null, reliable: false, broadcast: false,
                        clientID: client.ClientID, conn: client.Connection);
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error sending ping to client {client.ClientID}: {ex.Message}");
            }
        }
    }

    private void SendClientList(VRNPacketType packetType)
    {
        VRNRelayClientList clientList = new VRNRelayClientList();

        foreach (var clientInfo in _clientInfo.Values)
        {
            clientList.ClientList.Add(new VRNRelayClientInfo
            {
                ClientID = clientInfo.ClientID,
                IpAddress = clientInfo.IPAddress,
            });
        }

        foreach (var clientInfo in _clientInfo.Values)
        {
            SendNetMessage(packetType, clientList, reliable: true, broadcast: true,
                conn: clientInfo.Connection);
        }
    }

    private void SendClientStats()
    {
        VRNRelayClientStatsList statsList = new VRNRelayClientStatsList();

        foreach (var clientInfo in _clientInfo.Values)
        {
            if (clientInfo == null)
                continue;

            int sendQueueCount = -1;
            if (clientInfo.ReliableSendQueue != null)
                sendQueueCount = clientInfo.ReliableSendQueue.Count;

            statsList.ClientStats.Add(new VRNClientStats
            {
                ClientID = clientInfo.ClientID,
                ConnectTime = Time.realtimeSinceStartup - clientInfo.ConnectTime,
                AvgBitRate = GetClientBitRate(clientInfo),
                AvgPingRTT = clientInfo.CurrentPingRTT.Average,
                ReliablePipelineRTT = clientInfo.ReliablePipelineRTT,
                ReliablePipelineRTTVar = clientInfo.ReliablePipelineRTTVar,
                ReliableSendQueueCount = sendQueueCount,
            });



        }

        foreach (var clientInfo in _clientInfo.Values)
        {
            SendNetMessage(VRNPacketType.RelayClientStats, statsList, reliable: false, broadcast: true,
                conn: clientInfo.Connection);
        }
    }

    private float GetClientBitRate(RelayClientInfo client)
    {
        float clientTime = Time.realtimeSinceStartup - client.ConnectTime;

        var clientKB = (float)(client.ReceivedBytes / 1024);
        //var clientMB = clientKB / 1024.0f;
        //var clientMbps = (clientMB / clientTime) * 8.0f;
        var clientKbps = (clientKB / clientTime) * 8.0f;

        return clientKbps;
    }

    private void UpdateOutgoingBitrate()
    {
        float elapsed = Time.realtimeSinceStartup - _movingBytesSentStartTime;
        float kbps = (((float)_movingBytesSent / 1024.0f) * 8.0f) / elapsed;

        _movingBytesSent = 0;
        _movingBytesSentStartTime = Time.realtimeSinceStartup;
        _outgoingBitrate.AddSample(kbps);
    }

    private unsafe void UpdateReliabilityStats(RelayClientInfo client)
    {
        NativeArray<byte> receiveBuffer;
        NativeArray<byte> sendBuffer;
        NativeArray<byte> sharedBuffer;

        var reliableStageId = NetworkPipelineStageCollection.GetStageId(typeof(ReliableSequencedPipelineStage));
        NetworkDriver.GetPipelineBuffers(_reliablePipeline, reliableStageId, client.Connection,
            out receiveBuffer, out sendBuffer, out sharedBuffer);

        var context = (ReliableUtility.SharedContext*)sharedBuffer.GetUnsafePtr();

        var stats = context->stats;
        var rttInfo = context->RttInfo;

        client.ReliablePipelineRTT = rttInfo.SmoothedRtt;
        client.ReliablePipelineRTTVar = rttInfo.SmoothedVariance;


    }

    protected override void Update()
    {
        base.Update();


        if (Time.realtimeSinceStartup - _lastStatsMessage > 2.0f)
        {
            UpdateOutgoingBitrate();

            _lastStatsMessage = Time.realtimeSinceStartup;
            _statusText.Clear();

            float kBSent = (float)(_bytesSent / 1024);
            float totalMB = kBSent / 1024.0f;

            float Mbps = (totalMB * 8.0f) / Time.realtimeSinceStartup;

            _statusText.AppendFormat("\nUptime: {0,4:F0}s, {1} connected clients, {2:F2} Mbps  (total: {3:F1} MB)\n",
                Time.realtimeSinceStartup,
                _connections.Count,
                Mbps, totalMB);
            _statusText.AppendFormat("Send errors: {0,4} Out Avg: {1,4:F0}kbps In Pkt Size Avg: {2,4:F0} Max: {3}\n", 
                SendErrors,
                _outgoingBitrate.Average,
                _incomingPacketSize.Average,
                _incomingPacketMax);

            foreach (var client in _clientInfo.Values)
            {
                if (client == null)
                    continue;

                try
                {
                    if (!client.Connection.IsCreated)
                        continue;

                    UpdateReliabilityStats(client);

                    var endPoint = NetworkDriver.RemoteEndPoint(client.Connection);

                    float clientTime = Time.realtimeSinceStartup - client.ConnectTime;

                    var clientKB = (float)(client.ReceivedBytes / 1024);
                    //var clientMB = clientKB / 1024.0f;
                    //var clientMbps = (clientMB / clientTime) * 8.0f;
                    var clientKbps = (clientKB / clientTime) * 8.0f;

                    var timeSpan = System.TimeSpan.FromSeconds(clientTime);


                    float ping = client.CurrentPingRTT.Average;
                    if (!client.CurrentPingRTT.HasSamples)
                        ping = -1;

                    int sendQueueCount = -1;
                    if (client.ReliableSendQueue != null)
                        sendQueueCount = client.ReliableSendQueue.Count;

                    _statusText.AppendFormat("Client {0,22} ({1,3}) {2,4:F0}ms/{3,4:F0}ms (var{4,3:F0}) Q:{5,4} ({6} - {7:F2} kbps)\n",
                        endPoint.Address,
                        client.ClientID,
                        ping,
                        client.ReliablePipelineRTT,
                        client.ReliablePipelineRTTVar,
                        sendQueueCount,
                        timeSpan.ToString("hh':'mm':'ss"),
                        clientKbps);
                }
                catch (System.Exception ex)
                {
                    _statusText.AppendFormat("Error getting client status for {0}: {1}", client.ClientID, ex.Message);
                }
            }

            Debug.Log(_statusText.ToString());

        }
    }

}
