using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;

public abstract class VRMineRelayBase : VRMineTransport
{
    //public const int RelayPort = 2081;

    protected NativeList<NetworkConnection> _connections;

    protected override void Start()
    {
        base.Start();

        _connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        //NetworkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4; // The local address to which the client will connect to is 127.0.0.1
        endpoint.Port = VRMineRelay.RelayPort;
        if (NetworkDriver.Bind(endpoint) != 0)
            Debug.Log($"Failed to bind to port {VRMineRelay.RelayPort}");
        else
            NetworkDriver.Listen();


    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _connections.Dispose();
    }

    protected override void Update()
    {
        base.Update();

        NetworkDriver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < _connections.Length; i++)
        {
            if (!_connections[i].IsCreated)
            {
                _connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // AcceptNewConnections
        NetworkConnection c;
        while ((c = NetworkDriver.Accept()) != default(NetworkConnection))
        {
            _connections.Add(c);
            var remoteAddr = NetworkDriver.RemoteEndPoint(c);
            Debug.Log($"Accepted a connection from {remoteAddr.Address}");

            OnClientConnected(c);
        }

        DataStreamReader stream;
        for (int i = 0; i < _connections.Length; i++)
        {
            c = _connections[i];
            NetworkEvent.Type cmd;
            NetworkPipeline pipeline = default(NetworkPipeline);

            while ((cmd = NetworkDriver.PopEventForConnection(c, out stream, out pipeline)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    OnClientDataMessage(c, stream, pipeline);
                    //uint number = stream.ReadUInt();

                    //Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                    //number += 2;

                    //NetworkDriver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                    //writer.WriteUInt(number);
                    //NetworkDriver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    var remoteAddr = NetworkDriver.RemoteEndPoint(c);

                    Debug.Log($"Client {remoteAddr.Address} disconnected from server");

                    OnClientDisconnected(_connections[i]);

                    _connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}
