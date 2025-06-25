using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;


public abstract class VRMineClientBase : VRMineTransport
{
    //protected NetworkDriver NetworkDriver;
    protected NetworkConnection NetworkConnection;

    //protected NetworkPipeline ReliablePipeline;
    //protected NetworkPipeline UnreliablePipeline;
    //protected NetworkPipeline BasePipeline;
    //protected NetworkPipeline CriticalPipeline;

    public bool IsConnected
    {
        get
        {
            if (!NetworkConnection.IsCreated || !NetworkDriver.IsCreated)
                return false;

            return NetworkConnection.GetState(NetworkDriver) == NetworkConnection.State.Connected;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        //NetworkDriver = NetworkDriver.Create();
        NetworkConnection = default(NetworkConnection);

        //ReliablePipeline = NetworkDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        //UnreliablePipeline = NetworkDriver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
        //BasePipeline = NetworkDriver.CreatePipeline();
        //CriticalPipeline = NetworkDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        //BasePipeline = NetworkDriver.CreatePipeline();
    }

    protected override void Start()
    {
        base.Start();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //NetworkDriver.Dispose();
    }

    protected abstract void OnConnected(DataStreamReader reader);
    protected abstract void OnDisconnected(DataStreamReader reader);
    protected abstract void OnDataMessage(DataStreamReader reader);

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
        
        Connect(ep);
    }

    public void Connect(NetworkEndPoint endpoint)
    {
        //endpoint.Port = port;
        NetworkConnection = NetworkDriver.Connect(endpoint);
    }

    public void Disconnect()
    {
        if (!IsConnected)
            return;

        NetworkConnection.Disconnect(NetworkDriver);
        NetworkConnection = default(NetworkConnection);
    }

    protected virtual void Update()
    {
        NetworkDriver.ScheduleUpdate().Complete();

        if (!NetworkConnection.IsCreated)
        {
            //if (!m_Done)
            //    Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = NetworkConnection.PopEvent(NetworkDriver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log($"Client {gameObject.name} connected to server");

                OnConnected(stream);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnDataMessage(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log($"Client {gameObject.name} disconnected from server");
                NetworkConnection = default(NetworkConnection);
            }
        }
    }
}
