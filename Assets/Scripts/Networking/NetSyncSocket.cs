using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Google.Protobuf;

[RequireComponent(typeof(CustomXRSocket))]
//[RequireComponent(typeof(NetworkedObject))]
public class NetSyncSocket : MonoBehaviour
{
    public bool SmoothTranslationUpdate = true;
    public bool SocketActiveState = true;
    public string SocketedObjToSync = "";
    private VRNTransformData _xform;
    private VRNTextMessage _message;

    private Vector3 _targetPos;
    private Quaternion _targetRot;

    private bool _receivingData = false;
    private CustomXRSocket _customXRSocket;
    private bool _socketActive = false;
    //private NetworkedObject _netObj;

    //private float _velocity = 0;

    public bool NeedsUpdate()
    {
        return true;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _receivingData = true;
        //SocketedObjToSync = VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message;

        if (_message == null)
            _message = new VRNTextMessage();

        reader.ReadMessage(_message);

        SocketedObjToSync = _message.Message;

        //bool state = false;
        //if (bool.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out state))
        //{
        //    SocketActiveState = state;
        //}
        //if (SocketActiveState)
        //{
        //    _customXRSocket.IsActive = true;
        //}
        //else
        //{
        //    _customXRSocket.RemoveSocketedItem();
        //}
        //if (_xform == null)
        //    _xform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        //else
        //    _xform.MergeDelimitedFrom(reader);

        //if (_xform.Position != null)
        //{
        //    //transform.position = _xform.Position.ToVector3();
        //    _targetPos = _xform.Position.ToVector3();
        //}

        //if (_xform.Rotation != null)
        //{
        //    //transform.rotation = _xform.Rotation.ToQuaternion();
        //    _targetRot = _xform.Rotation.ToQuaternion();
        //}

    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _receivingData = false;
        if(SocketedObjToSync == null)
        {
            SocketedObjToSync = "";
        }
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    //Message = SocketActiveState.ToString()
        //    Message = SocketedObjToSync
        //};

        //text.WriteDelimitedTo(writer);

        if (_message == null)
            _message = new VRNTextMessage();

        _message.Message = SocketedObjToSync;
        writer.WriteMessage(_message);

        //if (_customXRSocket.SocketedInteractable != null)
        //{
        //    //VRNTransformData xform = new VRNTransformData
        //    //{
        //    //    Rotation = transform.rotation.ToVRNQuaternion(),
        //    //    Position = transform.position.ToVRNVector3(),
        //    //};

        //    //xform.WriteDelimitedTo(writer);
        //    NetworkedObject net = _customXRSocket.SocketedInteractable.GetComponent<NetworkedObject>();
        //    VRNTextMessage message = new VRNTextMessage
        //    {
        //        Message = net.UniqueIDString
        //    };
        //    message.WriteDelimitedTo(writer);
        //}
        //else
        //{
        //    VRNTextMessage message = new VRNTextMessage
        //    {
        //        Message = "Empty"
        //    };
        //    message.WriteDelimitedTo(writer);
        //}
    }

    void Start()
    {
        //_xform = new VRNTransformData();
        //_targetPos = transform.position;
        //_targetRot = transform.rotation;
        _customXRSocket = GetComponent<CustomXRSocket>();
        //CustomXRSocket.OnSocketAttach += CustomXRSocket_OnSocketAttach;
        //CustomXRSocket.OnSocketDetach += CustomXRSocket_OnSocketDetach;
        //_netObj = GetComponent<NetworkedObject>();
    }
        
    private void CustomXRSocket_OnSocketAttach(CustomXRSocket xrSocket)
    {
        if(xrSocket == _customXRSocket)
        {
            //Send network attach
            Debug.Log("Attach socket");
        }
    }

    private void CustomXRSocket_OnSocketDetach(CustomXRSocket xrSocket)
    {
        if(xrSocket == _customXRSocket)
        {
            //Send network detach
            Debug.Log("Detach socket");
        }
    }

    //void Update()
    //{
    //    if (_receivingData)
    //    {
    //        if (SmoothTranslationUpdate)
    //        {
    //            float dist = Vector3.Distance(transform.position, _targetPos);

    //            if (dist < 0.1)
    //            {
    //                transform.position = _targetPos;
    //            }
    //            else
    //            {
    //                //float vel = dist * 4; //cover the distance in 0.25 seconds
    //                //transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * vel);

    //                transform.position = Vector3.Lerp(transform.position, _targetPos, 0.5f);

    //            }
    //        }
    //        else
    //        {
    //            transform.position = _targetPos;
    //        }
    //        //transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRot, Time.deltaTime * 1200);
    //        transform.rotation = _targetRot;
    //    }
    //}
}
