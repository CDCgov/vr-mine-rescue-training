using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;
using Google.Protobuf;
using Unity.Collections.LowLevel.Unsafe;

public class VRMineClientSim : VRMineTransport
{
    public bool AutoConnect = false;
    public bool SendTestData = false;
    public int NumPacketsReceived = 0;
    public int NumTextMessagesReceived = 0;
    public int NumClientJoinedMessages = 0;
    public int NumClientLeftMessages = 0;
    public int NumPingMessages = 0;
    public int NumUnknownPackets = 0;
    
    public int PlayerID = -1;
    public VRNTextMessage LastTextMessage;
    public VRNRelayClientStatsList ClientStats;


    private byte[] _receiveBuffer;
    private VRNHeader _vrnHeader;

    private VRNPingResponse _pingResponse;
    

    protected override void Awake()
    {
        base.Awake();

        _vrnHeader = new VRNHeader();
        _pingResponse = new VRNPingResponse();
        _receiveBuffer = new byte[10000];
        LastTextMessage = new VRNTextMessage();
    }

    protected override void Start()
    {
        base.Start();

        if (AutoConnect)
            Connect("localhost", (ushort)VRMineRelay.RelayPort);

        if (SendTestData)
            StartCoroutine(SendTestMessages());
        //StartCoroutine(StartupSequence());
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        var queueSize = NetworkDriver.GetEventQueueSizeForConnection(_clientConnection);
        if (queueSize > 0)
        {
            Debug.Log($"{gameObject.name} Queue Size : {queueSize}");
        }
    }

    private IEnumerator StartupSequence()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        
        Connect("localhost", VRMineRelay.RelayPort);
    }

    private IEnumerator SendTestMessages()
    {
        VRNTextMessage msg = new VRNTextMessage();
        while (true)
        {
            msg.Message = $"Test Message : {Time.time:F2}";
            SendNetMessage(VRNPacketType.TextMessage, msg, reliable: false, broadcast: true);
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    protected override void OnClientConnected(NetworkConnection client)
    {
        int queueSize = NetworkDriver.GetEventQueueSizeForConnection(client);
        Debug.Log($"ClientSim: {gameObject.name} Connected, Queue size: {queueSize}");
    }

    unsafe protected override void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline)
    {
        NativeArray<byte> buffer = new NativeArray<byte>(reader.Length, Allocator.Temp);

        reader.ReadBytes(buffer);

        NumPacketsReceived++;


        //buffer.CopyTo(_receiveBuffer);
        //buffer.ToArray();

        fixed (byte* unsafeBuffer = _receiveBuffer)
        {
            UnsafeUtility.MemCpy(unsafeBuffer, buffer.GetUnsafePtr<byte>(), reader.Length);
        }

        var codedRecvStream = new CodedInputStream(_receiveBuffer, 0, buffer.Length);

        _vrnHeader.PacketType = VRNPacketType.Unknown;
        codedRecvStream.ReadMessage(_vrnHeader);

        switch (_vrnHeader.PacketType)
        {
            case VRNPacketType.AssignClientInfo:
                VRNAssignClientInfo clientInfo = new VRNAssignClientInfo();
                codedRecvStream.ReadMessage(clientInfo);
                ClientID = clientInfo.ClientID;
                Debug.Log($"Client {gameObject.name} assigned client ID {ClientID}");
                break;

            case VRNPacketType.TextMessage:
                codedRecvStream.ReadMessage(LastTextMessage);
                Debug.Log($"Client {gameObject.name} received message \"{LastTextMessage.Message}\"");
                NumTextMessagesReceived++;
                break;

            case VRNPacketType.AssignPlayerId:
                VRNAssignPlayerID assignPlayerID = new VRNAssignPlayerID();
                codedRecvStream.ReadMessage(assignPlayerID);
                PlayerID = assignPlayerID.PlayerID;
                Debug.Log($"Client {gameObject.name} assigned player ID {PlayerID}");
                break;

            case VRNPacketType.RelayClientJoined:
                NumClientJoinedMessages++;

                var clientList = new VRNRelayClientList();
                codedRecvStream.ReadMessage(clientList);
                //foreach (var relayClient in clientList.ClientList)
                //{
                //    Debug.Log($"ClientList: {relayClient.ClientID} : {relayClient.IpAddress}");
                //}
                break;

            case VRNPacketType.RelayClientLeft:
                NumClientLeftMessages++;
                Debug.Log($"Client {gameObject.name} received RelayClientLeft");
                break;

            case VRNPacketType.Ping:
                NumPingMessages++;
                SendNetMessage(VRNPacketType.PingResponse, _pingResponse, reliable: false, broadcast: false);
                break;

            case VRNPacketType.RelayClientStats:
                ClientStats = new VRNRelayClientStatsList();
                codedRecvStream.ReadMessage(ClientStats);
                break;

            case VRNPacketType.Unknown:
                NumUnknownPackets++;
                break;

            default:
                //Debug.Log($"ClientSim: Received {_vrnHeader.PacketType.ToString()} bytes: {buffer.Length}");
                break;
        }

        

    }

    protected override void OnClientDisconnected(NetworkConnection client)
    {
        Debug.Log($"ClientSim: {gameObject.name} Disconnected");
    }
}
