using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MockSystemManager", menuName = "VRMine/Managers/MockSystemManager", order = 0)]
public class MockSystemManager : SystemManager
{
    public string MultiplayerName;
    public int MultiplayerPort = 9090;
    public string MultiplayerServer = "127.0.0.1";

    //public override void Awake()
    //{
        
    //}

    public override void OnEnable()
    {
        SystemConfig = new SystemConfig();

        SystemConfig.MultiplayerName = MultiplayerName;
        SystemConfig.MultiplayerPort = MultiplayerPort;
        SystemConfig.MultiplayerServer = MultiplayerServer;

    }
}
