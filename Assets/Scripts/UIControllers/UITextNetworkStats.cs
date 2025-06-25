using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UITextNetworkStats : MonoBehaviour, IStatusText
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;

    public void AppendStatusText(StringBuilder statusText)
    {
        if (NetworkManager == null || NetworkedObjectManager == null)
            return;

        if (!NetworkManager.IsStatLogEnabled)
            NetworkManager.IsStatLogEnabled = true;

        Debug.Log("UITextNetworkStats Updating Text");

        statusText.AppendFormat("Average Send kbps    : {0}\n", NetworkManager.AverageBytesPerSecond * 8.0f / 1000.0f);
        statusText.AppendFormat("Sync Count Last Frame: {0}\n", NetworkManager.FrameSyncCount);
        statusText.AppendFormat("Sync Count Max       : {0}\n", NetworkManager.FrameSyncCountMax);
        statusText.AppendFormat("Number of NetObj     : {0}\n", NetworkedObjectManager.NetworkedObjectCount);

        statusText.AppendLine("Packet Send Stats:");
        foreach (var sendStat in NetworkManager.PacketSendStats.Values)
        {
            AppendStatLine(statusText, sendStat);
        }

        statusText.AppendLine("Packet Receive Stats:");
        foreach (var recvStat in NetworkManager.PacketRecvStats.Values)
        {
            AppendStatLine(statusText, recvStat);
        }
    }

    private void AppendStatLine(StringBuilder sb, NetworkManager.PacketStats stats)
    {
        var packetSize = stats.TotalBytes / stats.PacketCount;
        sb.AppendFormat("{0,35}: {1,8} {2} B ({3} max)\n", stats.PacketType, stats.PacketCount, packetSize, stats.MaxPacketSize);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
    }
}
