using Google.Protobuf;
//using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpectatorRepresentation
{
    public int ClientID;
    public string Name;
    public string IPAddress;
}

public class SpectatorManager : SceneManagerBase
{
    public static SpectatorManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SpectatorManager>("SpectatorManager");
    }

    public SystemManager SystemManager;
    public NetworkManager NetworkManager;

    public List<SpectatorRepresentation> Spectators;

    public event System.Action SpectatorListChanged;
    public event System.Action<int> SpectatorJoined;
    public event System.Action<int> SpectatorLeft;

    public void SendSpectatorJoinRequest()
    {
        if (!NetworkManager.ClientConnected)
            return;

        VRNSpectatorInfo spectator = new VRNSpectatorInfo
        {
            Name = SystemManager.SystemConfig.MultiplayerName,
            IpAddress = "",
            ClientID = NetworkManager.ClientID,
        };

        NetworkManager.SendNetMessage(VRNPacketType.RequestJoinSpectator, spectator);
    }

    // Start is called before the first frame update
    void Start()
    {
        Spectators = new List<SpectatorRepresentation>();

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();        

        NetworkManager.RegisterHandler(VRNPacketType.SendSpectatorInfo, SpectatorInfoHandler);
        NetworkManager.RegisterHandler(VRNPacketType.RequestJoinSpectator, SpectatorJoinHandler);

        NetworkManager.ClientDisconnected += OnClientDisconnected;
        NetworkManager.ClientJoinedServer += OnClientJoinedServer;
        NetworkManager.DisconnectedFromServer += OnDisconnectedFromServer;
        NetworkManager.ServerDisconnectedFromRelay += OnServerDisconnectedFromRelay;

        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (NetworkManager != null)
        {
            NetworkManager.UnregisterHandler(VRNPacketType.SendSpectatorInfo, SpectatorInfoHandler);
            NetworkManager.UnregisterHandler(VRNPacketType.RequestJoinSpectator, SpectatorJoinHandler);

            NetworkManager.ClientDisconnected -= OnClientDisconnected;
            NetworkManager.ClientJoinedServer -= OnClientJoinedServer;
            NetworkManager.DisconnectedFromServer -= OnDisconnectedFromServer;
            NetworkManager.ServerDisconnectedFromRelay -= OnServerDisconnectedFromRelay;
        }
    }

    private void OnDisconnectedFromServer()
    {
        ClearSpectatorList();
    }

    private void OnServerDisconnectedFromRelay()
    {
        ClearSpectatorList();
    }

    private void OnClientJoinedServer(int obj)
    {
        if (!NetworkManager.IsServer)
            return;


        VRNSpectatorInfo spectator = new VRNSpectatorInfo();
        spectator.IpAddress = "";
        spectator.Name = "";
        spectator.ClientID = -1;

        for (int i = 0; i < Spectators.Count; i++)
        {

            spectator.ClientID = Spectators[i].ClientID;
            spectator.Name = Spectators[i].Name;

            NetworkManager.SendNetMessage(VRNPacketType.SendSpectatorInfo, spectator);
        }
    }

    private void OnClientDisconnected(int clientID)
    {
        for (int i = 0; i < Spectators.Count; i++)
        {
            if (Spectators[i].ClientID == clientID)
            {
                SpectatorLeft?.Invoke(clientID);
                Spectators.RemoveAt(i);
                SpectatorListChanged?.Invoke();
                break;
            }
        }
    }

    void ClearSpectatorList()
    {
        Spectators.Clear();
    }

    void AddOrUpdateSpectator(int clientID, VRNSpectatorInfo spectator)
    {
        if (clientID < 0)
        {
            Debug.LogError($"Tried to update spectator {spectator.Name} with invalid client ID");
            return;
        }

        SpectatorRepresentation spr = new SpectatorRepresentation
        {
            ClientID = clientID,
            Name = spectator.Name,
            IPAddress = spectator.IpAddress,
        };

        for (int i = 0; i < Spectators.Count; i++)
        {
            if (Spectators[i].ClientID == clientID)
            {
                //update entry
                Spectators[i] = spr;
                SpectatorListChanged?.Invoke();
                return;
            }
        }

        //add new entry
        Spectators.Add(spr);
        SpectatorListChanged?.Invoke();
        SpectatorJoined?.Invoke(clientID);
    }

    void SpectatorInfoHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var spectatorInfo = VRNSpectatorInfo.Parser.ParseDelimitedFrom(recvStream);
        var spectatorInfo = new VRNSpectatorInfo();
        reader.ReadMessage(spectatorInfo);

        //var clientID = NetworkManager.GetClientID(fromPeer);
        clientID = spectatorInfo.ClientID;

        Debug.Log($"Spectator info {spectatorInfo.Name} {spectatorInfo.ClientID}::{clientID}");

        AddOrUpdateSpectator(clientID, spectatorInfo);
    }

    void SpectatorJoinHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var spectatorInfo = VRNSpectatorInfo.Parser.ParseDelimitedFrom(recvStream);
        var spectatorInfo = new VRNSpectatorInfo();
        reader.ReadMessage(spectatorInfo);

        //var clientID = NetworkManager.GetClientID(fromPeer);
        clientID = spectatorInfo.ClientID;

        Debug.Log($"Spectator joined {spectatorInfo.Name} {spectatorInfo.ClientID}::{clientID}");

        AddOrUpdateSpectator(clientID, spectatorInfo);

        if (NetworkManager.IsServer)
        {
            NetworkManager.SendNetMessage(VRNPacketType.SendSpectatorInfo, spectatorInfo);
        }
    }

}
