using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;


public class VRMineTestClient1 : VRMineClientBase
{
    private NativeArray<byte> _buffer = new NativeArray<byte>(4096, Allocator.Persistent);

    protected override void OnConnected(DataStreamReader reader)
    {
        //uint value = 1;
        //NetworkDriver.BeginSend(NetworkConnection, out var writer);
        //writer.WriteUInt(value);
        //NetworkDriver.EndSend(writer);
    }

    protected override void OnDataMessage(DataStreamReader reader)
    {
        //uint value = reader.ReadUInt();
        //Debug.Log("Got the value = " + value + " back from the server");

        //reader.ReadBytes(_buffer);
        //Debug.Log($"Received Bytes: {_buffer.Length}");
        var str = reader.ReadFixedString512();

        Debug.Log($"{gameObject.name}: {str.ToString()}");
    }

    protected override void OnDisconnected(DataStreamReader reader)
    {
        
    }

    public void SendTestStringReliable(string message)
    {
        var fixedString = new FixedString512Bytes(message);
        NetworkDriver.BeginSend(_reliablePipeline, NetworkConnection, out var writer);        
        writer.WriteFixedString512(fixedString);
        NetworkDriver.EndSend(writer);
    }

    public void SendTestStringUnreliable(string message)
    {
        var fixedString = new FixedString512Bytes(message);
        NetworkDriver.BeginSend(_unreliablePipeline, NetworkConnection, out var writer);
        writer.WriteFixedString512(fixedString);
        NetworkDriver.EndSend(writer);
    }

    public void SendTestStringBase(string message)
    {
        var fixedString = new FixedString512Bytes(message);
        NetworkDriver.BeginSend(_basePipeline, NetworkConnection, out var writer);
        writer.WriteFixedString512(fixedString);
        NetworkDriver.EndSend(writer);
    }

    public void SendTestStringCritical(string message)
    {
        var fixedString = new FixedString512Bytes(message);
        NetworkDriver.BeginSend(_criticalPipeline, NetworkConnection, out var writer);
        writer.WriteFixedString512(fixedString);
        NetworkDriver.EndSend(writer);
    }

    //public NetworkDriver NetworkDriver;
    //public NetworkConnection NetworkConnection;

    //public bool IsConnected {
    //    get
    //    {
    //        if (!NetworkConnection.IsCreated || !NetworkDriver.IsCreated)
    //            return false;

    //        return NetworkConnection.GetState(NetworkDriver) == NetworkConnection.State.Connected;
    //    }
    //}


    protected override void Start()
    {
        base.Start();

        //NetworkDriver = NetworkDriver.Create();
        //NetworkConnection = default(NetworkConnection);

        //var endpoint = NetworkEndPoint.LoopbackIpv4;
        //endpoint.Port = VRMineRelay.RelayPort;
        //NetworkConnection = NetworkDriver.Connect(endpoint);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _buffer.Dispose();
    }

    protected override void OnClientConnected(NetworkConnection client)
    {
        
    }

    protected override void OnClientDisconnected(NetworkConnection client)
    {
        
    }

    protected override void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline)
    {
        
    }

    //public override void OnDestroy()
    //{
    //    NetworkDriver.Dispose();
    //}

    //public void Disconnect()
    //{
    //    if (!IsConnected)
    //        return;

    //    NetworkConnection.Disconnect(NetworkDriver);
    //    NetworkConnection = default(NetworkConnection);
    //}

    //void Update()
    //{
    //    NetworkDriver.ScheduleUpdate().Complete();

    //    if (!NetworkConnection.IsCreated)
    //    {
    //        //if (!m_Done)
    //        //    Debug.Log("Something went wrong during connect");
    //        return;
    //    }

    //    DataStreamReader stream;
    //    NetworkEvent.Type cmd;

    //    while ((cmd = NetworkConnection.PopEvent(NetworkDriver, out stream)) != NetworkEvent.Type.Empty)
    //    {
    //        if (cmd == NetworkEvent.Type.Connect)
    //        {
    //            Debug.Log("We are now connected to the server");

    //            uint value = 1;
    //            NetworkDriver.BeginSend(NetworkConnection, out var writer);
    //            writer.WriteUInt(value);
    //            NetworkDriver.EndSend(writer);
    //        }
    //        else if (cmd == NetworkEvent.Type.Data)
    //        {
    //            uint value = stream.ReadUInt();
    //            Debug.Log("Got the value = " + value + " back from the server");
    //            //m_Done = true;
    //            //NetworkConnection.Disconnect(NetworkDriver);
    //            //NetworkConnection = default(NetworkConnection);
    //        }
    //        else if (cmd == NetworkEvent.Type.Disconnect)
    //        {
    //            Debug.Log("Client got disconnected from server");
    //            NetworkConnection = default(NetworkConnection);
    //        }
    //    }
    //}
}
