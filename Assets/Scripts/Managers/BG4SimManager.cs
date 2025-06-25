using Google.Protobuf;
//using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BG4SimManager : SceneManagerBase
{
    public static BG4SimManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<BG4SimManager>("BG4SimManager");
    }

    public NetworkManager NetworkManager;

    public Action<VRNBG4SimData> SimDataChanged;

    public Dictionary<int, VRNBG4SimData> _bg4SimData;


    public void UpdateLocalBG4SimData(int playerID, VRNBG4SimData simData)
    {
        if (NetworkManager == null)
            return;

        simData.PlayerID = playerID;
        simData.ClientID = NetworkManager.ClientID;

        StoreSimData(simData);
        SimDataChanged?.Invoke(simData);
        NetworkManager.SendNetMessage(VRNPacketType.SendBg4SimData, simData);
    }

    public VRNBG4SimData GetSimData(int playerID)
    {
        if (_bg4SimData == null)
            return null;

        VRNBG4SimData simData = null;
        _bg4SimData.TryGetValue(playerID, out simData);

        return simData;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);        

        _bg4SimData = new Dictionary<int, VRNBG4SimData>();

        NetworkManager.RegisterHandler(VRNPacketType.SendBg4SimData, OnBG4SimData);

        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        NetworkManager.UnregisterHandler(VRNPacketType.SendBg4SimData, OnBG4SimData);
    }

    private void OnBG4SimData(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var bg4SimData = VRNBG4SimData.Parser.ParseDelimitedFrom(recvStream);
        VRNBG4SimData bg4SimData = new VRNBG4SimData();
        reader.ReadMessage(bg4SimData);

        StoreSimData(bg4SimData);

        SimDataChanged?.Invoke(bg4SimData);
    }

    private void StoreSimData(VRNBG4SimData simData)
    {
        if (simData.PlayerID < 0)
        {
            Debug.LogError("BG4Sim: Invalid PlayerID for BG4 SimData Update");
            return;
        }

        _bg4SimData[simData.PlayerID] = simData;
    }

}
