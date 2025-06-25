using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

/*
public struct VRMineRelayData
{

}

public abstract class VRMineTransport : MonoBehaviour
{
    
    public NetworkDriver NetworkDriver;

    protected NetworkConnection _clientConnection;

    protected NetworkPipeline _reliablePipeline;
    protected NetworkPipeline _unreliablePipeline;
    protected NetworkPipeline _basePipeline;
    protected NetworkPipeline _criticalPipeline;

    protected Dictionary<NetworkPipeline, string> _pipelineNames;

    public bool IsConnected
    {
        get
        {
            if (!_clientConnection.IsCreated || !NetworkDriver.IsCreated)
                return false;

            return _clientConnection.GetState(NetworkDriver) == NetworkConnection.State.Connected;
        }
    }

    protected virtual void Awake()
    {
        NetworkDriver = NetworkDriver.Create();

        _reliablePipeline = NetworkDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        _unreliablePipeline = NetworkDriver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
        _basePipeline = NetworkDriver.CreatePipeline();
        _criticalPipeline = NetworkDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

        _pipelineNames = new Dictionary<NetworkPipeline, string>();
        _pipelineNames.Add(_reliablePipeline, "Reliable");
        _pipelineNames.Add(_unreliablePipeline, "Unreliable");
        _pipelineNames.Add(_basePipeline, "Base");
        _pipelineNames.Add(_criticalPipeline, "Critical");
    }

    protected virtual void Start()
    {

        
    }

    protected virtual void OnDestroy()
    {
        NetworkDriver.Dispose();
        
    }

    
    protected abstract void OnClientConnected(NetworkConnection client);
    protected abstract void OnClientDisconnected(NetworkConnection client);
    protected abstract void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline);

    protected virtual void Update()
    {
       
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
}
*/