using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Google.Protobuf;

[RequireComponent(typeof(NetworkedObject))]
public class NetSyncID : MonoBehaviour, INetSync
{
    public int ID;

    private bool _receivingData = false;
    private NetworkedObject _netObj;
    private VRNTextMessage _textMessage;

    private void Start()
    {
        _netObj = GetComponent<NetworkedObject>();
        ID = _netObj.OwnerClientID;
    } 

    public bool NeedsUpdate()
    {
        return true;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        reader.ReadMessage(_textMessage);

        _receivingData = true;
        int input;
        //if(int.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out input))
        if(int.TryParse(_textMessage.Message, out input))
        {
            ID = input;
        }
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        _receivingData = false;

        _textMessage.Message = _netObj.OwnerClientID.ToString();
        writer.WriteMessage(_textMessage);

        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = _netObj.OwnerClientID.ToString()
        //};        
        //text.WriteDelimitedTo(writer);
    }

    
}
