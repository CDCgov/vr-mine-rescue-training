using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Google.Protobuf;

[System.Obsolete]
public class NetSyncGrabState : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    public bool Grabbed = false;

    private VRNTextMessage _VRText;
    private bool _receivingData = false;

    public void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
    }

    public bool NeedsUpdate()
    {
        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        //_receivingData = true;
        //bool state = false;
        //var txtMessage = VRNTextMessage.Parser.ParseDelimitedFrom(reader);
        //if (txtMessage != null && txtMessage.Message == "true")
        //    state = true;

        //try
        //{
        //    bool ignore = false;
        //    if(tag == "SoundingStick" || tag == "Chalk" || tag == "Belt")
        //    {
        //        ignore = true;
        //    }
        //    if (Grabbed != state && NetworkManager.IsRecording && !ignore)
        //    {
        //        var netObj = GetComponent<NetworkedObject>();
        //        var playerID = PlayerManager.LookupPlayerID(netObj.OwnerClientID);

        //        //if (state)
        //        //{
        //        //    NetworkManager.LogSessionEvent(new VRNLogEvent
        //        //    {
        //        //        EventType = VRNLogEventType.PickupObj,
        //        //        Position = transform.position.ToVRNVector3(),
        //        //        Rotation = transform.rotation.ToVRNQuaternion(),
        //        //        //PositionMetadata = "Stretcher", //Is on stretcher?
        //        //        ObjectType = VRNLogObjectType.Generic,
        //        //        ObjectName = gameObject.name,
        //        //        SourcePlayerID = playerID,
        //        //    });
        //        //}
        //        //else
        //        //{
        //        //    NetworkManager.LogSessionEvent(new VRNLogEvent
        //        //    {
        //        //        EventType = VRNLogEventType.DropObj,
        //        //        Position = transform.position.ToVRNVector3(),
        //        //        Rotation = transform.rotation.ToVRNQuaternion(),
        //        //        //PositionMetadata = "Stretcher", //Is on stretcher?
        //        //        ObjectType = VRNLogObjectType.Generic,
        //        //        ObjectName = gameObject.name,
        //        //        SourcePlayerID = playerID,
        //        //    });
        //        //}
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"NetSyncGrabState: Error writing log message {ex.Message}");
        //}

        //Grabbed = state;

    }

    public void WriteObjState(CodedOutputStream writer)
    {
        //_receivingData = false;
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = Grabbed ? "true" : "false",
        //};
        ////VRNTransformData xform = new VRNTransformData
        ////{
        ////    Rotation = transform.rotation.ToVRNQuaternion(),
        ////    Position = transform.position.ToVRNVector3(),
        ////};

        //text.WriteDelimitedTo(writer);
    }

    public void SetGrabbed(bool grabState)
    {
        Grabbed = grabState;
    }
}
