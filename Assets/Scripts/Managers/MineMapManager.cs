using Google.Protobuf;
//using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MineMapData : ISessionTimeSeriesData<MineMapData>
{
    public string SymbolManagerName;
    public Guid SymbolManagerID;
    public MineMapSymbolManager SymbolManager;
    public VRNSymbolManagerState SymbolState;
    

    public void CopyTo(MineMapData dest)
    {
        dest.SymbolManagerName = SymbolManagerName;
        dest.SymbolManagerID = SymbolManagerID;
        dest.SymbolManager = SymbolManager;
        dest.SymbolState = SymbolState;
    }

    public void Interpolate(MineMapData next, float interp, ref MineMapData result)
    {
        CopyTo(result);
    }
}

public class MineMapManager : SceneManagerBase
{

    public static MineMapManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<MineMapManager>("MineMapManager");
    }

    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public Dictionary<Guid, MineMapData> SymbolManagers;
    public Action MapLoaded;

    //private VRNSymbolManagerState _vrnSymbolManagerState;

    public MineMapData GetMineMapData(Guid symbolManagerID)
    {
        MineMapData data;
        if (!SymbolManagers.TryGetValue(symbolManagerID, out data))
        {
            data = new MineMapData();
            data.SymbolManagerName = "";
            SymbolManagers[symbolManagerID] = data;
        }

        return data;
    }

    public MineMapData GetMineMapData(int playerID)
    {
        foreach (var data in SymbolManagers.Values)
        {
            if (data.SymbolState != null && data.SymbolState.PlayerID == playerID)
                return data;
        }

        return null;
    }

    public void RegisterMineMap(Guid symbolManagerID,  MineMapSymbolManager symbolManager)
    {
        Debug.Log($"MineMapManager: Registering mine map ID {symbolManagerID}");

        MineMapData data = GetMineMapData(symbolManagerID);

        data.SymbolManager = symbolManager;
        data.SymbolManagerID = symbolManagerID;

        if (data.SymbolState != null)
        {
            Debug.Log($"MineMapManager: Loading existing mine map symbols: {data.SymbolState.Symbols.Count}");
            symbolManager.LoadFromSerializedState(data.SymbolState);
        }
    }

    public void UnregisterMineMap(Guid symbolManagerID)
    {
        if (SymbolManagers != null)
            SymbolManagers.Remove(symbolManagerID);
    }

    public void ClearAllMaps()
    {
        var maps = FindObjectsByType<MineMapSymbolManager>(FindObjectsSortMode.None);

        foreach (var map in maps)
        {
            if (map == null || map.gameObject == null)
                continue;

            map.RemoveAllSymbolsIncludingScene();
        }

        //foreach (var map in SymbolManagers.Values)
        //{
        //    if (map == null || map.SymbolManager == null || map.SymbolManager.gameObject == null)
        //        continue;

        //    map.SymbolManager.RemoveAllSymbols();
        //}
    }

    public void SendSymbolManagerState(Guid symbolManagerID, string symbolManagerName, int playerID, MineMapSymbolManager manager)
    {
        if (symbolManagerName == null)
            symbolManagerName = "Unknown";

        var state = manager.GetSerializedState();
        state.SymbolManagerID = symbolManagerID.ToByteString();        
        state.SymbolManagerName = symbolManagerName;
        state.PlayerID = playerID;
        state.ClientID = NetworkManager.ClientID;

        MineMapData data = GetMineMapData(symbolManagerID);

        data.SymbolManagerName = symbolManagerName;
        data.SymbolState = state;

        NetworkManager.SendNetMessage(VRNPacketType.SendSymbolManagerState, state);
    }

    private void Awake()
    {
        SymbolManagers = new Dictionary<Guid, MineMapData>();
        //_vrnSymbolManagerState = new VRNSymbolManagerState();
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        NetworkManager.RegisterHandler(VRNPacketType.SendSymbolManagerState, OnSendSymbolManagerState);
        NetworkManager.ClientRequestedWorldState += OnClientRequestedWorldState;

        SceneManager.sceneLoaded += OnSceneLoaded;

        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnClientRequestedWorldState(int obj)
    {
        if (NetworkManager.IsServer)
        {
            SendAllData();
        }
    }
    
    private void SendAllData()
    {
        foreach (var data in SymbolManagers.Values)
        {
            if (data.SymbolState != null)
            {
                Debug.Log($"MineMapManageR: Sending map data for {data.SymbolManagerID}");
                NetworkManager.SendNetMessage(VRNPacketType.SendSymbolManagerState, data.SymbolState);
            }
        }
    }

    private void OnDestroy()
    {
        NetworkManager.UnregisterHandler(VRNPacketType.SendSymbolManagerState, OnSendSymbolManagerState);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        var oldManagers = SymbolManagers;
        SymbolManagers = new Dictionary<Guid, MineMapData>();

        foreach (var kvp in oldManagers)
        {
            if (kvp.Value != null && kvp.Value.SymbolManager != null && kvp.Value.SymbolManager.gameObject != null)
            {
                Debug.Log($"MineMapManager: retained mine map symbol manager {kvp.Value.SymbolManager.gameObject.name} after scene change");
                SymbolManagers.Add(kvp.Key, kvp.Value);
            }
        }
        MapLoaded?.Invoke();
    }

    public void LoadSymbolManagerState(VRNSymbolManagerState state)
    {
        if (state.ClientID == NetworkManager.ClientID && !NetworkManager.IsPlaybackMode)
            return; //don't load state updates for our own maps

        MineMapData data = GetMineMapData(state.SymbolManagerID.ToGuid());

        //data.SymbolManagerName = state.SymbolManagerName;
        var player = PlayerManager.GetPlayer(state.PlayerID);
        if (player != null)
        {
            data.SymbolManagerName = player.Name;
        }
        else
        {
            data.SymbolManagerName = $"Mine Map {state.PlayerID}";
        }

        if (data.SymbolState != state)
        {
            //Debug.Log($"Loading new state for mine map {data.SymbolManagerID}");
            data.SymbolState = state;

            if (data.SymbolManager != null)
                data.SymbolManager.LoadFromSerializedState(data.SymbolState);
        }
    }

    private void OnSendSymbolManagerState(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var symbolState = VRNSymbolManagerState.Parser.ParseDelimitedFrom(recvStream);
        var symbolState = new VRNSymbolManagerState();
        reader.ReadMessage(symbolState);

        if (symbolState != null && symbolState.Symbols != null)
            Debug.Log($"Received symbol manager state count: {symbolState.Symbols.Count}");

        //Debug.Log($"MineMapManager: received mine map data for ID {symbolState.SymbolManagerID.ToGuid()}");

        LoadSymbolManagerState(symbolState);
    }
}
