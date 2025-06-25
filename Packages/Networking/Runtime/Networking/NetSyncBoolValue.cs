using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Google.Protobuf;

public class NetSyncBoolValue : MonoBehaviour, INetSync
{
    public bool Value = false;

    private VRNTextMessage _textMessage;
    private bool _receivingData = false;

    public bool NeedsUpdate()
    {
        return true;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        _receivingData = true;

        reader.ReadMessage(_textMessage);

        bool state = false;
        //if (bool.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out state))
        if (bool.TryParse(_textMessage.Message, out state))
        {
            Value = state;
        }

    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_textMessage == null)
            _textMessage = new VRNTextMessage();

        _receivingData = false;

        _textMessage.Message = Value.ToString();

        writer.WriteMessage(_textMessage);

        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = Value.ToString()
        //};
        //VRNTransformData xform = new VRNTransformData
        //{
        //    Rotation = transform.rotation.ToVRNQuaternion(),
        //    Position = transform.position.ToVRNVector3(),
        //};

        //text.WriteDelimitedTo(writer);
    }

    public void SetValue(bool val)
    {
        Value = val;
    }
}
