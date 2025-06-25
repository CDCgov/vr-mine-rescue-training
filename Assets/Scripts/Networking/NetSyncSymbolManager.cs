using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;

[RequireComponent(typeof(MineMapSymbolManager))]
public class NetSyncSymbolManager : MonoBehaviour//, INetSync
{
    public MineMapManager MineMapManager;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    public Guid SymbolManagerID;
    public string SymbolManagerName;

    private MineMapSymbolManager _symbolManager;
    private bool _mapChanged = false;
    private float _lastMapSend = 0;
    private NetworkedObject _netObj;

    // Start is called before the first frame update
    void Start()
    {
        if (MineMapManager == null)
            MineMapManager = MineMapManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _symbolManager = GetComponent<MineMapSymbolManager>();

        _symbolManager.SymbolAdded += OnSymbolAdded;
        _symbolManager.SymbolRemoved += OnSymbolRemoved;

        _netObj = GetComponent<NetworkedObject>();
        if (_netObj == null)
            _netObj = GetComponentInParent<NetworkedObject>();

        if (_netObj != null)
            SymbolManagerID = _netObj.uniqueID;
        else
        {
            Debug.LogError($"NetSyncSymbolManager: Couldn't find NetworkedObject on network-synced mine map");
            SymbolManagerID = Guid.NewGuid();
        }

        MineMapManager.RegisterMineMap(SymbolManagerID, _symbolManager);

    }

    private void OnDestroy()
    {
        if (MineMapManager != null)
        {
            MineMapManager.UnregisterMineMap(SymbolManagerID);
        }
    }

    public void SetMapChanged()
    {
        _mapChanged = true;
    }

    private void OnSymbolRemoved(MineMapSymbol obj)
    {
        _mapChanged = true;
    }

    private void OnSymbolAdded(MineMapSymbol obj)
    {
        _mapChanged = true;
    }

    public bool NeedsUpdate()
    {
        float elapsed = Time.time - _lastMapSend;
        if (elapsed > 5)
            _mapChanged = true;

        return _mapChanged;
    }

    private void Update()
    {
        if (_mapChanged && _netObj != null && _netObj.HasAuthority)
        {
            var elapsed = Time.time - _lastMapSend;
            if (elapsed < 1.5f)
                return;

            _mapChanged = false;
            _lastMapSend = Time.time;

            int playerID = -1;

            if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
            {
                SymbolManagerName = PlayerManager.CurrentPlayer.Name;
                playerID = PlayerManager.CurrentPlayer.PlayerID;
            }
            else
                SymbolManagerName = null;

            MineMapManager.SendSymbolManagerState(SymbolManagerID, SymbolManagerName, playerID, _symbolManager);
        }
    }

    /*

    public void WriteObjState(Stream writer)
    {
        //var codedStream = new CodedOutputStream(writer);
        //codedStream.WriteBool(_mapChanged);

        var bwriter = new BinaryWriter(writer);
        bwriter.Write((bool)_mapChanged);

        //for now just send the entire map if it changes
        //future work - add more control messages (add/remove/update) and
        //a way to determine if the entire map needs sent (new player, etc.)

        if (_mapChanged)
        {
            _mapChanged = false;
            _lastMapSend = Time.time;
            Debug.Log("SymbolManagerSync: Sending symbol manager state");

            VRNSymbolManagerState state = _symbolManager.GetSerializedState();
            //codedStream.WriteMessage(state);
            state.WriteDelimitedTo(writer);
        }        
    }

    public void SyncObjState(Stream reader)
    {
        //var codedStream = new CodedInputStream(reader);
        //bool mapChanged = codedStream.ReadBool();

        var breader = new BinaryReader(reader);
        var mapChanged = breader.ReadBoolean();

        if (mapChanged)
        {
            //var state = VRNSymbolManagerState.Parser.ParseFrom(codedStream);
            var state = VRNSymbolManagerState.Parser.ParseDelimitedFrom(reader);
            state.SymbolManagerName = "";

            Debug.Log("SymbolManagerSync: Received symbol manager state");

            _symbolManager.LoadFromSerializedState(state);
        }
    }
    */
}
