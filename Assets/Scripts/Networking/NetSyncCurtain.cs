using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System;

//deprecated, functionality moved to HalfCurtainBehavior
[RequireComponent(typeof(NetworkedObject))]
public class NetSyncCurtain : MonoBehaviour//, INetSync
{
    /*
    public bool Grabbed = false;
    public Vector3 HangPosition;
    public PlayerManager PlayerManager;

    private Vector3 _targetPos;
    private VRNTransformData _xform;
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
        if (_xform == null)
        {
            _xform = new VRNTransformData();
            _xform.Position = new VRNVector3();
            _xform.Rotation = new VRNQuaternion();
        }

        reader.ReadMessage(_textMessage);
        reader.ReadMessage(_xform);

        _receivingData = true;
        bool state = false;
        //if (bool.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out state))
        if (bool.TryParse(_textMessage.Message, out state))
        {
            Grabbed = state;
        }
        else
        {
            Grabbed = false;
        }

        //_xform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        if (_xform.Position != null)
        {
            HangPosition = _xform.Position.ToVector3();
        }
        
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _receivingData = false;
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = Grabbed.ToString()
        //};        
        //text.WriteDelimitedTo(writer);
        //VRNTransformData tformData = new VRNTransformData
        //{
        //    Rotation = Quaternion.identity.ToVRNQuaternion(),
        //    Position = HangPosition.ToVRNVector3()
        //};
        //tformData.WriteDelimitedTo(writer);

        if (_textMessage == null)
            _textMessage = new VRNTextMessage();
        if (_xform == null)
        {
            _xform = new VRNTransformData();
        }

        _textMessage.Message = Grabbed.ToString();
        _xform.Position = HangPosition.ToVRNVector3();
        _xform.Rotation = Quaternion.identity.ToVRNQuaternion();

        writer.WriteMessage(_textMessage);
        writer.WriteMessage(_xform);
    }

    // Start is called before the first frame update
    void Start()
    {
        _xform = new VRNTransformData();
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
    }
        */
}
