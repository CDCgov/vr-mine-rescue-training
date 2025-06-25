using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Google.Protobuf;

public class NetSyncFloatValue : MonoBehaviour, INetSync
{
    public float ValueToSync;

    private VRNTextMessage _textMessage;

    public bool NeedsUpdate()
    {
        return true;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        float toSync = -1;
        //if (float.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out toSync))
        //{
        //    ValueToSync = toSync;
        //}

        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        _textMessage.Message = null;
        reader.ReadMessage(_textMessage);
        if (float.TryParse(_textMessage.Message, out toSync))
        {
            ValueToSync = toSync;
        }
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = ValueToSync.ToString()
        //};
        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        _textMessage.Message = ValueToSync.ToString();
        //text.WriteDelimitedTo(writer);
        writer.WriteMessage(_textMessage);
    }
}
