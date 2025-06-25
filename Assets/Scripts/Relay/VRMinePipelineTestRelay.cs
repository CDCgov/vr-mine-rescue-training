using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class VRMinePipelineTestRelay : VRMineRelayBase
{
    protected override void OnClientConnected(NetworkConnection client)
    {
        
    }

    protected override void OnClientDataMessage(NetworkConnection client, DataStreamReader reader, NetworkPipeline pipeline)
    {
        var pipelineName = _pipelineNames[pipeline];
        var str = reader.ReadFixedString512();
        Debug.Log($"Received ({pipelineName}): {str.ToString()}");


        //NetworkDriver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
        //writer.WriteUInt(number);
        //NetworkDriver.EndSend(writer);
    }

    protected override void OnClientDisconnected(NetworkConnection client)
    {
        
    }

}
